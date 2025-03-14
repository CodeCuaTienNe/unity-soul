using UnityEngine;

public class DamagingRock : MonoBehaviour
{
    [Tooltip("Amount of damage this rock deals on impact")]
    public float damage = 1f;
    
    [Tooltip("Layers that can be damaged by this rock")]
    public LayerMask damageLayers = -1; // Default to all layers
    
    [Tooltip("Should this rock damage the player?")]
    public bool canDamagePlayer = true;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Rock triggered with: {other.gameObject.name}");
        
        // Kiểm tra layer của đối tượng va chạm
        int otherLayer = 1 << other.gameObject.layer;
        if ((damageLayers.value & otherLayer) == 0)
        {
            Debug.Log($"Rock ignoring collision with {other.gameObject.name} due to layer mask");
            return;
        }
        
        // Check if the rock hit the player
        PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
        if (playerHealth != null && canDamagePlayer)
        {
            Debug.Log($"Player hit! Dealing {damage} damage");
            // Apply damage to the player
            playerHealth.TakeDamage(damage);
            
            // Destroy the rock after impact
            Destroy(gameObject);
        }
        
        // Check if the rock hit a boss
        BossHealthBarController bossHealth = other.GetComponent<BossHealthBarController>();
        if (bossHealth != null)
        {
            Debug.Log($"Boss hit! Dealing {damage} damage");
            // Apply damage to the boss
            bossHealth.TakeDamage(damage);
            
            // Destroy the rock after impact
            Destroy(gameObject);
        }
    }
}