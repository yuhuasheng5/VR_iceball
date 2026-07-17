using UnityEngine;

public class VRHandVisualController : MonoBehaviour
{
    [Header("Controller Visuals")]
    [SerializeField] private GameObject leftControllerModel;
    [SerializeField] private GameObject rightControllerModel;

    [Header("Ray Interactors")]
    [SerializeField] private GameObject leftRayInteractor;
    [SerializeField] private GameObject rightRayInteractor;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void Start()
    {
        HandleGameStateChanged(GameState.Ready);
    }

    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Ready:
                SetControllerVisible(true);
                SetRayVisible(true);
                break;

            case GameState.Playing:
                SetControllerVisible(false);
                SetRayVisible(false);
                break;

            case GameState.Paused:
                SetControllerVisible(true);
                SetRayVisible(true);
                break;

            case GameState.GameOver:
                SetControllerVisible(true);
                SetRayVisible(true);
                break;
        }
    }

    private void SetControllerVisible(bool visible)
    {
        if (leftControllerModel != null)
            leftControllerModel.SetActive(visible);

        if (rightControllerModel != null)
            rightControllerModel.SetActive(visible);
    }

    private void SetRayVisible(bool visible)
    {
        if (leftRayInteractor != null)
            leftRayInteractor.SetActive(visible);

        if (rightRayInteractor != null)
            rightRayInteractor.SetActive(visible);
    }
}