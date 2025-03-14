using UnityEngine;

public class FlyTowardsPlayer : MonoBehaviour
{
    [HideInInspector]
    public Transform playerTransform;

    [HideInInspector]
    public float flySpeed = 5f;

    [HideInInspector]
    public bool homingBehavior = false;

    private Vector3 flyDirection;
    private Rigidbody rb;

    private void Start()
    {
        // Lấy component Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("Added Rigidbody to object for FlyTowardsPlayer");
        }
        
        // Cấu hình Rigidbody
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        if (playerTransform == null)
        {
            // Try to find the player if not set
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log($"Found player: {player.name}");
            }
            else
            {
                Debug.LogError("No player transform set for FlyTowardsPlayer script!");
                Destroy(this);
                return;
            }
        }

        // Set initial direction if not using homing behavior
        if (!homingBehavior)
        {
            flyDirection = (playerTransform.position - transform.position).normalized;
            Debug.Log($"Initial direction to player: {flyDirection}");
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || rb == null)
        {
            return;
        }

        // Update direction if using homing behavior
        if (homingBehavior)
        {
            flyDirection = (playerTransform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(flyDirection);
        }

        // Move towards the target using Rigidbody
        rb.linearVelocity = flyDirection * flySpeed;
        
        // Log để debug
        if (Time.frameCount % 60 == 0) // Log mỗi 60 frame để tránh spam console
        {
            Debug.Log($"Flying towards player. Position: {transform.position}, Velocity: {rb.linearVelocity}, Direction: {flyDirection}");
        }
    }
} 