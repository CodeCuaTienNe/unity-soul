using UnityEngine;

public class DespawnOnCollide : MonoBehaviour
{
    [Tooltip("Tags that will trigger despawn when collided with (leave empty to despawn on any collision)")]
    public string[] despawnTriggerTags;

    [Tooltip("Whether to destroy the object that was collided with")]
    public bool destroyCollidedObject = false;

    [Tooltip("Particle effect to spawn when despawning (optional)")]
    public GameObject despawnEffect;

    [Tooltip("Sound to play when despawning (optional)")]
    public AudioClip despawnSound;

    [Tooltip("Volume of the despawn sound")]
    [Range(0f, 1f)]
    public float despawnSoundVolume = 0.7f;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if we should despawn based on collision tags
        if (despawnTriggerTags.Length > 0)
        {
            bool tagFound = false;
            foreach (string tag in despawnTriggerTags)
            {
                if (collision.gameObject.CompareTag(tag))
                {
                    tagFound = true;
                    break;
                }
            }

            // If no matching tag was found, return without despawning
            if (!tagFound)
            {
                return;
            }
        }

        // Spawn particle effect if assigned
        if (despawnEffect != null)
        {
            Instantiate(despawnEffect, transform.position, Quaternion.identity);
        }

        // Play sound if assigned
        if (despawnSound != null)
        {
            AudioSource.PlayClipAtPoint(despawnSound, transform.position, despawnSoundVolume);
        }

        // Destroy the collided object if the option is enabled
        if (destroyCollidedObject)
        {
            Destroy(collision.gameObject);
        }

        // Destroy this object
        Destroy(gameObject);
    }
}
