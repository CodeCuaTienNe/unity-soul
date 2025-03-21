using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Singleton instance
    private static CameraShake instance;

    // The original position of the camera before shaking
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Current active shake coroutine
    private Coroutine currentShake;

    private void Awake()
    {
        // Set up singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        // Store initial position and rotation
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    /// <summary>
    /// Trigger a camera shake effect - can be called from other scripts
    /// </summary>
    /// <param name="duration">Duration of the shake in seconds</param>
    /// <param name="magnitude">Strength of the shake</param>
    /// <param name="decreaseOverTime">Whether the shake should decrease over time</param>
    public static void Shake(float duration = 0.5f, float magnitude = 0.1f, bool decreaseOverTime = true)
    {
        // Check if instance exists
        if (instance == null)
        {
            Debug.LogWarning("Cannot shake camera: No CameraShake component found!");
            return;
        }

        // Stop any existing shake
        if (instance.currentShake != null)
        {
            instance.StopCoroutine(instance.currentShake);
        }

        // Start new shake
        instance.currentShake = instance.StartCoroutine(instance.DoShake(duration, magnitude, decreaseOverTime));
    }

    private IEnumerator DoShake(float duration, float magnitude, bool decreaseOverTime)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Calculate current magnitude (decreasing over time if specified)
            float currentMagnitude = decreaseOverTime ?
                                     magnitude * (1f - (elapsed / duration)) :
                                     magnitude;

            // Apply random position offset
            Vector3 shakeOffset = Random.insideUnitSphere * currentMagnitude;
            transform.localPosition = originalPosition + shakeOffset;

            // Optional: Apply some random rotation for more intense shake
            transform.localRotation = originalRotation * Quaternion.Euler(
                Random.Range(-1f, 1f) * currentMagnitude * 2.0f,
                Random.Range(-1f, 1f) * currentMagnitude * 2.0f,
                Random.Range(-1f, 1f) * currentMagnitude * 2.0f
            );

            elapsed += Time.deltaTime;

            yield return null;
        }

        // Reset to original position when done
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        currentShake = null;
    }
}