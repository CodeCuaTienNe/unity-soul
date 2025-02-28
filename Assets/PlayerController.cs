using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Movement settings
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 15.0f;
    [SerializeField] private bool useRigidbodyMovement = true;

    // Component references
    private Rigidbody rb;
    private Vector3 velocity;
    private Animator animator;
    private CameraController cameraController;
    private CharacterController characterController;
    private Vector2 moveInput;

    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // Check if we have proper movement component
        if (useRigidbodyMovement && rb == null)
        {
            Debug.LogError("PlayerController: Using Rigidbody movement but no Rigidbody component found!");
        }
        else if (!useRigidbodyMovement && characterController == null)
        {
            Debug.LogError("PlayerController: Using CharacterController movement but no CharacterController component found!");
            useRigidbodyMovement = true; // Fall back to Rigidbody if available
        }

        // Find camera controller
        cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("PlayerController: Cannot find CameraController on main camera!");
        }
    }

    public void onMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        // We'll use this input in FixedUpdate for movement
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    void Update()
    {
        // Update animator
        if (animator != null)
        {
            animator.SetFloat("speed", velocity.magnitude);
        }
    }

    private void HandleMovement()
    {
        // Skip if no camera controller
        if (cameraController == null) return;
        
        // Calculate camera-relative movement direction
        Vector3 forward = cameraController.GetCameraForward();
        Vector3 right = cameraController.GetCameraRight();
        
        // Use the input from the Input System
        Vector3 movementDirection = (forward * moveInput.y + right * moveInput.x);
        
        // Only normalize if we have movement
        if (movementDirection.magnitude > 0.1f)
        {
            movementDirection.Normalize();
        }
        
        // Store current velocity for animation
        velocity = movementDirection;
        
        // Apply movement based on the component we're using
        if (useRigidbodyMovement)
        {
            if (rb != null)
            {
                // Use Rigidbody movement
                rb.linearVelocity = new Vector3(
                    movementDirection.x * moveSpeed,
                    rb.linearVelocity.y,  // Preserve vertical velocity for gravity
                    movementDirection.z * moveSpeed
                );
                
                // Rotate towards movement direction
                if (movementDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                    rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                }
            }
        }
        else
        {
            if (characterController != null)
            {
                // Use CharacterController movement
                characterController.Move(movementDirection * moveSpeed * Time.fixedDeltaTime);
                
                // Rotate towards movement direction
                if (movementDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                }
            }
        }
    }
}
