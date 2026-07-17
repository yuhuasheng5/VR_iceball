using UnityEngine;

public class VRHandVisual : MonoBehaviour
{
    [Header("Controller Model")]
    [SerializeField] private GameObject controllerModel;

    public void SetControllerModelVisible(bool visible)
    {
        if (controllerModel != null)
        {
            controllerModel.SetActive(visible);
        }
    }
}