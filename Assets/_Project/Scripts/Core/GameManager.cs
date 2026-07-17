using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PuckController puckController;
    [SerializeField] private HockeyStickTwoHandController hockeyStickController;
    [SerializeField] private GoalkeeperAI goalkeeperAI;

    public GameState CurrentState { get; private set; } = GameState.Ready;

    public event Action<GameState> OnGameStateChanged;

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
        ChangeState(GameState.Ready);
    }

    public void StartGame()
    {
        ResetGameObjects();

        ChangeState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
    }

    public void EndGame()
    {
        if (CurrentState == GameState.GameOver)
        {
            return;
        }

        ChangeState(GameState.GameOver);
    }

    public void RestartGame()
    {
        ResetGameObjects();

        ChangeState(GameState.Playing);
    }

    public void ReturnToReady()
    {
        ResetGameObjects();

        ChangeState(GameState.Ready);
    }

    private void ResetGameObjects()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        if (ShotStatsManager.Instance != null)
        {
            ShotStatsManager.Instance.ResetStats();
        }

        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.ResetTimer();
        }

        if (puckController != null)
        {
            puckController.ResetPuck();
        }

        if (hockeyStickController != null)
        {
            hockeyStickController.ResetStick();
        }

        if (goalkeeperAI != null)
        {
            goalkeeperAI.ResetGoalkeeper();
        }
    }

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        Debug.Log("Game State Changed To: " + newState);
    }
}