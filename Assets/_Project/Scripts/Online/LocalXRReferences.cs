using UnityEngine;

public class LocalXRReferences : MonoBehaviour
{
    public static LocalXRReferences Instance { get; private set; }

    [Header("Local XR Transforms")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private void Awake()
    {
        Instance = this;
    }
}