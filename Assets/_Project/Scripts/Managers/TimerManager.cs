using System;
using System.Collections;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Timer Settings")]
    [SerializeField] private float startTime = 60f;

    public float RemainingTime { get; private set; }

    public event Action<float> OnTimeChanged;

    private bool isRunning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private IEnumerator Start()
    {
        ResetTimer();

        while (GameManager.Instance == null)
        {
            yield return null;
        }

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(GameManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        RemainingTime -= Time.deltaTime;

        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;
            isRunning = false;

            OnTimeChanged?.Invoke(RemainingTime);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame();
            }

            return;
        }

        OnTimeChanged?.Invoke(RemainingTime);
    }

    public void ResetTimer()
    {
        RemainingTime = startTime;
        OnTimeChanged?.Invoke(RemainingTime);
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Ready:
                isRunning = false;
                ResetTimer();
                break;

            case GameState.Playing:
                isRunning = true;
                break;

            case GameState.Paused:
                isRunning = false;
                break;

            case GameState.GameOver:
                isRunning = false;
                break;
        }
    }
}