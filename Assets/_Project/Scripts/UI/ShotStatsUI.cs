using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShotStatsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Text statsText;

    private IEnumerator Start()
    {
        if (statsText == null)
        {
            statsText = GetComponent<Text>();
        }

        while (ShotStatsManager.Instance == null)
        {
            yield return null;
        }

        ShotStatsManager.Instance.OnStatsChanged += UpdateStatsText;

        UpdateStatsText(
            ShotStatsManager.Instance.ShotCount,
            ShotStatsManager.Instance.GoalCount,
            ShotStatsManager.Instance.Accuracy
        );
    }

    private void OnDestroy()
    {
        if (ShotStatsManager.Instance != null)
        {
            ShotStatsManager.Instance.OnStatsChanged -= UpdateStatsText;
        }
    }

    private void UpdateStatsText(int shotCount, int goalCount, float accuracy)
    {
        if (statsText == null)
        {
            return;
        }

        int accuracyPercent = Mathf.RoundToInt(accuracy * 100f);

        statsText.text =
            "射门：" + shotCount +
            "  命中：" + goalCount +
            "  命中率：" + accuracyPercent + "%";
    }
}