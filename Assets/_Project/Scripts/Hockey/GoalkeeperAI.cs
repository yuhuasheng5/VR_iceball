using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class GoalkeeperAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform puck;
    [SerializeField] private Rigidbody puckRigidbody;

    [Header("Movement Range")]
    [SerializeField] private float minX = -0.85f;
    [SerializeField] private float maxX = 0.85f;

    [Header("Tracking Settings")]
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float trackStartZ = 1.5f;
    [SerializeField] private float predictionTime = 0.15f;
    [SerializeField] private bool onlyTrackWhilePlaying = true;

    [Header("Idle Settings")]
    [SerializeField] private bool returnToCenterWhenIdle = true;
    [SerializeField] private float idleMoveSpeed = 1.5f;

    [Header("Block Settings")]
    [SerializeField] private string puckTag = "Puck";
    [SerializeField] private float blockImpulse = 5f;
    [SerializeField] private float sideDeflectAmount = 0.35f;
    [SerializeField] private float minTimeBetweenBlocks = 0.12f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    private Rigidbody rb;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private float lastBlockTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void Start()
    {
        if (puck != null && puckRigidbody == null)
        {
            puckRigidbody = puck.GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        if (onlyTrackWhilePlaying)
        {
            if (GameManager.Instance == null ||
                GameManager.Instance.CurrentState != GameState.Playing)
            {
                MoveToX(startPosition.x, idleMoveSpeed);
                return;
            }
        }

        if (puck == null)
        {
            if (returnToCenterWhenIdle)
            {
                MoveToX(startPosition.x, idleMoveSpeed);
            }

            return;
        }

        bool shouldTrack = puck.position.z >= trackStartZ;

        if (shouldTrack)
        {
            float targetX = CalculateTargetX();
            MoveToX(targetX, moveSpeed);
        }
        else if (returnToCenterWhenIdle)
        {
            MoveToX(startPosition.x, idleMoveSpeed);
        }
    }

    private float CalculateTargetX()
    {
        float targetX = puck.position.x;

        if (puckRigidbody != null)
        {
            targetX += puckRigidbody.velocity.x * predictionTime;
        }

        return Mathf.Clamp(targetX, minX, maxX);
    }

    private void MoveToX(float targetX, float speed)
    {
        Vector3 currentPosition = rb.position;

        float newX = Mathf.MoveTowards(
            currentPosition.x,
            targetX,
            speed * Time.fixedDeltaTime
        );

        Vector3 targetPosition = new Vector3(
            newX,
            startPosition.y,
            startPosition.z
        );

        rb.MovePosition(targetPosition);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryBlockPuck(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryBlockPuck(collision);
    }

    private void TryBlockPuck(Collision collision)
    {
        if (Time.time - lastBlockTime < minTimeBetweenBlocks)
        {
            return;
        }

        if (!IsPuck(collision))
        {
            return;
        }

        Rigidbody hitPuckRigidbody = collision.rigidbody;

        if (hitPuckRigidbody == null)
        {
            return;
        }

        Vector3 sideDirection = Vector3.right * Mathf.Sign(
            hitPuckRigidbody.position.x - transform.position.x
        );

        if (sideDirection.sqrMagnitude < 0.001f)
        {
            sideDirection = Vector3.right;
        }

        Vector3 blockDirection =
            Vector3.back + sideDirection * sideDeflectAmount;

        blockDirection = Vector3.ProjectOnPlane(blockDirection, Vector3.up).normalized;

        hitPuckRigidbody.AddForce(
            blockDirection * blockImpulse,
            ForceMode.Impulse
        );

        lastBlockTime = Time.time;

        if (showDebugLog)
        {
            Debug.Log("守门员挡住冰球");
        }
    }

    private bool IsPuck(Collision collision)
    {
        if (collision.gameObject.CompareTag(puckTag))
        {
            return true;
        }

        if (collision.rigidbody != null &&
            collision.rigidbody.CompareTag(puckTag))
        {
            return true;
        }

        return false;
    }

    public void ResetGoalkeeper()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Physics.SyncTransforms();

        if (showDebugLog)
        {
            Debug.Log("守门员已重置");
        }
    }
}