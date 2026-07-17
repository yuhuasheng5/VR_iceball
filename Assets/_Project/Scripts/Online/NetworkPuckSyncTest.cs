using Fusion;
using UnityEngine;

public class NetworkPuckSyncTest : NetworkBehaviour
{
    [Header("Visual")]
    public Transform puckVisual;

    [Header("Test Movement")]
    public bool testAutoMove = true;
    public float moveRange = 1.2f;
    public float moveSpeed = 1.5f;

    [Header("Debug")]
    public bool showDebugLog = true;

    [Networked] private Vector3 NetworkPosition { get; set; }
    [Networked] private Quaternion NetworkRotation { get; set; }

    private Vector3 startPosition;
    private bool hasStartPosition;

    public override void Spawned()
    {
        startPosition = transform.position;
        hasStartPosition = true;

        if (Object.HasStateAuthority)
        {
            NetworkPosition = transform.position;
            NetworkRotation = transform.rotation;
        }

        if (showDebugLog)
        {
            Debug.Log(
                "NetworkPuck Spawned. StateAuthority: " +
                Object.HasStateAuthority
            );
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if (!hasStartPosition)
        {
            startPosition = transform.position;
            hasStartPosition = true;
        }

        if (testAutoMove)
        {
            float offsetX =
                Mathf.Sin((float)Runner.SimulationTime * moveSpeed) *
                moveRange;

            transform.position = new Vector3(
                startPosition.x + offsetX,
                startPosition.y,
                startPosition.z
            );
        }

        NetworkPosition = transform.position;
        NetworkRotation = transform.rotation;
    }

    public override void Render()
    {
        transform.SetPositionAndRotation(NetworkPosition, NetworkRotation);
    }
}