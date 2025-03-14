using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlyToPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed at which the object flies toward the player")]
    public float moveSpeed = 5f;
    
    [Tooltip("How fast the object rotates to face its movement direction")]
    public float rotationSpeed = 3f;
    
    [Tooltip("Time it takes to reach full speed")]
    public float accelerationTime = 1f;
    
    [Tooltip("Curve that controls how speed increases over time")]
    public AnimationCurve accelerationCurve;

    [Header("Target Settings")]
    [Tooltip("Whether to automatically find and target the player on start")]
    public bool findPlayerOnStart = true;
    
    [Tooltip("Tag used to find the player object")]
    public string playerTag = "Player";
    
    [Tooltip("Layers to ignore when colliding")]
    public LayerMask ignoreLayerMask;

    [Header("Behavior Settings")]
    [Tooltip("Maximum lifetime in seconds before self-destruction")]
    public float maxLifetime = 15f;
    
    [Tooltip("Whether to destroy this object when it hits the player")]
    public bool destroyOnPlayerCollision = true;
    
    [Tooltip("Effect to spawn when hitting the player")]
    public GameObject hitEffect;

    // References
    private Rigidbody rb;
    private float currentSpeed;
    private float accelerationProgress = 0f;
    private bool isInitialized = false;
    private DamagingRock damagingRock;

    // The fixed target position (player's position at spawn time)
    private Vector3 targetPosition;
    private Vector3 flyDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        damagingRock = GetComponent<DamagingRock>();

        // Make sure rigidbody is set up correctly
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent physics rotation

        // Set default acceleration curve if none provided
        if (accelerationCurve == null || accelerationCurve.length == 0)
        {
            accelerationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }

    private void Start()
    {
        if (findPlayerOnStart)
        {
            FindPlayerOnce();
        }

        // Destroy after max lifetime to prevent objects from flying forever
        if (maxLifetime > 0)
        {
            Destroy(gameObject, maxLifetime);
        }
        
        // If we have a DamagingRock component, make sure it's configured correctly
        if (damagingRock != null)
        {
            // We'll handle destruction in this component, so disable auto-destruction in DamagingRock
            damagingRock.destroyOnAnyImpact = false;
            
            // Log that we're working with a DamagingRock
            Debug.Log("FlyToPlayer is working with a DamagingRock component");
        }
    }

    public void FindPlayerOnce()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            // Record the player's position at this moment only
            targetPosition = playerObject.transform.position;

            // Calculate the direction to fly (only once)
            flyDirection = (targetPosition - transform.position).normalized;

            isInitialized = true;
            StartCoroutine(AccelerateTowardsTarget());
            
            Debug.Log($"FlyToPlayer targeting player at position {targetPosition}");
        }
        else
        {
            Debug.LogWarning($"FlyToPlayer: No object with tag '{playerTag}' found!");
        }
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        flyDirection = (targetPosition - transform.position).normalized;
        isInitialized = true;
        StartCoroutine(AccelerateTowardsTarget());
        
        Debug.Log($"FlyToPlayer targeting custom position {targetPosition}");
    }

    private IEnumerator AccelerateTowardsTarget()
    {
        accelerationProgress = 0f;

        while (accelerationProgress < 1f)
        {
            accelerationProgress += Time.deltaTime / accelerationTime;
            yield return null;
        }

        accelerationProgress = 1f;
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        // Calculate current speed with acceleration curve
        currentSpeed = moveSpeed * accelerationCurve.Evaluate(accelerationProgress);

        // Move in the fixed direction (does not update with player movement)
        rb.linearVelocity = flyDirection * currentSpeed;

        // Keep rotation pointing in the travel direction
        if (rotationSpeed > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flyDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    private void HandleCollision(GameObject other)
    {
        // Check if we hit the player
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"FlyToPlayer hit player: {other.name}");
            
            // Optionally spawn hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            // Destroy if configured to do so
            if (destroyOnPlayerCollision)
            {
                // Let DamagingRock handle the damage application
                // We don't need to do anything special here
                
                // Only destroy if we don't have a DamagingRock component
                // (DamagingRock will handle destruction if it exists)
                if (damagingRock == null)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            // Ignore collisions with objects on the ignored layers
            if (((1 << other.layer) & ignoreLayerMask.value) != 0)
            {
                Collider myCollider = GetComponent<Collider>();
                Collider otherCollider = other.GetComponent<Collider>();
                
                if (myCollider != null && otherCollider != null)
                {
                    Physics.IgnoreCollision(myCollider, otherCollider);
                    Debug.Log($"FlyToPlayer ignoring collision with {other.name} due to layer mask");
                }
            }
        }
    }
}