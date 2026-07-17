using Fusion;
using UnityEngine;

public class NetworkPlayerMarker : NetworkBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer headRenderer;

    [Header("Colors")]
    [SerializeField] private Color localColor = Color.green;
    [SerializeField] private Color remoteColor = Color.red;

    public override void Spawned()
    {
        if (headRenderer == null)
        {
            headRenderer = GetComponentInChildren<Renderer>();
        }

        if (headRenderer != null)
        {
            headRenderer.material.color = Object.HasInputAuthority ? localColor : remoteColor;
        }

        Debug.Log("Network Player Spawned. Local Authority: " + Object.HasInputAuthority);
    }
}