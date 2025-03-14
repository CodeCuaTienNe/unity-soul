using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyAtObject : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private bool useTargetTag = false;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float searchRadius = 50f;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool useConstantSpeed = true;
    [SerializeField] private float accelerationRate = 2f;
    [SerializeField] private float maxSpeed = 20f;

    [Header("Behavior Settings")]
    [SerializeField] private bool startFlyingOnAwake = true;
    [SerializeField] private bool destroyOnReachTarget = false;
    [SerializeField] private float targetReachedDistance = 0.5f;
    [SerializeField] private bool lookAtTarget = true;

    private float currentSpeed;
    private bool isFlying = false;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = speed;

        // If no rigidbody is attached, add one
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }

        // Find target by tag if enabled
        if (useTargetTag && string.IsNullOrEmpty(targetTag) == false)
        {
            FindTargetByTag();
        }

        if (startFlyingOnAwake)
        {
            StartFlying();
        }
    }

    private void FindTargetByTag()
    {
        GameObject taggedObject = GameObject.FindGameObjectWithTag(targetTag);
        if (taggedObject != null)
        {
            target = taggedObject.transform;
        }
        else
        {
            Debug.LogWarning($"No object with tag '{targetTag}' found!");
        }
    }

    private void FindNearestTargetWithTag()
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(targetTag);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (GameObject obj in taggedObjects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < closestDistance && distance <= searchRadius)
            {
                closestDistance = distance;
                closestTarget = obj.transform;
            }
        }

        if (closestTarget != null)
        {
            target = closestTarget;
        }
        else
        {
            Debug.LogWarning($"No objects with tag '{targetTag}' found within {searchRadius} units!");
        }
    }

    public void StartFlying()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned! Cannot start flying.");
            return;
        }

        isFlying = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.None;
    }

    public void StopFlying()
    {
        isFlying = false;
        rb.linearVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!isFlying || target == null)
            return;

        // Calculate direction to target
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // Rotate towards target if enabled
        if (lookAtTarget)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Apply movement
        if (useConstantSpeed)
        {
            // Use constant speed
            rb.linearVelocity = transform.forward * currentSpeed;
        }
        else
        {
            // Accelerate over time
            currentSpeed = Mathf.Min(currentSpeed + accelerationRate * Time.deltaTime, maxSpeed);
            rb.linearVelocity = transform.forward * currentSpeed;
        }

        // Check if reached target
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (destroyOnReachTarget && distanceToTarget <= targetReachedDistance)
        {
            Destroy(gameObject);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        currentSpeed = newSpeed;
    }

    // Optional: Visualize search radius in editor
    private void OnDrawGizmosSelected()
    {
        if (useTargetTag)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, searchRadius);
        }

        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}