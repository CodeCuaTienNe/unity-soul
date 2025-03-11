using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [Tooltip("Amount of damage this sword deals on impact")]
    public float damage = 100f;
    
    [Tooltip("Whether the sword can currently deal damage")]
    public bool canDealDamage = false;
    
    // Reference to the player controller to prevent self-damage
    private PlayerController playerController;
    
    private void Start()
    {
        // Find the player controller this sword belongs to
        playerController = GetComponentInParent<PlayerController>();
        
        // Set the sword to not deal damage by default
        canDealDamage = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Only deal damage when allowed and not hitting self
        if (!canDealDamage || other.gameObject == playerController?.gameObject) 
            return;
        
        // Try to find a boss health controller on the hit object
        BossHealthBarController bossHealth = other.GetComponent<BossHealthBarController>();
        if (bossHealth != null)
        {
            // Apply damage to the boss
            bossHealth.TakeDamage(damage);
            Debug.Log($"Sword hit boss! Dealing {damage} damage. Boss health: {bossHealth.luongMauHienTai}");
        }
    }
}