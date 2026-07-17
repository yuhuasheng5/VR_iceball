using System.Collections;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using Fusion.Photon.Realtime;
public class FusionRoomStarter : MonoBehaviour
{
    [Header("UI")]
    public InputField roomNameInput;
    public Button createOrJoinButton;
    public Text statusText;

    [Header("Debug UI")]
    public Text onlineDebugText;

    [Header("UI Root")]
    public GameObject roomPanel;
    public GameObject matchHUDCanvas;

    [Header("Online")]
    public OnlinePlayerSpawner playerSpawner;
    public OnlinePuckSpawner puckSpawner;
    public OnlineMatchSpawner matchSpawner;

    [Header("Room Settings")]
    public string defaultRoomName = "Room_001";

    [Header("Photon Region")]
    public string fixedRegion = "asia";
    public string appVersion = "VRBall_Online_001";

    [Header("Auto Join For Headset Test")]
    public bool autoJoinOnAndroid = true;
    public float autoJoinDelay = 1.0f;

    [Header("Debug")]
    public bool showRoomDebug = true;

    private NetworkRunner runner;
    private bool isStarting;
    private bool hasStartedRoom;
    private Coroutine debugCoroutine;

    private void Start()
    {
        if (roomNameInput != null)
        {
            roomNameInput.text = defaultRoomName;
        }

        if (createOrJoinButton != null)
        {
            createOrJoinButton.onClick.AddListener(CreateOrJoinRoom);
        }

        if (roomPanel != null)
        {
            roomPanel.SetActive(true);
        }

        if (matchHUDCanvas != null)
        {
            matchHUDCanvas.SetActive(false);
        }

        SetStatus("未连接");

        if (autoJoinOnAndroid && Application.platform == RuntimePlatform.Android)
        {
            StartCoroutine(AutoJoinRoomAfterDelay());
        }
    }

    private void OnDestroy()
    {
        if (createOrJoinButton != null)
        {
            createOrJoinButton.onClick.RemoveListener(CreateOrJoinRoom);
        }

        if (debugCoroutine != null)
        {
            StopCoroutine(debugCoroutine);
            debugCoroutine = null;
        }
    }

    private IEnumerator AutoJoinRoomAfterDelay()
    {
        yield return new WaitForSeconds(autoJoinDelay);

        if (!hasStartedRoom && !isStarting)
        {
            CreateOrJoinRoom();
        }
    }

    private void CreateOrJoinRoom()
    {
        if (isStarting || hasStartedRoom)
        {
            return;
        }

        _ = StartSharedRoomAsync();
    }

    private async Task StartSharedRoomAsync()
    {
        isStarting = true;

        if (createOrJoinButton != null)
        {
            createOrJoinButton.interactable = false;
        }

        string roomName = defaultRoomName;

        if (roomNameInput != null && !string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            roomName = roomNameInput.text.Trim();
        }

        SetStatus(
            "正在连接房间：" + roomName +
            "\n固定Region：" + fixedRegion +
            "\nAppVersion：" + appVersion
        );

        if (runner == null)
        {
            GameObject runnerObject = new GameObject("NetworkRunner");
            runner = runnerObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;

            DontDestroyOnLoad(runnerObject);
        }

        var customAppSettings = PhotonAppSettings.Global.AppSettings.GetCopy();

        customAppSettings.UseNameServer = true;
        customAppSettings.AppVersion = appVersion;

        if (!string.IsNullOrWhiteSpace(fixedRegion))
        {
            customAppSettings.FixedRegion = fixedRegion.Trim().ToLower();
        }

        StartGameResult result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            CustomPhotonAppSettings = customAppSettings,
            UseCachedRegions = false
        });

        if (result.Ok)
        {
            hasStartedRoom = true;

            SetStatus(
                "已进入房间：" + roomName +
                "\n固定Region：" + fixedRegion +
                "\nAppVersion：" + appVersion
            );

            SpawnOnlineObjects();
            ShowMatchHUD();

            if (debugCoroutine != null)
            {
                StopCoroutine(debugCoroutine);
            }

            debugCoroutine = StartCoroutine(UpdateRoomDebug());
        }
        else
        {
            string failMessage =
                "连接失败：" + result.ShutdownReason +
                "\n详情：" + result +
                "\n固定Region：" + fixedRegion +
                "\nAppVersion：" + appVersion +
                "\n网络状态：" + Application.internetReachability;

            SetStatus(failMessage);
            Debug.LogError(failMessage);

            hasStartedRoom = false;

            if (createOrJoinButton != null)
            {
                createOrJoinButton.interactable = true;
            }

            if (runner != null)
            {
                Destroy(runner.gameObject);
                runner = null;
            }
        }

        isStarting = false;
    }

    private void SpawnOnlineObjects()
    {
        if (runner == null)
        {
            Debug.LogError("生成联网对象失败：NetworkRunner 为空");
            return;
        }

        if (playerSpawner != null)
        {
            playerSpawner.SpawnLocalPlayer(runner);
        }
        else
        {
            Debug.LogError("没有绑定 OnlinePlayerSpawner，无法生成网络玩家");
        }

        if (puckSpawner != null)
        {
            puckSpawner.SpawnPuckIfMasterClient(runner);
        }
        else
        {
            Debug.LogError("没有绑定 OnlinePuckSpawner，无法生成网络冰球");
        }

        if (matchSpawner != null)
        {
            matchSpawner.SpawnMatchManagerIfMasterClient(runner);
        }
        else
        {
            Debug.LogError("没有绑定 OnlineMatchSpawner，无法生成比赛管理器");
        }
    }

    private IEnumerator UpdateRoomDebug()
    {
        while (runner != null)
        {
            int activePlayerCount = 0;

            foreach (PlayerRef player in runner.ActivePlayers)
            {
                activePlayerCount++;
            }

            NetworkPlayerAvatar[] avatars = FindObjectsOfType<NetworkPlayerAvatar>();

            string sessionName = runner.SessionInfo != null
                ? runner.SessionInfo.Name
                : "Unknown";

            string message =
                "房间：" + sessionName +
                "\n本地玩家：" + runner.LocalPlayer +
                "\nPhoton人数：" + activePlayerCount +
                "\nNetworkPlayer数量：" + avatars.Length +
                "\n固定Region：" + fixedRegion +
                "\nAppVersion：" + appVersion +
                "\n网络状态：" + Application.internetReachability;

            Debug.Log(message);

            if (showRoomDebug && onlineDebugText != null)
            {
                onlineDebugText.text = message;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void ShowMatchHUD()
    {
        if (roomPanel != null)
        {
            roomPanel.SetActive(false);
        }

        if (matchHUDCanvas != null)
        {
            matchHUDCanvas.SetActive(true);
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        if (onlineDebugText != null)
        {
            onlineDebugText.text = message;
        }

        Debug.Log(message);
    }
}