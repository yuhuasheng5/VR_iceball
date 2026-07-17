using Fusion;
using UnityEngine;

public class OnlineMatchSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public NetworkObject onlineMatchManagerPrefab;

    [Header("References")]
    public Transform puckCenterPoint;

    [Header("Debug")]
    public bool showDebugLog = true;

    private bool hasSpawnedMatchManager;

    public void SpawnMatchManagerIfMasterClient(NetworkRunner runner)
    {
        if (runner == null)
        {
            Debug.LogError("生成比赛管理器失败：NetworkRunner 为空");
            return;
        }

        if (hasSpawnedMatchManager)
        {
            return;
        }

        if (!runner.IsSharedModeMasterClient)
        {
            if (showDebugLog)
            {
                Debug.Log("当前不是 Shared Mode Master Client，不生成 OnlineMatchManager");
            }

            return;
        }

        if (onlineMatchManagerPrefab == null)
        {
            Debug.LogError("生成比赛管理器失败：OnlineMatchManager Prefab 没有绑定");
            return;
        }

        NetworkObject managerObject = runner.Spawn(
            onlineMatchManagerPrefab,
            Vector3.zero,
            Quaternion.identity,
            runner.LocalPlayer
        );

        if (managerObject == null)
        {
            Debug.LogError("生成比赛管理器失败：runner.Spawn 返回 null");
            return;
        }

        OnlineMatchManager matchManager = managerObject.GetComponent<OnlineMatchManager>();

        if (matchManager != null)
        {
            matchManager.puckCenterPoint = puckCenterPoint;
        }

        managerObject.gameObject.name = "OnlineMatchManager";

        hasSpawnedMatchManager = true;

        if (showDebugLog)
        {
            Debug.Log("OnlineMatchManager 生成成功");
        }
    }
}