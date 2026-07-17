using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PuckController : MonoBehaviour
{
    [Header("Reset")]
    public Transform resetPoint;
    public float resetHeight = -2f;

    [Header("Speed Limit")]
    public float maxSpeed = 14f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        LimitSpeed();
        CheckFallOut();
    }

    private void LimitSpeed()
    {
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    private void CheckFallOut()
    {
        if (transform.position.y < resetHeight)
        {
            ResetPuck();
        }
    }

    public void ResetPuck()
    {
        Vector3 targetPosition = resetPoint != null ? resetPoint.position : startPosition;

        transform.SetPositionAndRotation(targetPosition, startRotation);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}