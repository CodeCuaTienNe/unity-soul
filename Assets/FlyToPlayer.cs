using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlyToPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float accelerationTime = 1f;
    [SerializeField] private AnimationCurve accelerationCurve;

    [Header("Target Settings")]
    [SerializeField] private bool findPlayerOnStart = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LayerMask ignoreLayerMask;

    [Header("Behavior Settings")]
    [SerializeField] private float maxLifetime = 15f;
    [SerializeField] private bool destroyOnPlayerCollision = true;
    [SerializeField] private GameObject hitEffect;

    // References
    private Rigidbody rb;
    private float currentSpeed;
    private float accelerationProgress = 0f;
    private bool isInitialized = false;

    // The fixed target position (player's position at spawn time)
    private Vector3 targetPosition;
    private Vector3 flyDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Make sure rigidbody is set up correctly
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent physics rotation

        // Set default acceleration curve if none provided
        if (accelerationCurve.length == 0)
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
        // Check if we hit the player
        if (collision.gameObject.CompareTag(playerTag))
        {
            // Custom player collision handling here

            // Optionally spawn hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }

            // Destroy if configured to do so
            if (destroyOnPlayerCollision)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // Ignore collisions with objects on the ignored layers
            if (((1 << collision.gameObject.layer) & ignoreLayerMask.value) != 0)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
            }
        }
    }
}