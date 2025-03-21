using UnityEngine;

public class Skill3 : MonoBehaviour
{
    // Flight parameters
    public float targetYPosition = 5f;           // Target Y position to fly to
    public float flightSpeed = 1f;               // Speed of flight movement

    // Rotation parameters
    public float initialRotationSpeed = 30f;     // Initial rotation speed in degrees per second
    public float maxRotationSpeed = 360f;        // Maximum rotation speed in degrees per second
    public AnimationCurve accelerationCurve = new AnimationCurve(
        new Keyframe(0, 0),                      // Start at 0% of max speed
        new Keyframe(0.2f, 0.1f),                // Slow acceleration at first
        new Keyframe(0.6f, 0.5f),                // Medium acceleration in the middle
        new Keyframe(1, 1)                       // Reach 100% of max speed at end
    );
    public float rotationDuration = 10f;         // Duration of rotation in seconds
    public float returnRotationSpeed = 180f;     // Speed of rotation return
    public float returnFlightSpeed = 1f;         // Speed of return flight

    // Random activation parameters
    public float minTimeBetweenActivations = 5f;  // Minimum time between activations
    public float maxTimeBetweenActivations = 15f; // Maximum time between activations
    public bool startRandomlyOnAwake = true;      // Whether to start random activations immediately

    // Execution count parameters
    public int maxExecutions = 3;                 // Maximum number of times to execute the sequence (0 = infinite)
    private int executionCount = 0;               // Current number of executions

    // PrefabSpawner integration
    public PrefabSpawner1 prefabSpawner;           // Reference to the PrefabSpawner component
    private bool spawningStarted = false;         // Flag to track if spawning was started

    // Private variables
    private enum State { Idle, Flying, Rotating, ReturningRotation, ReturningPosition, Finished }
    private State currentState = State.Idle;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private float rotationTimer = 0f;
    private float nextActivationTime = 0f;

    void Start()
    {
        // Store original values
        originalRotation = transform.rotation;
        originalPosition = transform.position;

        // Calculate target position (only Y changes)
        targetPosition = new Vector3(
            originalPosition.x,
            targetYPosition,
            originalPosition.z
        );

        // Verify prefabSpawner reference
        if (prefabSpawner == null)
        {
            prefabSpawner = GetComponent<PrefabSpawner1>();
            if (prefabSpawner == null)
            {
                Debug.LogWarning("No PrefabSpawner assigned or found on this object. Please assign one in the inspector.");
            }
        }

        // Set up first random activation if enabled
        if (startRandomlyOnAwake)
        {
            ScheduleNextActivation();
        }
    }

    void Update()
    {
        // Check if it's time to activate when idle
        if (currentState == State.Idle && Time.time >= nextActivationTime && currentState != State.Finished)
        {
            // Begin the sequence
            currentState = State.Flying;
            Debug.Log("Random activation: Starting to fly to target position. Execution " + (executionCount + 1) +
                      (maxExecutions > 0 ? " of " + maxExecutions : ""));
        }

        // State machine for movement and rotation
        switch (currentState)
        {
            case State.Idle:
                // Do nothing, waiting for next activation time
                break;

            case State.Flying:
                // Smoothly fly to target position
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    flightSpeed * Time.deltaTime
                );

                // Check if we've reached the target position
                if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                {
                    currentState = State.Rotating;
                    rotationTimer = 0f; // Reset rotation timer
                    spawningStarted = false; // Reset spawning flag
                    Debug.Log("Reached target position. Starting rotation.");
                }
                break;

            case State.Rotating:
                // Update rotation timer
                rotationTimer += Time.deltaTime;

                if (rotationTimer <= rotationDuration)
                {
                    // Calculate normalized time (0 to 1)
                    float normalizedTime = rotationTimer / rotationDuration;

                    // Evaluate the curve to get the current speed percentage
                    float speedPercentage = accelerationCurve.Evaluate(normalizedTime);

                    // Calculate actual rotation speed
                    float currentSpeed = Mathf.Lerp(initialRotationSpeed, maxRotationSpeed, speedPercentage);

                    // Rotate around Y axis
                    transform.Rotate(0, currentSpeed * Time.deltaTime, 0);

                    // Start the PrefabSpawner when rotation begins
                    if (!spawningStarted && prefabSpawner != null)
                    {
                        prefabSpawner.StartSpawning();
                        spawningStarted = true;
                        Debug.Log("Started prefab spawning during rotation.");
                    }
                }
                else
                {
                    // Stop spawning as rotation ends
                    if (spawningStarted && prefabSpawner != null)
                    {
                        prefabSpawner.StopSpawning();
                        spawningStarted = false;
                        Debug.Log("Stopped prefab spawning as rotation completed.");
                    }

                    // Rotation phase complete, move to returning rotation phase
                    currentState = State.ReturningRotation;
                    Debug.Log("Rotation complete. Returning to original rotation.");
                }
                break;

            case State.ReturningRotation:
                // Smoothly rotate back to original rotation
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    originalRotation,
                    returnRotationSpeed * Time.deltaTime
                );

                // Check if rotation has returned to original
                if (Quaternion.Angle(transform.rotation, originalRotation) < 0.1f)
                {
                    transform.rotation = originalRotation; // Ensure exact original rotation
                    currentState = State.ReturningPosition;
                    Debug.Log("Rotation reset complete. Now returning to original position.");
                }
                break;

            case State.ReturningPosition:
                // Smoothly fly back to original position
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    originalPosition,
                    returnFlightSpeed * Time.deltaTime
                );

                // Check if position has returned to original
                if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
                {
                    transform.position = originalPosition; // Ensure exact original position

                    // Make sure spawning is stopped
                    if (spawningStarted && prefabSpawner != null)
                    {
                        prefabSpawner.StopSpawning();
                        spawningStarted = false;
                    }

                    // Increment execution counter
                    executionCount++;

                    // Check if we've reached the maximum number of executions
                    if (maxExecutions > 0 && executionCount >= maxExecutions)
                    {
                        currentState = State.Finished;
                        Debug.Log("Maximum executions (" + maxExecutions + ") reached. Sequence finished.");
                    }
                    else
                    {
                        currentState = State.Idle;
                        ScheduleNextActivation();
                        Debug.Log("Position reset complete. Waiting for next random activation. " +
                                 (maxExecutions > 0 ? (executionCount + " of " + maxExecutions + " completed.") :
                                 (executionCount + " executions completed.")));
                    }
                }
                break;

            case State.Finished:
                // Sequence is completed, do nothing
                break;
        }
    }

    // Schedule the next random activation time
    private void ScheduleNextActivation()
    {
        float randomDelay = Random.Range(minTimeBetweenActivations, maxTimeBetweenActivations);
        nextActivationTime = Time.time + randomDelay;
        Debug.Log("Next activation scheduled in " + randomDelay.ToString("F1") + " seconds.");
    }

    // Public method to manually trigger activation
    public void TriggerActivation()
    {
        if (currentState == State.Idle)
        {
            currentState = State.Flying;
            Debug.Log("Manual activation triggered.");
        }
    }

    // Public method to reset the execution counter and resume if finished
    public void ResetExecutionCount()
    {
        executionCount = 0;
        if (currentState == State.Finished)
        {
            currentState = State.Idle;
            ScheduleNextActivation();
            Debug.Log("Execution count reset. Sequence will restart.");
        }
    }

    // Called when script is disabled or GameObject is destroyed
    private void OnDisable()
    {
        // Make sure to stop spawning if this script is disabled
        if (spawningStarted && prefabSpawner != null)
        {
            prefabSpawner.StopSpawning();
        }
    }
}
