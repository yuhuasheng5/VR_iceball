using Fusion;
using UnityEngine;

public class OnlineStickHitbox : MonoBehaviour
{
    [Header("Owner")]
    public NetworkObject ownerNetworkObject;

    [Header("Hit Settings")]
    public float minHitSpeed = 0.25f;
    public float hitForceMultiplier = 1.8f;
    public float maxHitForce = 16f;
    public float minTimeBetweenHits = 0.15f;
    public bool flattenDirection = true;

    [Header("Direction Settings")]
    public bool requireMovingTowardPuck = true;
    public float minTowardPuckDot = 0.15f;
    public bool blendDirectionToPuck = true;
    public float directionToPuckBlend = 0.2f;

    [Header("Debug")]
    public bool showDebugLog = true;

    private Vector3 lastPosition;
    private Vector3 hitboxVelocity;
    private float lastHitTime;

    private void Start()
    {
        lastPosition = transform.position;

        if (ownerNetworkObject == null)
        {
            ownerNetworkObject = GetComponentInParent<NetworkObject>();
        }
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
        {
            return;
        }

        Vector3 instantVelocity = (transform.position - lastPosition) / deltaTime;

        hitboxVelocity = Vector3.Lerp(
            hitboxVelocity,
            instantVelocity,
            0.7f
        );

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (Time.time - lastHitTime < minTimeBetweenHits)
        {
            return;
        }

        if (ownerNetworkObject == null)
        {
            return;
        }

        // 只允许本地玩家自己的球杆发起击球
        if (!ownerNetworkObject.HasStateAuthority && !ownerNetworkObject.HasInputAuthority)
        {
            return;
        }

        NetworkPuckPhysics networkPuck =
            other.GetComponentInParent<NetworkPuckPhysics>();

        if (networkPuck == null)
        {
            return;
        }

        float hitSpeed = hitboxVelocity.magnitude;

        if (hitSpeed < minHitSpeed)
        {
            return;
        }

        Vector3 swingDirection = hitboxVelocity.normalized;

        if (flattenDirection)
        {
            swingDirection = Vector3.ProjectOnPlane(swingDirection, Vector3.up);

            if (swingDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            swingDirection.Normalize();
        }

        Vector3 directionToPuck =
            networkPuck.transform.position - transform.position;

        directionToPuck = Vector3.ProjectOnPlane(directionToPuck, Vector3.up);

        if (directionToPuck.sqrMagnitude > 0.001f)
        {
            directionToPuck.Normalize();

            if (requireMovingTowardPuck)
            {
                float dot = Vector3.Dot(swingDirection, directionToPuck);

                if (dot < minTowardPuckDot)
                {
                    if (showDebugLog)
                    {
                        Debug.Log("挥杆方向不是朝向冰球，不触发击打。Dot = " + dot);
                    }

                    return;
                }
            }

            if (blendDirectionToPuck)
            {
                swingDirection = Vector3.Slerp(
                    swingDirection,
                    directionToPuck,
                    directionToPuckBlend
                ).normalized;
            }
        }

        float hitForce = Mathf.Clamp(
            hitSpeed * hitForceMultiplier,
            0f,
            maxHitForce
        );

        networkPuck.RPC_ApplyHit(
            swingDirection,
            hitForce,
            networkPuck.transform.position
        );

        lastHitTime = Time.time;

        if (showDebugLog)
        {
            Debug.Log(
                "双手球杆击中网络冰球，速度：" +
                hitSpeed +
                "，力度：" +
                hitForce +
                "，方向：" +
                swingDirection
            );
        }
    }
}