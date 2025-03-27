using UnityEngine;

public class BloodHitEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController; // Reference to player controller
    [SerializeField] private Animator playerAnimator; // Reference to player animator

    [Header("Blood Effect Settings")]
    [SerializeField] private GameObject bloodSplashPrefab;
    [SerializeField] private float bloodPoolLifetime = 5f;
    [SerializeField] private float minSplashForce = 2f;
    [SerializeField] private float maxSplashForce = 5f;
    [SerializeField] private int minDroplets = 5;
    [SerializeField] private int maxDroplets = 15;
    [SerializeField] private LayerMask bloodSplatterLayers;
    [SerializeField] private bool showDebugLogs = true;

    // Cache the isHitting field reflection for performance
    private System.Reflection.FieldInfo isHittingField;

    private void Start()
    {
        // Auto-find references if not set
        if (playerController == null)
        {
            playerController = GetComponentInParent<PlayerController>();
            if (playerController == null)
            {
                playerController = FindObjectOfType<PlayerController>();
            }
        }

        if (playerAnimator == null && playerController != null)
        {
            playerAnimator = playerController.GetComponent<Animator>();
        }

        // Use reflection to access the isHitting field from PlayerController
        // (This is a fallback since we can't modify PlayerController)
        if (playerController != null)
        {
            isHittingField = playerController.GetType().GetField("isHitting",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (isHittingField == null && showDebugLogs)
            {
                Debug.LogWarning("Could not find isHitting field in PlayerController");
            }
        }
    }

    // This method should be called from your sword's trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Only create blood splash if the player is attacking (using multiple detection methods)
        if (IsPlayerAttacking())
        {
            // Check enemy/boss/flesh tags
            if (other.CompareTag("Enemy") || other.CompareTag("Boss") || other.CompareTag("Flesh"))
            {
                if (showDebugLogs)
                {
                    Debug.Log($"Creating blood splash on hit with {other.name} during active attack");
                }

                // Create the blood effect at the collision point
                CreateBloodSplash(other.ClosestPoint(transform.position));
            }
        }
    }

    // Combine multiple methods to detect if player is attacking
    private bool IsPlayerAttacking()
    {
        bool isAttacking = false;

        // Method 1: Try to get isHitting directly from reflection
        if (isHittingField != null && playerController != null)
        {
            isAttacking = isAttacking || (bool)isHittingField.GetValue(playerController);
        }

        // Method 2: Check animator parameters
        if (playerAnimator != null)
        {
            // Check if the attack animation is playing
            bool isHittingAnim = playerAnimator.GetBool("isHitting");
            AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
            bool isInAttackState = stateInfo.IsTag("Attack") || stateInfo.IsName("Attack") ||
                                  stateInfo.IsName("AttackCombo") || stateInfo.IsName("Attack1") ||
                                  stateInfo.IsName("Attack2") || stateInfo.IsName("Attack3");

            isAttacking = isAttacking || isHittingAnim || isInAttackState;

            // Check if we're in the early portion of the attack animation
            // (create blood right at the beginning of the swing)
            if (isInAttackState)
            {
                float normalizedTime = stateInfo.normalizedTime % 1;
                // Create blood effect during the early part of the animation
                isAttacking = isAttacking && (normalizedTime > 0.05f && normalizedTime < 0.4f);
            }
        }

        return isAttacking;
    }

    // Method to create blood splash effect
    public void CreateBloodSplash(Vector3 hitPosition)
    {
        // Only create blood splash if we have a prefab
        if (bloodSplashPrefab != null)
        {
            // Create the main blood splash
            GameObject bloodPool = Instantiate(bloodSplashPrefab, hitPosition, Quaternion.identity);

            // Rotate blood to match surface normal if possible
            RaycastHit hit;
            if (Physics.Raycast(hitPosition + Vector3.up * 0.1f, Vector3.down, out hit, 0.5f, bloodSplatterLayers))
            {
                bloodPool.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }

            // Destroy after lifetime
            Destroy(bloodPool, bloodPoolLifetime);

            // Check if we hit a boss for special effects
            bool isBossHit = false;
            RaycastHit[] hits = Physics.SphereCastAll(hitPosition, 0.5f, Vector3.up, 0.1f);
            foreach (RaycastHit nearbyHit in hits)
            {
                if (nearbyHit.collider.CompareTag("Boss"))
                {
                    isBossHit = true;
                    break;
                }
            }

            // Create additional blood droplets
            int dropletCount = isBossHit ?
                Random.Range(minDroplets * 2, maxDroplets * 2) :
                Random.Range(minDroplets, maxDroplets);

            CreateBloodDroplets(hitPosition, dropletCount);
        }
        else
        {
            Debug.LogWarning("Blood splash prefab is not assigned!");
        }
    }

    // Method to create blood droplets
    private void CreateBloodDroplets(Vector3 hitPosition, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Create smaller blood splash
            GameObject droplet = Instantiate(bloodSplashPrefab, hitPosition, Random.rotation);

            // Make it smaller
            droplet.transform.localScale *= Random.Range(0.1f, 0.3f);

            // Add force in random direction
            Vector3 randomDir = Random.onUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y); // Make them go mostly upward

            // Add rigidbody if needed
            Rigidbody rb = droplet.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = droplet.AddComponent<Rigidbody>();
            }

            // Apply random force
            float force = Random.Range(minSplashForce, maxSplashForce);
            rb.AddForce(randomDir * force, ForceMode.Impulse);

            // Destroy after random time
            Destroy(droplet, Random.Range(1f, 3f));
        }
    }
}
