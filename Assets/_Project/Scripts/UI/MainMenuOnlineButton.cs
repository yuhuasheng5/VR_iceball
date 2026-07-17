using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuOnlineButton : MonoBehaviour
{
    [Header("Button")]
    public Button online1v1Button;

    [Header("Scene")]
    public string onlineSceneName = "Scene_OnlineRoomTest";

    private void Reset()
    {
        online1v1Button = GetComponent<Button>();
    }

    private void Awake()
    {
        if (online1v1Button == null)
        {
            online1v1Button = GetComponent<Button>();
        }
    }

    private void Start()
    {
        if (online1v1Button != null)
        {
            online1v1Button.onClick.AddListener(LoadOnlineScene);
        }
        else
        {
            Debug.LogWarning("MainMenuOnlineButton 没有绑定 Button");
        }
    }

    private void OnDestroy()
    {
        if (online1v1Button != null)
        {
            online1v1Button.onClick.RemoveListener(LoadOnlineScene);
        }
    }

    private void LoadOnlineScene()
    {
        if (string.IsNullOrEmpty(onlineSceneName))
        {
            Debug.LogWarning("联网场景名为空，无法跳转");
            return;
        }

        SceneManager.LoadScene(onlineSceneName);
    }
}