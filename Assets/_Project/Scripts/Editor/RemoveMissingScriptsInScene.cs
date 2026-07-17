using UnityEditor;
using UnityEngine;

public class RemoveMissingScriptsInScene
{
    [MenuItem("Tools/Clean Missing Scripts In Current Scene")]
    public static void CleanMissingScripts()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>(true);

        int totalRemoved = 0;

        foreach (GameObject go in allObjects)
        {
            int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

            if (removedCount > 0)
            {
                totalRemoved += removedCount;
                Debug.LogWarning("已清理丢失脚本：" + GetFullPath(go) + "，数量：" + removedCount, go);
            }
        }

        Debug.Log("当前场景丢失脚本清理完成，总数量：" + totalRemoved);
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}