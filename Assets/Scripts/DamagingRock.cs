using UnityEngine;

public class DamagingRock : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage this rock deals on impact")]
    public float damage = 1f;
    
    [Tooltip("Layers that can be damaged by this rock")]
    public LayerMask damageLayers = -1; // Default to all layers
    
    [Tooltip("Should this rock damage the player?")]
    public bool canDamagePlayer = true;

    [Header("Destruction Settings")]
    [Tooltip("Should this rock be destroyed on impact with any collider?")]
    public bool destroyOnAnyImpact = true;
    
    [Tooltip("Should this rock be destroyed on impact with the ground?")]
    public bool destroyOnGroundImpact = true;
    
    [Tooltip("Time in seconds before the rock is automatically destroyed")]
    public float autoDestroyTime = 10f;
    
    [Tooltip("Effect to spawn when the rock is destroyed")]
    public GameObject destroyEffect;

    private void Start()
    {
        // Auto-destroy after a certain time to prevent rocks from accumulating
        if (autoDestroyTime > 0)
        {
            Destroy(gameObject, autoDestroyTime);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    private void HandleCollision(GameObject other)
    {
        Debug.Log($"Rock collided with: {other.name} (Layer: {LayerMask.LayerToName(other.layer)})");
        
        // Kiểm tra layer của đối tượng va chạm
        int otherLayer = 1 << other.layer;
        if ((damageLayers.value & otherLayer) == 0)
        {
            Debug.Log($"Rock ignoring collision with {other.name} due to layer mask");
            return;
        }
        
        bool shouldDestroy = false;
        
        // Check if the rock hit the player
        PlayerHealthController playerHealth = other.GetComponent<PlayerHealthController>();
        if (playerHealth != null && canDamagePlayer)
        {
            Debug.Log($"Player hit! Dealing {damage} damage");
            // Apply damage to the player
            playerHealth.TakeDamage(damage);
            shouldDestroy = true;
        }
        
        // Check if the rock hit a boss
        BossHealthBarController bossHealth = other.GetComponent<BossHealthBarController>();
        if (bossHealth != null)
        {
            Debug.Log($"Boss hit! Dealing {damage} damage");
            // Apply damage to the boss
            bossHealth.TakeDamage(damage);
            shouldDestroy = true;
        }
        
        // Check if the rock hit the ground or floor
        if ((other.name.Contains("Ground") || other.name.Contains("Floor") || other.name.Contains("Terrain")) && destroyOnGroundImpact)
        {
            Debug.Log($"Rock hit {other.name} and will be destroyed");
            shouldDestroy = true;
        }
        
        // Destroy on any impact if configured
        if (destroyOnAnyImpact)
        {
            shouldDestroy = true;
        }
        
        // Destroy the rock if needed
        if (shouldDestroy)
        {
            // Spawn destroy effect if available
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}