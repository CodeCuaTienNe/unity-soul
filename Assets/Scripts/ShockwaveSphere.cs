using System.Collections;
using UnityEngine;

public class ShockwaveSphere : MonoBehaviour
{
    [Header("Shockwave Properties")]
    [SerializeField] private float expandDuration = 0.5f;
    [SerializeField] private float maxRadius = 5.0f;
    [SerializeField] private float knockbackForce = 10f;

    [Header("Visual Effects")]
    [SerializeField] private Material shockwaveMaterial;
    [SerializeField] private Color startColor = new Color(1f, 0.5f, 0f, 0.8f); // Orange with some transparency
    [SerializeField] private Color endColor = new Color(1f, 0.5f, 0f, 0f); // Fully transparent at the end

    private bool hasDealtDamage = false;
    private float initialScale;

    private void Awake()
    {
        // Set up the sphere's material if provided
        if (shockwaveMaterial != null)
        {
            GetComponent<Renderer>().material = shockwaveMaterial;
            GetComponent<Renderer>().material.color = startColor;
        }

        // Store initial scale factor (usually 1)
        initialScale = transform.localScale.x;

        // Start at a very small scale
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f) * initialScale;

        // Begin expanding
        StartCoroutine(ExpandAndFade());
    }

    private IEnumerator ExpandAndFade()
    {
        float startTime = Time.time;

        while (Time.time < startTime + expandDuration)
        {
            // Calculate progress (0 to 1)
            float t = (Time.time - startTime) / expandDuration;

            // Calculate the current radius based on progress
            float currentRadius = Mathf.Lerp(0.1f, maxRadius, t) * initialScale;

            // Update scale
            transform.localScale = new Vector3(currentRadius, currentRadius, currentRadius);

            // Update color/transparency if material exists
            if (shockwaveMaterial != null)
            {
                GetComponent<Renderer>().material.color = Color.Lerp(startColor, endColor, t);
            }

            yield return null;
        }

        // Ensure we reach final size
        transform.localScale = new Vector3(maxRadius, maxRadius, maxRadius) * initialScale;

        // Destroy after expansion is complete
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Only interact with the player once per shockwave
            if (!hasDealtDamage)
            {
                // Apply knockback force
                Rigidbody playerRigidbody = other.GetComponent<Rigidbody>();
                if (playerRigidbody != null)
                {
                    Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                    // Keep knockback mostly horizontal (reduce Y component)
                    knockbackDirection.y = 0.2f;
                    playerRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                }

                // Mark as dealt with this player
                hasDealtDamage = true;

                // NOTE: Damage is handled by your existing damage system
                // The collider on this sphere will trigger your damage system
            }
        }
    }
}