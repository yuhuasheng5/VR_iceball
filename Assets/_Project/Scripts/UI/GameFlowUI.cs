using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFlowUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private GameObject gameHUD;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button returnMenuButton;

    [Header("Texts")]
    [SerializeField] private Text resultText;

    [Header("Result Settings")]
    [SerializeField] private bool showShotStatsInResult = true;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "Scene_MainMenu";

    private IEnumerator Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (returnMenuButton != null)
        {
            returnMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

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

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }

        if (returnMenuButton != null)
        {
            returnMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        }
    }

    private void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    private void RestartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void ReturnToMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("主菜单场景名为空，无法返回主菜单");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (readyPanel != null)
        {
            readyPanel.SetActive(state == GameState.Ready);
        }

        if (gameHUD != null)
        {
            gameHUD.SetActive(state == GameState.Playing);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(state == GameState.GameOver);
        }

        if (state == GameState.GameOver)
        {
            UpdateResultText();
        }
    }

    private void UpdateResultText()
    {
        if (resultText == null)
        {
            return;
        }

        int finalScore = 0;

        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.Score;
        }

        if (!showShotStatsInResult)
        {
            resultText.text =
                "训练结束\n" +
                "最终得分：" + finalScore;

            return;
        }

        int shotCount = 0;
        int goalCount = 0;
        int accuracyPercent = 0;
        string rating = "C";

        if (ShotStatsManager.Instance != null)
        {
            shotCount = ShotStatsManager.Instance.ShotCount;
            goalCount = ShotStatsManager.Instance.GoalCount;
            accuracyPercent = Mathf.RoundToInt(ShotStatsManager.Instance.Accuracy * 100f);
            rating = ShotStatsManager.Instance.GetRating();
        }

        resultText.text =
            "训练结束\n" +
            "最终得分：" + finalScore + "\n" +
            "射门次数：" + shotCount + "\n" +
            "命中次数：" + goalCount + "\n" +
            "命中率：" + accuracyPercent + "%\n" +
            "训练评级：" + rating;
    }
}