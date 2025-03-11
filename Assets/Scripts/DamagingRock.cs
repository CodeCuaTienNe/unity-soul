using UnityEngine;

public class DamagingRock : MonoBehaviour
{
    [Tooltip("Amount of damage this rock deals on impact")]
    public float damage = 1f;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Rock triggered with: {other.gameObject.name}");
        
        // Check if the rock hit the player
        PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
        if (playerHealth != null)
        {
            Debug.Log($"Player hit! Dealing {damage} damage");
            // Apply damage to the player
            playerHealth.TakeDamage(damage);
            
            // Destroy the rock after impact
            Destroy(gameObject);
        }
    }
}