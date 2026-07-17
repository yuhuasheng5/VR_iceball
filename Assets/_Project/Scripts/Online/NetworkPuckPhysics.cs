using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkPuckPhysics : NetworkBehaviour
{
    [Header("References")]
    public Rigidbody puckRigidbody;

    [Header("Sync Settings")]
    public float remoteLerpSpeed = 24f;
    public float snapDistance = 2.0f;

    [Header("Physics Limit")]
    public float maxHorizontalSpeed = 12f;
    public bool reduceOppositeVelocityOnHit = true;
    public float oppositeVelocityKeepRatio = 0.25f;

    [Header("Debug")]
    public bool showDebugLog = true;

    [Networked] private Vector3 NetworkPosition { get; set; }
    [Networked] private Quaternion NetworkRotation { get; set; }
    [Networked] private Vector3 NetworkVelocity { get; set; }
    [Networked] private Vector3 NetworkAngularVelocity { get; set; }

    private void Awake()
    {
        if (puckRigidbody == null)
        {
            puckRigidbody = GetComponent<Rigidbody>();
        }
    }

    public override void Spawned()
    {
        if (puckRigidbody == null)
        {
            puckRigidbody = GetComponent<Rigidbody>();
        }

        if (Object.HasStateAuthority)
        {
            puckRigidbody.isKinematic = false;
            puckRigidbody.useGravity = true;
            puckRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            puckRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
            NetworkVelocity = puckRigidbody.velocity;
            NetworkAngularVelocity = puckRigidbody.angularVelocity;
        }
        else
        {
            puckRigidbody.isKinematic = true;
            puckRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        if (showDebugLog)
        {
            Debug.Log("NetworkPuckPhysics Spawned. StateAuthority: " + Object.HasStateAuthority);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        LimitHorizontalSpeed();

        NetworkPosition = transform.position;
        NetworkRotation = transform.rotation;
        NetworkVelocity = puckRigidbody.velocity;
        NetworkAngularVelocity = puckRigidbody.angularVelocity;
    }

    public override void Render()
    {
        if (Object.HasStateAuthority)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, NetworkPosition);

        if (distance > snapDistance)
        {
            transform.SetPositionAndRotation(NetworkPosition, NetworkRotation);
            return;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            NetworkPosition,
            remoteLerpSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            NetworkRotation,
            remoteLerpSpeed * Time.deltaTime
        );
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
public void RPC_ResetPuck(Vector3 resetPosition)
{
    if (puckRigidbody == null)
    {
        puckRigidbody = GetComponent<Rigidbody>();
    }

    transform.position = resetPosition;
    transform.rotation = Quaternion.identity;

    puckRigidbody.velocity = Vector3.zero;
    puckRigidbody.angularVelocity = Vector3.zero;
    puckRigidbody.isKinematic = false;
    puckRigidbody.WakeUp();

    NetworkPosition = transform.position;
    NetworkRotation = transform.rotation;
    NetworkVelocity = Vector3.zero;
    NetworkAngularVelocity = Vector3.zero;

    if (showDebugLog)
    {
        Debug.Log("网络冰球已重置到：" + resetPosition);
    }
}
    public void RPC_ApplyHit(Vector3 hitDirection, float hitForce, Vector3 hitPoint)
    {
        if (puckRigidbody == null)
        {
            puckRigidbody = GetComponent<Rigidbody>();
        }

        Vector3 flatDirection = Vector3.ProjectOnPlane(hitDirection, Vector3.up);

        if (flatDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        flatDirection.Normalize();

        puckRigidbody.isKinematic = false;
        puckRigidbody.WakeUp();

        if (reduceOppositeVelocityOnHit)
        {
            ReduceOppositeVelocity(flatDirection);
        }

        puckRigidbody.AddForceAtPosition(
            flatDirection * hitForce,
            hitPoint,
            ForceMode.Impulse
        );

        LimitHorizontalSpeed();

        NetworkPosition = transform.position;
        NetworkRotation = transform.rotation;
        NetworkVelocity = puckRigidbody.velocity;
        NetworkAngularVelocity = puckRigidbody.angularVelocity;

        if (showDebugLog)
        {
            Debug.Log("网络冰球受到击打，方向：" + flatDirection + "，力度：" + hitForce);
        }
    }

    private void ReduceOppositeVelocity(Vector3 hitDirection)
    {
        Vector3 velocity = puckRigidbody.velocity;
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);

        if (horizontalVelocity.sqrMagnitude < 0.001f)
        {
            return;
        }

        float dot = Vector3.Dot(horizontalVelocity.normalized, hitDirection.normalized);

        if (dot < 0f)
        {
            Vector3 verticalVelocity = Vector3.Project(velocity, Vector3.up);
            Vector3 keptHorizontalVelocity = horizontalVelocity * oppositeVelocityKeepRatio;

            puckRigidbody.velocity = keptHorizontalVelocity + verticalVelocity;
        }
    }

    private void LimitHorizontalSpeed()
    {
        if (puckRigidbody == null)
        {
            return;
        }

        Vector3 velocity = puckRigidbody.velocity;
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);

        if (horizontalVelocity.magnitude <= maxHorizontalSpeed)
        {
            return;
        }

        Vector3 verticalVelocity = Vector3.Project(velocity, Vector3.up);
        horizontalVelocity = horizontalVelocity.normalized * maxHorizontalSpeed;

        puckRigidbody.velocity = horizontalVelocity + verticalVelocity;
    }
}