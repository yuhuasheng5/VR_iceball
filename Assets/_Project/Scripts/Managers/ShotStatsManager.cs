using System;
using UnityEngine;

public class ShotStatsManager : MonoBehaviour
{
    public static ShotStatsManager Instance { get; private set; }

    [Header("Runtime Stats")]
    [SerializeField] private int shotCount;
    [SerializeField] private int goalCount;
    [SerializeField] private float accuracyPercent;
    [SerializeField] private string rating = "C";

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    public int ShotCount => shotCount;
    public int GoalCount => goalCount;

    public float Accuracy
    {
        get
        {
            if (shotCount <= 0)
            {
                return 0f;
            }

            return (float)goalCount / shotCount;
        }
    }

    public event Action<int, int, float> OnStatsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResetStats();
    }

    public void RegisterShot()
    {
        shotCount++;
        RefreshStats();

        if (showDebugLog)
        {
            Debug.Log("射门次数：" + shotCount);
        }
    }

    public void RegisterGoal()
    {
        goalCount++;
        RefreshStats();

        if (showDebugLog)
        {
            Debug.Log("命中次数：" + goalCount);
        }
    }

    public void ResetStats()
    {
        shotCount = 0;
        goalCount = 0;
        RefreshStats();
    }

    public string GetRating()
    {
        if (shotCount <= 0)
        {
            return "C";
        }

        float accuracy = Accuracy;

        if (accuracy >= 0.8f)
        {
            return "S";
        }

        if (accuracy >= 0.6f)
        {
            return "A";
        }

        if (accuracy >= 0.4f)
        {
            return "B";
        }

        return "C";
    }

    private void RefreshStats()
    {
        accuracyPercent = Mathf.RoundToInt(Accuracy * 100f);
        rating = GetRating();

        OnStatsChanged?.Invoke(shotCount, goalCount, Accuracy);
    }
}