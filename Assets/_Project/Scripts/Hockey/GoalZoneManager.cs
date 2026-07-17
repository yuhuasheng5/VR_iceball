using System;
using System.Collections;
using UnityEngine;

public class GoalZoneManager : MonoBehaviour
{
    public static GoalZoneManager Instance { get; private set; }

    [Header("Reset")]
    [SerializeField] private float resetDelay = 0.35f;

    [Header("Stats")]
    [SerializeField] private bool registerGoalStats = true;

    [Header("Game State")]
    [SerializeField] private bool requirePlayingState = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private bool canScore = true;

    public event Action<string, int> OnZoneScored;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TryScore(PuckController puckController, string zoneName, int scoreValue)
    {
        if (!canScore)
        {
            if (showDebugLog)
            {
                Debug.Log("不能加分：canScore 为 false，正在等待冰球重置");
            }

            return;
        }

        if (puckController == null)
        {
            if (showDebugLog)
            {
                Debug.LogWarning("不能加分：没有找到 PuckController");
            }

            return;
        }

        if (requirePlayingState)
        {
            if (GameManager.Instance == null)
            {
                if (showDebugLog)
                {
                    Debug.LogWarning("不能加分：没有找到 GameManager.Instance");
                }

                return;
            }

            if (GameManager.Instance.CurrentState != GameState.Playing)
            {
                if (showDebugLog)
                {
                    Debug.LogWarning("不能加分：当前状态不是 Playing，当前状态是 " + GameManager.Instance.CurrentState);
                }

                return;
            }
        }

        StartCoroutine(HandleScore(puckController, zoneName, scoreValue));
    }

    private IEnumerator HandleScore(PuckController puckController, string zoneName, int scoreValue)
    {
        canScore = false;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }
        else
        {
            Debug.LogWarning("ScoreManager.Instance 为空，无法加分");
        }

        if (registerGoalStats && ShotStatsManager.Instance != null)
        {
            ShotStatsManager.Instance.RegisterGoal();
        }

        OnZoneScored?.Invoke(zoneName, scoreValue);

        if (showDebugLog)
        {
            Debug.Log("命中区域：" + zoneName + "，得分：" + scoreValue);
        }

        yield return new WaitForSeconds(resetDelay);

        puckController.ResetPuck();

        yield return new WaitForSeconds(0.25f);

        canScore = true;
    }
}