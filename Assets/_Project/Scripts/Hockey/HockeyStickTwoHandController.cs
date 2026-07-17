using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
public class HockeyStickTwoHandController : MonoBehaviour
{
    [Header("Hand Equip Points")]
    [SerializeField] private Transform rightHandEquipPoint;
    [SerializeField] private Transform leftHandEquipPoint;

    [Header("Stick Grip Anchors")]
    [SerializeField] private Transform rightGripAnchor;
    [SerializeField] private Transform leftGripAnchor;

    [Header("Controller Visuals")]
    [SerializeField] private VRHandVisual rightHandVisual;
    [SerializeField] private VRHandVisual leftHandVisual;

    [Header("Equip Settings")]
    [SerializeField] private XRNode equipHand = XRNode.RightHand;
    [SerializeField] private float pickupDistance = 1.2f;
    [SerializeField] private bool autoEquipOnStart = false;
    [SerializeField] private bool autoEquipWhenNear = true;
    [SerializeField] private bool allowTriggerEquip = true;
    [SerializeField] private bool allowGripEquip = true;
    [SerializeField] private bool allowUnequip = false;

    [Header("Reset")]
    [SerializeField] private Transform resetPoint;
    [SerializeField] private float equipCooldownAfterReset = 0.8f;

    [Header("Follow")]
    [SerializeField] private float positionLerpSpeed = 40f;
    [SerializeField] private float rotationLerpSpeed = 40f;
    [SerializeField] private float minTwoHandDistance = 0.12f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    [Header("State")]
    [SerializeField] private bool isEquipped;

    private Rigidbody rb;
    private InputDevice inputDevice;

    private bool lastTriggerPressed;
    private bool lastGripPressed;
    private bool lastSecondaryPressed;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private float nextEquipAllowedTime;

    public bool IsEquipped => isEquipped;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Start()
    {
        if (autoEquipOnStart)
        {
            Equip();
        }
    }

    private void Update()
    {
        UpdateInputDevice();

        if (!isEquipped)
        {
            if (Time.time < nextEquipAllowedTime)
            {
                return;
            }

            if (autoEquipWhenNear)
            {
                TryAutoEquipWhenNear();
            }
            else
            {
                TryEquipByInput();
            }
        }
        else if (allowUnequip)
        {
            TryUnequipByInput();
        }
    }

    private void LateUpdate()
    {
        if (!isEquipped)
        {
            return;
        }

        FollowHands();
        Physics.SyncTransforms();
    }

    private void UpdateInputDevice()
    {
        if (!inputDevice.isValid)
        {
            inputDevice = InputDevices.GetDeviceAtXRNode(equipHand);
        }
    }

    private void TryAutoEquipWhenNear()
    {
        if (IsRightHandNearStick())
        {
            Equip();
        }
    }

    private void TryEquipByInput()
    {
        if (!inputDevice.isValid)
        {
            return;
        }

        bool triggerPressed = false;
        bool gripPressed = false;

        inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out triggerPressed);
        inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripPressed);

        bool triggerDown = triggerPressed && !lastTriggerPressed;
        bool gripDown = gripPressed && !lastGripPressed;

        lastTriggerPressed = triggerPressed;
        lastGripPressed = gripPressed;

        bool equipInput =
            (allowTriggerEquip && triggerDown) ||
            (allowGripEquip && gripDown);

        if (!equipInput)
        {
            return;
        }

        if (IsRightHandNearStick())
        {
            Equip();
        }
        else if (showDebugLog)
        {
            float distance = GetRightHandDistanceToStick();
            Debug.Log("距离太远，无法装备球杆。当前距离: " + distance + "，允许距离: " + pickupDistance);
        }
    }

    private void TryUnequipByInput()
    {
        if (!inputDevice.isValid)
        {
            return;
        }

        bool secondaryPressed = false;
        inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryPressed);

        bool secondaryDown = secondaryPressed && !lastSecondaryPressed;
        lastSecondaryPressed = secondaryPressed;

        if (secondaryDown)
        {
            Unequip();
        }
    }

    private bool IsRightHandNearStick()
    {
        float distance = GetRightHandDistanceToStick();

        if (showDebugLog && !isEquipped)
        {
            Debug.Log("右手距离球杆握点: " + distance);
        }

        return distance <= pickupDistance;
    }

    private float GetRightHandDistanceToStick()
    {
        if (rightHandEquipPoint == null || rightGripAnchor == null)
        {
            return float.MaxValue;
        }

        return Vector3.Distance(rightHandEquipPoint.position, rightGripAnchor.position);
    }

    public void Equip()
    {
        if (rightHandEquipPoint == null)
        {
            Debug.LogWarning("没有绑定 RightHandEquipPoint");
            return;
        }

        if (leftHandEquipPoint == null)
        {
            Debug.LogWarning("没有绑定 LeftHandEquipPoint");
            return;
        }

        if (rightGripAnchor == null)
        {
            Debug.LogWarning("没有绑定 RightGripAnchor");
            return;
        }

        if (leftGripAnchor == null)
        {
            Debug.LogWarning("没有绑定 LeftGripAnchor");
            return;
        }

        isEquipped = true;

        if (rightHandVisual != null)
        {
            rightHandVisual.SetControllerModelVisible(false);
        }

        if (leftHandVisual != null)
        {
            leftHandVisual.SetControllerModelVisible(false);
        }

        SnapToHands();

        if (showDebugLog)
        {
            Debug.Log("球杆已装备");
        }
    }

    public void Unequip()
    {
        isEquipped = false;

        if (rightHandVisual != null)
        {
            rightHandVisual.SetControllerModelVisible(true);
        }

        if (leftHandVisual != null)
        {
            leftHandVisual.SetControllerModelVisible(true);
        }

        if (showDebugLog)
        {
            Debug.Log("球杆已解除装备");
        }
    }

    public void ResetStick()
    {
        Unequip();

        nextEquipAllowedTime = Time.time + equipCooldownAfterReset;

        Vector3 targetPosition = resetPoint != null ? resetPoint.position : startPosition;
        Quaternion targetRotation = resetPoint != null ? resetPoint.rotation : startRotation;

        transform.SetPositionAndRotation(targetPosition, targetRotation);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Physics.SyncTransforms();

        if (showDebugLog)
        {
            Debug.Log("球杆已重置到初始位置");
        }
    }

    private void FollowHands()
    {
        CalculateTargetPose(out Vector3 targetPosition, out Quaternion targetRotation);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionLerpSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationLerpSpeed * Time.deltaTime
        );
    }

    private void SnapToHands()
    {
        CalculateTargetPose(out Vector3 targetPosition, out Quaternion targetRotation);

        transform.SetPositionAndRotation(targetPosition, targetRotation);

        Physics.SyncTransforms();
    }

    private void CalculateTargetPose(out Vector3 targetPosition, out Quaternion targetRotation)
    {
        Quaternion baseRotation =
            rightHandEquipPoint.rotation *
            Quaternion.Inverse(rightGripAnchor.localRotation);

        Vector3 localGripDirection =
            leftGripAnchor.localPosition - rightGripAnchor.localPosition;

        Vector3 currentWorldGripDirection =
            baseRotation * localGripDirection;

        Vector3 targetWorldGripDirection =
            leftHandEquipPoint.position - rightHandEquipPoint.position;

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

            baseRotation = correction * baseRotation;
        }

        targetRotation = baseRotation;

        targetPosition =
            rightHandEquipPoint.position -
            targetRotation * rightGripAnchor.localPosition;
    }
}