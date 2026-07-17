using UnityEngine;

public class StickBladeTriggerHit : MonoBehaviour
{
    [Header("Puck")]
    [SerializeField] private string puckTag = "Puck";

    [Header("Hit Settings")]
    [SerializeField] private float minHitSpeed = 0.25f;
    [SerializeField] private float hitForceMultiplier = 1.8f;
    [SerializeField] private float maxHitForce = 16f;
    [SerializeField] private float minTimeBetweenHits = 0.08f;
    [SerializeField] private bool flattenDirection = true;

    [Header("Feedback")]
    [SerializeField] private HockeyStickHitFeedback hitFeedback;

    [Header("Shot Stats")]
    [SerializeField] private bool registerShotOnHit = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = false;

    private Vector3 lastPosition;
    private Vector3 bladeVelocity;
    private float lastHitTime;

    private void Start()
    {
        lastPosition = transform.position;

        if (hitFeedback == null)
        {
            hitFeedback = GetComponentInParent<HockeyStickHitFeedback>();
        }
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
        {
            return;
        }

        bladeVelocity = (transform.position - lastPosition) / deltaTime;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (Time.time - lastHitTime < minTimeBetweenHits)
        {
            return;
        }

        if (!IsPuck(other))
        {
            return;
        }

        Rigidbody puckRigidbody = other.attachedRigidbody;

        if (puckRigidbody == null)
        {
            return;
        }

        float hitSpeed = bladeVelocity.magnitude;

        if (hitSpeed < minHitSpeed)
        {
            return;
        }

        Vector3 hitDirection = bladeVelocity.normalized;

        if (flattenDirection)
        {
            hitDirection = Vector3.ProjectOnPlane(hitDirection, Vector3.up).normalized;

            if (hitDirection.sqrMagnitude < 0.001f)
            {
                return;
            }
        }

        float force = Mathf.Clamp(
            hitSpeed * hitForceMultiplier,
            0f,
            maxHitForce
        );

        puckRigidbody.AddForce(hitDirection * force, ForceMode.Impulse);

        if (registerShotOnHit && ShotStatsManager.Instance != null)
        {
            ShotStatsManager.Instance.RegisterShot();
        }

        float feedbackStrength = Mathf.InverseLerp(0f, maxHitForce, force);

        if (hitFeedback != null)
        {
            hitFeedback.PlayHitFeedback(feedbackStrength);
        }

        lastHitTime = Time.time;

        if (showDebugLog)
        {
            Debug.Log("击中冰球，速度：" + hitSpeed + "，力度：" + force);
        }
    }

    private bool IsPuck(Collider other)
    {
        if (other.CompareTag(puckTag))
        {
            return true;
        }

        if (other.attachedRigidbody != null &&
            other.attachedRigidbody.CompareTag(puckTag))
        {
            return true;
        }

        return false;
    }
}