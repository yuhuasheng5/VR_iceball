using Fusion;
using UnityEngine;

public class NetworkPlayerAvatar : NetworkBehaviour
{
    [Header("Body Visuals")]
    public Transform headVisual;
    public Transform leftHandVisual;
    public Transform rightHandVisual;

    [Header("Stick")]
    public Transform stickVisual;
    public Transform rightGripAnchor;
    public Transform leftGripAnchor;

    [Header("Renderers")]
    public Renderer headRenderer;
    public Renderer leftHandRenderer;
    public Renderer rightHandRenderer;
    public Renderer stickRenderer;

    [Header("Colors")]
    public Color localColor = Color.green;
    public Color remoteColor = Color.red;

    [Header("Local Display")]
    public bool hideLocalHead = true;
    public bool hideLocalHands = false;
    public bool hideLocalStick = false;

    [Header("Two Hand Stick Settings")]
    public bool useTwoHandStick = true;
    public float minTwoHandDistance = 0.12f;

    [Tooltip("如果球杆方向不对，优先调这个。常试：0,0,0 / 0,90,0 / 0,-90,0 / 90,0,0")]
    public Vector3 rightHandRotationOffsetEuler = Vector3.zero;

    [Tooltip("整体位置微调。一般先保持 0,0,0")]
    public Vector3 stickPositionOffset = Vector3.zero;

    [Tooltip("整体旋转微调。一般先保持 0,0,0")]
    public Vector3 stickRotationOffsetEuler = Vector3.zero;

    [Header("Debug")]
    public bool showDebugLog = true;

    [Networked] private Vector3 HeadPosition { get; set; }
    [Networked] private Quaternion HeadRotation { get; set; }

    [Networked] private Vector3 LeftHandPosition { get; set; }
    [Networked] private Quaternion LeftHandRotation { get; set; }

    [Networked] private Vector3 RightHandPosition { get; set; }
    [Networked] private Quaternion RightHandRotation { get; set; }

    private bool hasWarnedNoXR;

    public override void Spawned()
    {
        AutoAssignRenderers();
        ApplyColor();
        ApplyLocalVisibility();

        if (stickVisual != null)
        {
            stickVisual.gameObject.SetActive(true);
        }

        if (showDebugLog)
        {
            Debug.Log(
                "NetworkPlayerAvatar Spawned. StateAuthority: " +
                Object.HasStateAuthority +
                " InputAuthority: " +
                Object.HasInputAuthority
            );
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        LocalXRReferences localXR = LocalXRReferences.Instance;

        if (localXR == null)
        {
            if (!hasWarnedNoXR)
            {
                Debug.LogWarning("本地玩家无法同步：场景里没有 LocalXRReferences");
                hasWarnedNoXR = true;
            }

            return;
        }

        if (localXR.head != null)
        {
            HeadPosition = localXR.head.position;
            HeadRotation = localXR.head.rotation;
        }

        if (localXR.leftHand != null)
        {
            LeftHandPosition = localXR.leftHand.position;
            LeftHandRotation = localXR.leftHand.rotation;
        }

        if (localXR.rightHand != null)
        {
            RightHandPosition = localXR.rightHand.position;
            RightHandRotation = localXR.rightHand.rotation;
        }
    }

    public override void Render()
    {
        ApplyPose(headVisual, HeadPosition, HeadRotation);
        ApplyPose(leftHandVisual, LeftHandPosition, LeftHandRotation);
        ApplyPose(rightHandVisual, RightHandPosition, RightHandRotation);

        UpdateTwoHandStickPose();
    }

    private void ApplyPose(Transform target, Vector3 position, Quaternion rotation)
    {
        if (target == null)
        {
            return;
        }

        target.SetPositionAndRotation(position, rotation);
    }

    private void UpdateTwoHandStickPose()
    {
        if (stickVisual == null)
        {
            return;
        }

        if (!useTwoHandStick)
        {
            stickVisual.SetPositionAndRotation(RightHandPosition, RightHandRotation);
            return;
        }

        if (rightGripAnchor == null || leftGripAnchor == null)
        {
            stickVisual.SetPositionAndRotation(RightHandPosition, RightHandRotation);
            return;
        }

        Quaternion rightHandRotation =
            RightHandRotation *
            Quaternion.Euler(rightHandRotationOffsetEuler);

        Quaternion targetRotation =
            rightHandRotation *
            Quaternion.Inverse(rightGripAnchor.localRotation);

        Vector3 localGripDirection =
            leftGripAnchor.localPosition - rightGripAnchor.localPosition;

        Vector3 currentWorldGripDirection =
            targetRotation * localGripDirection;

        Vector3 targetWorldGripDirection =
            LeftHandPosition - RightHandPosition;

        bool useTwoHands =
            currentWorldGripDirection.sqrMagnitude > 0.0001f &&
            targetWorldGripDirection.sqrMagnitude > 0.0001f &&
            targetWorldGripDirection.magnitude > minTwoHandDistance;

        if (useTwoHands)
        {
            Quaternion correction = Quaternion.FromToRotation(
                currentWorldGripDirection.normalized,
                targetWorldGripDirection.normalized
            );

            targetRotation = correction * targetRotation;
        }

        Quaternion extraRotation = Quaternion.Euler(stickRotationOffsetEuler);
        targetRotation = targetRotation * extraRotation;

        Vector3 targetPosition =
            RightHandPosition -
            targetRotation * rightGripAnchor.localPosition;

        targetPosition += targetRotation * stickPositionOffset;

        stickVisual.SetPositionAndRotation(targetPosition, targetRotation);
    }

    private void AutoAssignRenderers()
    {
        if (headRenderer == null && headVisual != null)
        {
            headRenderer = headVisual.GetComponentInChildren<Renderer>();
        }

        if (leftHandRenderer == null && leftHandVisual != null)
        {
            leftHandRenderer = leftHandVisual.GetComponentInChildren<Renderer>();
        }

        if (rightHandRenderer == null && rightHandVisual != null)
        {
            rightHandRenderer = rightHandVisual.GetComponentInChildren<Renderer>();
        }

        if (stickRenderer == null && stickVisual != null)
        {
            stickRenderer = stickVisual.GetComponentInChildren<Renderer>();
        }
    }

    private void ApplyColor()
    {
        Color targetColor = Object.HasStateAuthority ? localColor : remoteColor;

        SetRendererColor(headRenderer, targetColor);
        SetRendererColor(leftHandRenderer, targetColor);
        SetRendererColor(rightHandRenderer, targetColor);

        // 如果正式球杆不想被染成红/绿，可以注释掉下面这一行
        SetRendererColor(stickRenderer, targetColor);
    }

    private void ApplyLocalVisibility()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if (hideLocalHead && headVisual != null)
        {
            headVisual.gameObject.SetActive(false);
        }

        if (hideLocalHands)
        {
            if (leftHandVisual != null)
            {
                leftHandVisual.gameObject.SetActive(false);
            }

            if (rightHandVisual != null)
            {
                rightHandVisual.gameObject.SetActive(false);
            }
        }

        if (hideLocalStick && stickVisual != null)
        {
            stickVisual.gameObject.SetActive(false);
        }
    }

    private void SetRendererColor(Renderer targetRenderer, Color color)
    {
        if (targetRenderer == null)
        {
            return;
        }

        targetRenderer.material.color = color;
    }
}