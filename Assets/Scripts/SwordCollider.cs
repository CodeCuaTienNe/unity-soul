using UnityEngine;
using System.Collections.Generic;

public class SwordCollider : MonoBehaviour
{
    public int damage = 10;
    public bool showDebug = true;
    public GameObject hitEffectPrefab;
    
    // Track objects hit to prevent multiple hits in one swing
    private List<Collider> hitObjectsThisSwing = new List<Collider>();
    
    private void Start()
    {
        // Make sure the collider is set as trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("SwordCollider should have its collider set to trigger. Setting it now.");
            col.isTrigger = true;
        }
        else if (col == null)
        {
            Debug.LogError("SwordCollider requires a Collider component!");
        }
    }
    
    // Call this when starting a new swing
    public void ResetHitObjects()
    {
        hitObjectsThisSwing.Clear();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Skip if we've already hit this object this swing
        if (hitObjectsThisSwing.Contains(other))
            return;
        
        // Add to hit list to prevent multiple hits in same swing
        hitObjectsThisSwing.Add(other);
        
        // Try to find BossHealthBarController directly
        BossHealthBarController healthController = other.GetComponent<BossHealthBarController>();
        if (healthController == null)
        {
            // Try parent objects
            healthController = other.GetComponentInParent<BossHealthBarController>();
        }
        
        if (healthController != null)
        {
            // Apply damage directly to the health controller
            healthController.TakeDamage(damage);
            
            if (showDebug)
            {
                Debug.Log($"Sword hit boss! Dealing {damage} damage. Current health: {healthController.luongMauHienTai}");
            }
            
            // Spawn hit effect if assigned
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, other.ClosestPoint(transform.position), Quaternion.identity);
            }
        }
        else if (showDebug)
        {
            Debug.Log($"Sword hit object with no health controller: {other.gameObject.name}");
        }
    }
    
    // For testing in the Unity Editor
    [ContextMenu("Test Trigger Damage")]
    public void TestTriggerDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
        foreach (Collider col in colliders)
        {
            if (col.gameObject != gameObject)
            {
                BossHealthBarController healthController = col.GetComponent<BossHealthBarController>();
                if (healthController != null)
                {
                    healthController.TakeDamage(damage);
                    Debug.Log($"Test damage applied to {col.gameObject.name}");
                    break;
                }
            }
        }
    }
}