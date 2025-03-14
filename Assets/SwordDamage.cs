using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [Tooltip("Amount of damage this sword deals on impact")]
    public float damage = 100f;
    
    [Tooltip("Whether the sword can currently deal damage")]
    public bool canDealDamage = false;
    
    [Tooltip("Layer mask for objects that can be damaged")]
    public LayerMask damageLayers = -1; // Default to all layers
    
    [Tooltip("VFX prefab to spawn on hit (optional)")]
    public GameObject hitEffectPrefab;
    
    [Tooltip("Show debug messages")]
    public bool showDebug = true;
    
    // Reference to the player controller to prevent self-damage
    private PlayerController playerController;
    private GameObject playerObject;
    private PlayerHealthController playerHealth;
    
    // Track hit objects to prevent multiple hits in the same swing
    private System.Collections.Generic.HashSet<Collider> hitObjectsThisSwing = new System.Collections.Generic.HashSet<Collider>();
    
    private void Start()
    {
        // Find the player controller this sword belongs to (searching up in the hierarchy)
        Transform parent = transform.parent;
        while (parent != null && playerController == null)
        {
            playerController = parent.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerObject = parent.gameObject;
                playerHealth = parent.GetComponent<PlayerHealthController>();
            }
            parent = parent.parent;
        }
        
        // Set the sword to not deal damage by default
        canDealDamage = false;
        
        // Verify collider is present
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError("SwordDamage script requires a Collider component on the same GameObject!");
        }
        
        if (showDebug) Debug.Log("SwordDamage initialized. Damage amount: " + damage);
    }
    
    // Clear hit objects when damage is enabled (new swing)
    public void EnableDamage()
    {
        canDealDamage = true;
        hitObjectsThisSwing.Clear();
        if (showDebug) Debug.Log("Sword damage enabled");
    }
    
    // Disable damage
    public void DisableDamage()
    {
        canDealDamage = false;
        if (showDebug) Debug.Log("Sword damage disabled");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Add debug message to check collision is happening
        if (showDebug) Debug.Log($"Sword collided with: {other.gameObject.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");
        
        // Only deal damage when allowed and not hitting self
        if (!canDealDamage)
        {
            if (showDebug) Debug.Log("Collision ignored: Sword cannot deal damage now");
            return;
        }
        
        // Prevent hitting the same object multiple times in one swing
        if (hitObjectsThisSwing.Contains(other))
        {
            if (showDebug) Debug.Log("Collision ignored: Already hit during this swing");
            return;
        }
        
        // Kiểm tra kỹ lưỡng để tránh gây sát thương cho người chơi
        
        // Kiểm tra 1: Đối tượng va chạm có phải là người chơi không
        if (playerObject != null && other.gameObject == playerObject)
        {
            if (showDebug) Debug.Log("Collision ignored: Prevented self-damage to player");
            return;
        }
        
        // Kiểm tra 2: Đối tượng va chạm có phải là con của người chơi không
        if (playerObject != null && other.transform.IsChildOf(playerObject.transform))
        {
            if (showDebug) Debug.Log("Collision ignored: Prevented damage to player's child object");
            return;
        }
        
        // Kiểm tra 3: Đối tượng va chạm có tag "Player" không
        if (other.CompareTag("Player"))
        {
            if (showDebug) Debug.Log("Collision ignored: Object has Player tag");
            return;
        }
        
        // Kiểm tra 4: Đối tượng va chạm có PlayerHealthController không
        PlayerHealthController otherPlayerHealth = other.GetComponent<PlayerHealthController>();
        if (otherPlayerHealth != null)
        {
            if (showDebug) Debug.Log("Collision ignored: Object has PlayerHealthController");
            return;
        }
        
        // Check if the hit object is on a valid layer
        int otherLayer = 1 << other.gameObject.layer;
        if ((damageLayers.value & otherLayer) == 0)
        {
            if (showDebug) Debug.Log($"Collision ignored: Object on layer {LayerMask.LayerToName(other.gameObject.layer)} not in damage layers mask");
            return;
        }
        
        // Try to find a boss health controller on the hit object
        BossHealthBarController bossHealth = other.GetComponent<BossHealthBarController>();
        if (bossHealth != null)
        {
            // Add to hit list to prevent multiple hits in same swing
            hitObjectsThisSwing.Add(other);
            
            // Apply damage to the boss
            bossHealth.TakeDamage(damage);
            
            // Show debug info
            if (showDebug)
            {
                Debug.Log($"Sword hit boss! Dealing {damage} damage. Boss health: {bossHealth.luongMauHienTai}");
            }
            
            // Spawn hit effect if available
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, other.ClosestPoint(transform.position), Quaternion.identity);
            }
        }
        else
        {
            if (showDebug) Debug.Log("No BossHealthBarController found on hit object");
            
            // Try to find boss health on parent objects
            bossHealth = other.GetComponentInParent<BossHealthBarController>();
            if (bossHealth != null)
            {
                // Add to hit list to prevent multiple hits in same swing
                hitObjectsThisSwing.Add(other);
                
                // Apply damage to the boss
                bossHealth.TakeDamage(damage);
                
                if (showDebug) Debug.Log($"Sword hit boss (via parent)! Dealing {damage} damage. Boss health: {bossHealth.luongMauHienTai}");
                
                // Spawn hit effect if available
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, other.ClosestPoint(transform.position), Quaternion.identity);
                }
            }
        }
    }
    
    // Visualize the sword's hit area
    private void OnDrawGizmosSelected()
    {
        // Draw a wireframe box representing the collider
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.color = canDealDamage ? Color.red : Color.yellow;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                transform.position,
                transform.rotation,
                transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
    
    // Add a quick test method that can be called from editor
    [ContextMenu("Test Sword Damage")]
    public void TestSwordDamage()
    {
        EnableDamage();
        Debug.Log("Sword damage enabled for testing. Try hitting the boss now.");
        
        // Find all boss objects in scene for debug info
        BossHealthBarController[] allBossesInScene = FindObjectsOfType<BossHealthBarController>();
        Debug.Log($"Found {allBossesInScene.Length} boss objects in scene");
        foreach (var boss in allBossesInScene)
        {
            Debug.Log($"Boss: {boss.gameObject.name} at position: {boss.transform.position}");
        }
    }
}