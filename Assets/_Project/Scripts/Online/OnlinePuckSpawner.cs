using Fusion;
using UnityEngine;

public class OnlinePuckSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public NetworkObject networkPuckPrefab;

    [Header("Spawn Point")]
    public Transform puckSpawnPoint;

    [Header("Debug")]
    public bool showDebugLog = true;

    private bool hasSpawnedPuck;

    public void SpawnPuckIfMasterClient(NetworkRunner runner)
    {
        if (runner == null)
        {
            Debug.LogError("生成冰球失败：NetworkRunner 为空");
            return;
        }

        if (hasSpawnedPuck)
        {
            Debug.Log("已经生成过冰球，不重复生成");
            return;
        }

        if (networkPuckPrefab == null)
        {
            Debug.LogError("生成冰球失败：Network Puck Prefab 没有绑定");
            return;
        }

        if (!runner.IsSharedModeMasterClient)
        {
            if (showDebugLog)
            {
                Debug.Log("当前不是 Shared Mode Master Client，不生成冰球。当前玩家：" + runner.LocalPlayer);
            }

            return;
        }

        Vector3 spawnPosition = puckSpawnPoint != null
            ? puckSpawnPoint.position
            : new Vector3(0f, 0.15f, 4f);

        Quaternion spawnRotation = puckSpawnPoint != null
            ? puckSpawnPoint.rotation
            : Quaternion.identity;

        Debug.Log("准备生成网络冰球，位置：" + spawnPosition);

        NetworkObject puckObject = runner.Spawn(
            networkPuckPrefab,
            spawnPosition,
            spawnRotation,
            runner.LocalPlayer
        );

        if (puckObject == null)
        {
            Debug.LogError("生成冰球失败：runner.Spawn 返回 null");
            return;
        }

        puckObject.gameObject.name = "NetworkPuck";

        hasSpawnedPuck = true;

        if (showDebugLog)
        {
            Debug.Log("网络冰球生成成功：" + puckObject.gameObject.name);
        }
    }
}