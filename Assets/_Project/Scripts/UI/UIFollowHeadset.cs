using UnityEngine;

public class UIFollowHeadset : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform headset;

    [Header("Follow Settings")]
    [SerializeField] private float distance = 1.8f;
    [SerializeField] private float heightOffset = -0.15f;
    [SerializeField] private float positionSmoothTime = 0.35f;
    [SerializeField] private float rotationSmoothSpeed = 4f;

    [Header("Comfort")]
    [SerializeField] private bool yawOnly = true;

    private Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (headset == null)
        {
            return;
        }

        Vector3 forward = headset.forward;

        if (yawOnly)
        {
            forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = transform.forward;
            }
        }

        Vector3 targetPosition =
            headset.position +
            forward * distance +
            Vector3.up * heightOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            positionSmoothTime
        );

        Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }
}