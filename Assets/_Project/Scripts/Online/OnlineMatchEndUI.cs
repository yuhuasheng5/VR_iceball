using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnlineMatchEndUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject matchEndPanel;
    public GameObject matchHUDCanvas;

    [Header("UI")]
    public Text resultText;
    public Button restartButton;
    public Button returnMainMenuButton;

    [Header("Scene")]
    public string mainMenuSceneName = "Scene_MainMenu";

    [Header("Result Text")]
    public string drawText = "平局";
    public string player1WinText = "玩家1 获胜";
    public string player2WinText = "玩家2 获胜";

    private bool lastMatchEnded;

    private void Start()
    {
        if (matchEndPanel != null)
        {
            matchEndPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartMatch);
        }
        else
        {
            Debug.LogWarning("RestartButton 没有绑定");
        }

        if (returnMainMenuButton != null)
        {
            returnMainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        else
        {
            Debug.LogWarning("ReturnMainMenuButton 没有绑定");
        }
    }

    private void OnDestroy()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartMatch);
        }

        if (returnMainMenuButton != null)
        {
            returnMainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        }
    }

    private void Update()
    {
        OnlineMatchManager matchManager = OnlineMatchManager.Instance;

        if (matchManager == null)
        {
            return;
        }

        if (matchManager.MatchEnded == lastMatchEnded)
        {
            return;
        }

        lastMatchEnded = matchManager.MatchEnded;

        if (matchManager.MatchEnded)
        {
            ShowMatchEndPanel(matchManager);
        }
        else
        {
            HideMatchEndPanel();
        }
    }

    private void ShowMatchEndPanel(OnlineMatchManager matchManager)
    {
        if (matchHUDCanvas != null)
        {
            matchHUDCanvas.SetActive(false);
        }

        if (matchEndPanel != null)
        {
            matchEndPanel.SetActive(true);
        }

        if (resultText == null)
        {
            return;
        }

        string winnerText;

        if (matchManager.Player1Score > matchManager.Player2Score)
        {
            winnerText = player1WinText;
        }
        else if (matchManager.Player2Score > matchManager.Player1Score)
        {
            winnerText = player2WinText;
        }
        else
        {
            winnerText = drawText;
        }

        resultText.text =
            "比赛结束\n" +
            winnerText + "\n" +
            "最终比分：" +
            matchManager.Player1Score +
            " : " +
            matchManager.Player2Score;
    }

    private void HideMatchEndPanel()
    {
        if (matchEndPanel != null)
        {
            matchEndPanel.SetActive(false);
        }

        if (matchHUDCanvas != null)
        {
            matchHUDCanvas.SetActive(true);
        }
    }

    private void RestartMatch()
    {
        Debug.Log("点击了重新开始比赛按钮");

        OnlineMatchManager matchManager = OnlineMatchManager.Instance;

        if (matchManager == null)
        {
            Debug.LogWarning("没有找到 OnlineMatchManager，无法重新开始比赛");
            return;
        }

        NetworkPuckPhysics puck = FindObjectOfType<NetworkPuckPhysics>();

        if (puck == null)
        {
            Debug.LogWarning("没有找到 NetworkPuckPhysics，无法重置冰球");
            return;
        }

        NetworkObject puckNetworkObject = puck.GetComponent<NetworkObject>();

        if (puckNetworkObject == null)
        {
            Debug.LogWarning("冰球没有 NetworkObject，无法重置比赛");
            return;
        }

        matchManager.RPC_RestartMatch(puckNetworkObject.Id);

        Debug.Log("已发送重新开始比赛 RPC");
    }

    private void ReturnToMainMenu()
    {
        Debug.Log("点击了返回主菜单按钮");

        NetworkRunner[] runners = FindObjectsOfType<NetworkRunner>();

        for (int i = 0; i < runners.Length; i++)
        {
            if (runners[i] != null)
            {
                runners[i].Shutdown();
                Destroy(runners[i].gameObject);
            }
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}