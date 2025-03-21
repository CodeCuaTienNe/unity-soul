using System.Collections;
using UnityEngine;

public class JumpAttack : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float jumpDuration = 1.0f;
    [SerializeField] private float hangTime = 0.5f;
    [SerializeField] private float fallDuration = 0.8f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    [Header("Cooldown Settings")]
    [SerializeField] private float attackCooldown = 5.0f;
    private float cooldownTimer = 0f;

    [Header("Shockwave Settings")]
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private float shockwaveDuration = 0.5f;

    private bool isJumping = false;
    private float groundLevel;

    private void Start()
    {
        // Store the initial Y position as the ground level
        groundLevel = transform.position.y;
        StartJumpAttack();
    }

    private void Update()
    {
        // Decrease cooldown timer
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        if (transform.position.y != groundLevel) {
            return;
        }

        // If not jumping and cooldown is finished, the boss can jump again
        if (!isJumping && cooldownTimer <= 0)
        {
            // You can trigger the jump based on player distance, 
            // randomly, or other game logic here

            
            StartJumpAttack();
            // For AI-driven behavior, you might want to do something like:
            // if (ShouldJumpBasedOnAI())
            // {
            //     StartJumpAttack();
            // }
        }
    }

    private void StartJumpAttack()
    {
        if (!isJumping && cooldownTimer <= 0)
        {
            isJumping = true;
            StartCoroutine(JumpAndSlamCoroutine());
        }
    }

    private IEnumerator JumpAndSlamCoroutine()
    {
        // Jump phase - moving upward
        float startTime = Time.time;
        Vector3 startPos = transform.position;
        Vector3 jumpTargetPos = startPos + new Vector3(0, jumpHeight, 0);

        while (Time.time < startTime + jumpDuration)
        {
            float t = (Time.time - startTime) / jumpDuration;
            // Use easeOut curve for more natural jump
            float smoothT = 1 - (1 - t) * (1 - t);
            transform.position = Vector3.Lerp(startPos, jumpTargetPos, smoothT);
            yield return null;
        }

        // Ensure we reach exact target position
        transform.position = jumpTargetPos;

        // Hang time at apex
        yield return new WaitForSeconds(hangTime);

        // Fall phase
        startTime = Time.time;
        startPos = transform.position;
        Vector3 landingPos = new Vector3(startPos.x, groundLevel, startPos.z);

        while (Time.time < startTime + fallDuration)
        {
            float t = (Time.time - startTime) / fallDuration;

            // Different acceleration curve options for descent:

            // Option 1: Quadratic (t) - moderately accelerating fall (current)
            // float smoothT = t * t;

            // Option 2: Cubic (t) - more aggressive acceleration
            // float smoothT = t * t * t;

            // Option 3: Quartic (t) - very aggressive acceleration
            float smoothT = fallCurve.Evaluate(t);

            // Option 4: Custom curve - adjust values for different feel
            // float gravity = 2.5f;
            // float smoothT = Mathf.Pow(t, gravity);

            transform.position = Vector3.Lerp(startPos, landingPos, smoothT);
            yield return null;
        }

        // Ensure landing position is exact
        transform.position = landingPos;

        // Create shockwave on impact
        CreateShockwave();

        // Reset cooldown
        cooldownTimer = attackCooldown;
        isJumping = false;
    }

    private void CreateShockwave()
    {
        // This will be implemented later as requested
        // The code below is just a placeholder

        if (shockwavePrefab != null)
        {
            GameObject shockwave = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
            Destroy(shockwave, shockwaveDuration);
        }
        else
        {
            Debug.LogWarning("Shockwave prefab not assigned to BossJumpSlam script!");
        }
    }

    // Method that can be called by external scripts to trigger the jump
    public void TriggerJumpAttack()
    {
        StartJumpAttack();
    }

    // Helper method that returns true if boss is currently performing jump attack
    public bool IsPerformingJumpAttack()
    {
        return isJumping;
    }
}
