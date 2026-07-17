using Fusion;
using UnityEngine;

public class OnlineMatchManager : NetworkBehaviour
{
    public static OnlineMatchManager Instance { get; private set; }

    [Header("References")]
    public Transform puckCenterPoint;

    [Header("Match Settings")]
    public float matchDuration = 180f;
    public float scoreCooldown = 1.0f;
    public int requiredPlayerCount = 2;

    [Header("Debug")]
    public bool showDebugLog = true;

    [Networked] public int Player1Score { get; private set; }
    [Networked] public int Player2Score { get; private set; }

    [Networked] public float RemainingTime { get; private set; }
    [Networked] public bool MatchRunning { get; private set; }
    [Networked] public bool MatchEnded { get; private set; }
    [Networked] public int ConnectedPlayerCount { get; private set; }

    private float lastScoreTime;
    private bool hasStartedOnce;

    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        Instance = this;

        if (Object.HasStateAuthority)
        {
            Player1Score = 0;
            Player2Score = 0;

            RemainingTime = matchDuration;
            MatchRunning = false;
            MatchEnded = false;
            ConnectedPlayerCount = 0;
            hasStartedOnce = false;
        }

        if (showDebugLog)
        {
            Debug.Log("OnlineMatchManager Spawned. StateAuthority: " + Object.HasStateAuthority);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        UpdateConnectedPlayerCountByAvatars();

        if (!hasStartedOnce)
        {
            TryStartMatchWhenEnoughPlayers();
        }

        if (!MatchRunning || MatchEnded)
        {
            return;
        }

        RemainingTime -= Runner.DeltaTime;

        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            MatchRunning = false;
            MatchEnded = true;

            if (showDebugLog)
            {
                Debug.Log("比赛结束。最终比分：" + Player1Score + " : " + Player2Score);
            }
        }
    }

    private void UpdateConnectedPlayerCountByAvatars()
    {
        NetworkPlayerAvatar[] avatars = FindObjectsOfType<NetworkPlayerAvatar>();
        ConnectedPlayerCount = avatars.Length;

        if (showDebugLog)
        {
            Debug.Log("当前网络玩家对象数量：" + ConnectedPlayerCount);
        }
    }

    private void TryStartMatchWhenEnoughPlayers()
    {
        if (ConnectedPlayerCount < requiredPlayerCount)
        {
            MatchRunning = false;
            MatchEnded = false;
            RemainingTime = matchDuration;
            return;
        }

        hasStartedOnce = true;
        MatchRunning = true;
        MatchEnded = false;
        RemainingTime = matchDuration;

        if (showDebugLog)
        {
            Debug.Log("玩家人数已满足，比赛开始。当前人数：" + ConnectedPlayerCount);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GoalScored(int scoringPlayerNumber, NetworkId puckNetworkId)
    {
        if (!MatchRunning || MatchEnded)
        {
            return;
        }

        if (Time.time - lastScoreTime < scoreCooldown)
        {
            return;
        }

        lastScoreTime = Time.time;

        if (scoringPlayerNumber == 1)
        {
            Player1Score++;
        }
        else if (scoringPlayerNumber == 2)
        {
            Player2Score++;
        }

        if (showDebugLog)
        {
            Debug.Log("进球！玩家 " + scoringPlayerNumber + " 得分。比分：" + Player1Score + " : " + Player2Score);
        }

        ResetPuckById(puckNetworkId);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RestartMatch(NetworkId puckNetworkId)
    {
        Player1Score = 0;
        Player2Score = 0;

        RemainingTime = matchDuration;
        MatchEnded = false;

        UpdateConnectedPlayerCountByAvatars();

        if (ConnectedPlayerCount >= requiredPlayerCount)
        {
            hasStartedOnce = true;
            MatchRunning = true;
        }
        else
        {
            hasStartedOnce = false;
            MatchRunning = false;
        }

        ResetPuckById(puckNetworkId);

        if (showDebugLog)
        {
            Debug.Log(
                "重新开始比赛。当前人数：" +
                ConnectedPlayerCount +
                "，MatchRunning：" +
                MatchRunning
            );
        }
    }

    private void ResetPuckById(NetworkId puckNetworkId)
    {
        NetworkObject puckObject = Runner.FindObject(puckNetworkId);

        if (puckObject == null)
        {
            Debug.LogWarning("没有找到网络冰球，无法重置");
            return;
        }

        NetworkPuckPhysics puck = puckObject.GetComponent<NetworkPuckPhysics>();

        if (puck == null)
        {
            Debug.LogWarning("找到冰球对象，但没有 NetworkPuckPhysics");
            return;
        }

        Vector3 resetPosition = puckCenterPoint != null
            ? puckCenterPoint.position
            : new Vector3(0f, 0.15f, 2f);

        puck.RPC_ResetPuck(resetPosition);
    }
}