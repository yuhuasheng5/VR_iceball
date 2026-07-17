using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class OnlineTimerUI : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private string prefix = "时间：";
    [SerializeField] private string waitingPrefix = "等待玩家：";
    [SerializeField] private string endedText = "比赛结束";

    private Text timerText;

    private int lastDisplayedSeconds = -1;
    private int lastPlayerCount = -1;
    private bool lastRunningState;
    private bool lastEndedState;

    private void Awake()
    {
        timerText = GetComponent<Text>();
    }

    private void Update()
    {
        if (timerText == null)
        {
            timerText = GetComponent<Text>();
        }

        OnlineMatchManager matchManager = OnlineMatchManager.Instance;

        if (matchManager == null)
        {
            timerText.text = "等待比赛管理器";
            return;
        }

        if (matchManager.MatchEnded)
        {
            if (!lastEndedState)
            {
                timerText.text = endedText;
            }

            lastEndedState = true;
            return;
        }

        lastEndedState = false;

        if (!matchManager.MatchRunning)
        {
            if (lastPlayerCount != matchManager.ConnectedPlayerCount ||
                lastRunningState != matchManager.MatchRunning)
            {
                timerText.text =
                    waitingPrefix +
                    matchManager.ConnectedPlayerCount +
                    "/" +
                    matchManager.requiredPlayerCount;
            }

            lastPlayerCount = matchManager.ConnectedPlayerCount;
            lastRunningState = matchManager.MatchRunning;
            return;
        }

        lastRunningState = matchManager.MatchRunning;

        int totalSeconds = Mathf.CeilToInt(matchManager.RemainingTime);

        if (totalSeconds == lastDisplayedSeconds)
        {
            return;
        }

        lastDisplayedSeconds = totalSeconds;

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = prefix + minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}