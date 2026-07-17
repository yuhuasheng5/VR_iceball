using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private Text timerText;

    private IEnumerator Start()
    {
        if (timerText == null)
        {
            timerText = GetComponent<Text>();
        }

        while (TimerManager.Instance == null)
        {
            yield return null;
        }

        TimerManager.Instance.OnTimeChanged += UpdateTimerText;
        UpdateTimerText(TimerManager.Instance.RemainingTime);
    }

    private void OnDestroy()
    {
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.OnTimeChanged -= UpdateTimerText;
        }
    }

    private void UpdateTimerText(float time)
    {
        if (timerText == null)
        {
            return;
        }

        int displayTime = Mathf.CeilToInt(time);
        timerText.text = "时间：" + displayTime;
    }
}