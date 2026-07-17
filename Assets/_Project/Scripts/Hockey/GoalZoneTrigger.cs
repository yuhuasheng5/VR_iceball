using UnityEngine;

public class GoalZoneTrigger : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private string zoneName = "中间区域";
    [SerializeField] private int scoreValue = 1;
    [SerializeField] private string puckTag = "Puck";

    [Header("References")]
    [SerializeField] private GoalZoneManager goalZoneManager;

    private void Awake()
    {
        if (goalZoneManager == null)
        {
            goalZoneManager = GetComponentInParent<GoalZoneManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPuck(other))
        {
            return;
        }

        PuckController puckController = GetPuckController(other);

        if (puckController == null)
        {
            return;
        }

        if (goalZoneManager != null)
        {
            goalZoneManager.TryScore(puckController, zoneName, scoreValue);
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

    private PuckController GetPuckController(Collider other)
    {
        PuckController puckController = other.GetComponent<PuckController>();

        if (puckController != null)
        {
            return puckController;
        }

        if (other.attachedRigidbody != null)
        {
            puckController = other.attachedRigidbody.GetComponent<PuckController>();
        }

        return puckController;
    }
}