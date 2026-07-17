using Fusion;
using UnityEngine;

public class OnlineGoalTrigger : MonoBehaviour
{
    [Header("Goal Settings")]
    public int scoringPlayerNumber = 1;
    public float triggerCooldown = 1.0f;

    [Header("Debug")]
    public bool showDebugLog = true;

    private float lastTriggerTime;

    private void OnTriggerEnter(Collider other)
    {
        TryScore(other, "Enter");
    }

    private void OnTriggerStay(Collider other)
    {
        TryScore(other, "Stay");
    }

    private void TryScore(Collider other, string triggerType)
    {
        if (Time.time - lastTriggerTime < triggerCooldown)
        {
            return;
        }

        NetworkPuckPhysics puck = other.GetComponentInParent<NetworkPuckPhysics>();

        if (puck == null)
        {
            return;
        }

        NetworkObject puckNetworkObject = puck.GetComponent<NetworkObject>();

        if (puckNetworkObject == null)
        {
            Debug.LogWarning("GoalTrigger 检测到冰球，但冰球没有 NetworkObject");
            return;
        }

        if (showDebugLog)
        {
            Debug.Log(
                "GoalTrigger " +
                triggerType +
                " 检测到冰球。Puck StateAuthority = " +
                puckNetworkObject.HasStateAuthority
            );
        }

        // 只让冰球 StateAuthority 那一端真正加分，避免两个客户端重复加分
        if (!puckNetworkObject.HasStateAuthority)
        {
            return;
        }

        if (OnlineMatchManager.Instance == null)
        {
            Debug.LogWarning("没有找到 OnlineMatchManager，无法加分");
            return;
        }

        OnlineMatchManager.Instance.RPC_GoalScored(
            scoringPlayerNumber,
            puckNetworkObject.Id
        );

        lastTriggerTime = Time.time;

        if (showDebugLog)
        {
            Debug.Log("触发进球区域，得分玩家：" + scoringPlayerNumber);
        }
    }
}