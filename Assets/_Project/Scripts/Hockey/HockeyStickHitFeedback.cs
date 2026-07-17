using UnityEngine;
using UnityEngine.XR;

public class HockeyStickHitFeedback : MonoBehaviour
{
    [Header("Haptic")]
    [SerializeField] private XRNode hapticHand = XRNode.RightHand;
    [SerializeField] private float baseAmplitude = 0.35f;
    [SerializeField] private float maxAmplitude = 0.85f;
    [SerializeField] private float duration = 0.08f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private float minVolume = 0.45f;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.08f;

    [Header("Limit")]
    [SerializeField] private float minTimeBetweenFeedback = 0.08f;

    private InputDevice hapticDevice;
    private float lastFeedbackTime;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayHitFeedback(float strength)
    {
        if (Time.time - lastFeedbackTime < minTimeBetweenFeedback)
        {
            return;
        }

        lastFeedbackTime = Time.time;

        strength = Mathf.Clamp01(strength);

        PlayHaptic(strength);
        PlayAudio(strength);
    }

    private void PlayHaptic(float strength)
    {
        if (!hapticDevice.isValid)
        {
            hapticDevice = InputDevices.GetDeviceAtXRNode(hapticHand);
        }

        if (!hapticDevice.isValid)
        {
            return;
        }

        HapticCapabilities capabilities;

        if (!hapticDevice.TryGetHapticCapabilities(out capabilities))
        {
            return;
        }

        if (!capabilities.supportsImpulse)
        {
            return;
        }

        float amplitude = Mathf.Lerp(baseAmplitude, maxAmplitude, strength);
        hapticDevice.SendHapticImpulse(0, amplitude, duration);
    }

    private void PlayAudio(float strength)
    {
        if (audioSource == null || hitClip == null)
        {
            return;
        }

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, strength);
        audioSource.PlayOneShot(hitClip);
    }
}