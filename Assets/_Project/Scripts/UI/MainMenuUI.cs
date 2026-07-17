using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject modeSelectPanel;
    [SerializeField] private GameObject difficultyPanel;

    [Header("Start Panel Buttons")]
    [SerializeField] private Button startButton;

    [Header("Mode Panel Buttons")]
    [SerializeField] private Button timeChallengeButton;
    [SerializeField] private Button aiTrainingButton;
    [SerializeField] private Button modeBackButton;

    [Header("Difficulty Panel Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button difficultyBackButton;

    [Header("Scene Names")]
    [SerializeField] private string timeChallengeSceneName = "Scene_TimeChallenge";
    [SerializeField] private string aiEasySceneName = "Scene_AITraining_Easy";
    [SerializeField] private string aiNormalSceneName = "Scene_AITraining_Normal";
    [SerializeField] private string aiHardSceneName = "Scene_AITraining_Hard";

    private void Start()
    {
        BindButtons();
        ShowStartPanel();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void BindButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(ShowModeSelectPanel);
        }

        if (timeChallengeButton != null)
        {
            timeChallengeButton.onClick.AddListener(LoadTimeChallenge);
        }

        if (aiTrainingButton != null)
        {
            aiTrainingButton.onClick.AddListener(ShowDifficultyPanel);
        }

        if (modeBackButton != null)
        {
            modeBackButton.onClick.AddListener(ShowStartPanel);
        }

        if (easyButton != null)
        {
            easyButton.onClick.AddListener(LoadAIEasy);
        }

        if (normalButton != null)
        {
            normalButton.onClick.AddListener(LoadAINormal);
        }

        if (hardButton != null)
        {
            hardButton.onClick.AddListener(LoadAIHard);
        }

        if (difficultyBackButton != null)
        {
            difficultyBackButton.onClick.AddListener(ShowModeSelectPanel);
        }
    }

    private void UnbindButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(ShowModeSelectPanel);
        }

        if (timeChallengeButton != null)
        {
            timeChallengeButton.onClick.RemoveListener(LoadTimeChallenge);
        }

        if (aiTrainingButton != null)
        {
            aiTrainingButton.onClick.RemoveListener(ShowDifficultyPanel);
        }

        if (modeBackButton != null)
        {
            modeBackButton.onClick.RemoveListener(ShowStartPanel);
        }

        if (easyButton != null)
        {
            easyButton.onClick.RemoveListener(LoadAIEasy);
        }

        if (normalButton != null)
        {
            normalButton.onClick.RemoveListener(LoadAINormal);
        }

        if (hardButton != null)
        {
            hardButton.onClick.RemoveListener(LoadAIHard);
        }

        if (difficultyBackButton != null)
        {
            difficultyBackButton.onClick.RemoveListener(ShowModeSelectPanel);
        }
    }

    private void ShowStartPanel()
    {
        SetPanel(startPanel, true);
        SetPanel(modeSelectPanel, false);
        SetPanel(difficultyPanel, false);
    }

    private void ShowModeSelectPanel()
    {
        SetPanel(startPanel, false);
        SetPanel(modeSelectPanel, true);
        SetPanel(difficultyPanel, false);
    }

    private void ShowDifficultyPanel()
    {
        SetPanel(startPanel, false);
        SetPanel(modeSelectPanel, false);
        SetPanel(difficultyPanel, true);
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void LoadTimeChallenge()
    {
        LoadScene(timeChallengeSceneName);
    }

    private void LoadAIEasy()
    {
        LoadScene(aiEasySceneName);
    }

    private void LoadAINormal()
    {
        LoadScene(aiNormalSceneName);
    }

    private void LoadAIHard()
    {
        LoadScene(aiHardSceneName);
    }

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("场景名为空，无法跳转");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}