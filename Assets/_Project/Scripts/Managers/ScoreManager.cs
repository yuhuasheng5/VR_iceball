using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Runtime")]
    [SerializeField] private int score;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    public int Score => score;

    public event Action<int> OnScoreChanged;

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
        ResetScore();
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);

        if (showDebugLog)
        {
            Debug.Log("当前得分：" + score);
        }
    }

    public void ResetScore()
    {
        score = 0;
        OnScoreChanged?.Invoke(score);

        if (showDebugLog)
        {
            Debug.Log("分数已重置");
        }
    }
}