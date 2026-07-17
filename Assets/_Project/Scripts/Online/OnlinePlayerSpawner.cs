using Fusion;
using UnityEngine;

public class OnlinePlayerSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public NetworkObject networkPlayerPrefab;

    [Header("Spawn Points")]
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    [Header("Debug")]
    public bool showDebugLog = true;

    private bool hasSpawnedLocalPlayer;

    public void SpawnLocalPlayer(NetworkRunner runner)
    {
        if (hasSpawnedLocalPlayer)
        {
            Debug.LogWarning("已经生成过本地玩家，不重复生成");
            return;
        }

        if (runner == null)
        {
            Debug.LogError("生成失败：NetworkRunner 为空");
            return;
        }

        if (networkPlayerPrefab == null)
        {
            Debug.LogError("生成失败：Network Player Prefab 没有绑定");
            return;
        }

        PlayerRef localPlayer = runner.LocalPlayer;

        Transform spawnPoint = GetSpawnPoint(localPlayer);

        Vector3 spawnPosition = spawnPoint != null
            ? spawnPoint.position
            : new Vector3(0f, 1.5f, 3f);

        Quaternion spawnRotation = spawnPoint != null
            ? spawnPoint.rotation
            : Quaternion.identity;

        Debug.Log("准备生成网络玩家，Prefab = " + networkPlayerPrefab.name);
        Debug.Log("生成位置 = " + spawnPosition);
        Debug.Log("Local Player = " + localPlayer);

        NetworkObject playerObject = runner.Spawn(
            networkPlayerPrefab,
            spawnPosition,
            spawnRotation,
            localPlayer
        );

        if (playerObject == null)
        {
            Debug.LogError("生成失败：runner.Spawn 返回 null");
            return;
        }

        playerObject.gameObject.name = "NetworkPlayer_" + localPlayer;

        playerObject.transform.SetParent(null);
        playerObject.transform.position = spawnPosition;
        playerObject.transform.rotation = spawnRotation;
        playerObject.transform.localScale = Vector3.one;

        hasSpawnedLocalPlayer = true;

        Debug.Log("生成成功：" + playerObject.gameObject.name);
    }

    private Transform GetSpawnPoint(PlayerRef player)
    {
        if (player.RawEncoded % 2 == 0)
        {
            return player1SpawnPoint;
        }

        return player2SpawnPoint;
    }
}