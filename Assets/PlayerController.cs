using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // Movement settings
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 15.0f;
    [SerializeField] private float slopeLimit = 45f;
    [SerializeField] private float stepOffset = 0.4f;
    [SerializeField] private float skinWidth = 0.08f;

    [Header("Physics Settings")]
    [SerializeField] private bool useGravity = true;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float groundedGravity = -1f; // Force to keep grounded
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer = -1; // Default to all layers
    [SerializeField] private float obstacleCheckDistance = 0.5f; // Check for obstacles in front
    
    // Advanced movement settings
    [Header("Advanced Movement")]
    [SerializeField] private bool useSliding = true;
    [SerializeField] private bool useObstacleAvoidance = true;
    [SerializeField] private float avoidanceSmoothness = 0.2f;
    [SerializeField] private float obstacleDetectionRadius = 0.4f;
    [SerializeField] private int obstacleDetectionRays = 12;

    // Debug settings
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool identifyBlockingObjects = true;

    // Component references
    private Vector3 velocity;
    private Animator animator;
    private CameraController cameraController;
    private CharacterController characterController;
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool hasInitialized = false;
    private bool isObstacleInFront = false;
    
    // Keep track of colliding objects for debugging
    private HashSet<Collider> collidingObjects = new HashSet<Collider>();
    private float nextObstacleLogTime = 0f;

    void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Get animator if available
        animator = GetComponent<Animator>();
        
        // Get or add CharacterController with optimized settings
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.Log("Adding CharacterController component automatically");
            characterController = gameObject.AddComponent<CharacterController>();
        }
        
        // Configure CharacterController for better obstacle navigation
        characterController.slopeLimit = slopeLimit;
        characterController.stepOffset = stepOffset;
        characterController.skinWidth = skinWidth;
        characterController.minMoveDistance = 0.001f;
        characterController.center = new Vector3(0, 1.0f, 0);
        characterController.height = 2.0f;
        characterController.radius = 0.35f; // Smaller radius to avoid getting stuck
        
        // Find camera controller
        cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("PlayerController: Cannot find CameraController on main camera!");
        }
        
        hasInitialized = true;
        
        // Perform thorough environment check for obstacles
        if (identifyBlockingObjects)
        {
            DetectNearbyObstacles();
            InvokeRepeating("DetectNearbyObstacles", 5f, 5f); // Check every few seconds
        }
    }

    // Handle Input System movement
    public void onMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        
        // Emergency debug for input
        if (showDebugLogs && moveInput.magnitude > 0.1f)
        {
            Debug.Log($"Input received: {moveInput}");
        }
    }
    
    // Fallback method for direct input if new Input System is not responding
    private void CheckLegacyInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        // If we have input from legacy system, use it
        if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
        {
            moveInput = new Vector2(h, v);
            if (showDebugLogs)
            {
                Debug.Log($"Using legacy input: {moveInput}");
            }
        }
    }

    void Update()
    {
        if (!hasInitialized)
        {
            InitializeComponents();
            return;
        }
        
        // Try fallback input in case new Input System is not working
        CheckLegacyInput();
        
        // Check if we are grounded
        CheckGrounded();
        
        // Check for obstacles
        CheckForObstacles();
        
        // Handle movement
        HandleMovement();
        
        // Apply gravity
        ApplyGravity();
        
        // Apply final movement
        ApplyMovement();
        
        // Update animator if present
        UpdateAnimator();
        
        // Debug key to help unstuck
        if (Input.GetKeyDown(KeyCode.F))
        {
            AttemptUnstuck();
        }
    }
    
    private void DetectNearbyObstacles()
    {
        // Locate all colliders near the player
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 3.0f);
        Debug.Log($"Found {nearbyColliders.Length} colliders near player");
        
        foreach (Collider col in nearbyColliders)
        {
            // Skip the player's own collider
            if (col.gameObject == gameObject) continue;
            
            // Look for potential issues with colliders
            if (col.isTrigger)
            {
                Debug.Log($"Nearby trigger: {col.gameObject.name} - This shouldn't block movement");
            }
            else 
            {
                // Check if this collider is positioned in a way that could block movement
                Vector3 dirToCollider = (col.bounds.center - transform.position).normalized;
                dirToCollider.y = 0; // Only care about horizontal direction
                
                // Calculate the height difference
                float heightDiff = col.bounds.center.y - transform.position.y;
                
                // If the collider is at head level or below feet, it shouldn't block
                if (heightDiff > 2.0f || heightDiff < -0.5f)
                {
                    Debug.Log($"Collider {col.gameObject.name} at height {heightDiff} - should not block movement");
                }
                else 
                {
                    Debug.Log($"Potential obstacle: {col.gameObject.name} at height {heightDiff}, distance: {Vector3.Distance(transform.position, col.bounds.center)}");
                }
            }
        }
    }
    
    private void CheckGrounded()
    {
        // Use multiple methods for more reliable ground detection
        
        // Method 1: Character controller's built-in check
        isGrounded = characterController.isGrounded;
        
        // Method 2: Raycast check if not grounded by method 1
        if (!isGrounded)
        {
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance + 0.1f, groundLayer);
        }
        
        // Method 3: SphereCast for more forgiving ground detection
        if (!isGrounded)
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.SphereCast(origin, 0.3f, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer);
        }
    }
    
    private void CheckForObstacles()
    {
        isObstacleInFront = false;
        collidingObjects.Clear();
        
        // Skip if not trying to move
        if (moveInput.magnitude <= 0.1f) return;
        
        // Get the direction we're trying to move in
        Vector3 moveDir = GetMovementDirection();
        
        // Check for obstacles using multiple raycasts in a radial pattern
        float angleStep = 360f / obstacleDetectionRays;
        for (int i = 0; i < obstacleDetectionRays; i++)
        {
            float angle = i * angleStep;
            // Only check in the forward half-circle to avoid detecting obstacles behind
            if (angle > 90 && angle < 270) continue;
            
            Vector3 checkDir = Quaternion.Euler(0, angle, 0) * moveDir;
            
            // Use increased radius for more reliable obstacle detection
            RaycastHit hit;
            if (Physics.SphereCast(
                transform.position + Vector3.up * 0.5f, 
                obstacleDetectionRadius,
                checkDir, 
                out hit,
                obstacleCheckDistance,
                groundLayer, // Use groundLayer instead of ~0 to avoid unwanted collisions
                QueryTriggerInteraction.Ignore))
            {
                // Ignore triggers and other characters
                if (hit.collider.isTrigger || hit.collider.gameObject.CompareTag("Player")) 
                    continue;
                
                // Add to colliding objects set
                collidingObjects.Add(hit.collider);
                
                // Check if this is a walkable slope
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                
                // Only mark as obstacle if it exceeds slope limit by a good margin
                // (Adding a small buffer to avoid edge cases)
                if (slopeAngle > slopeLimit * 1.2f)
                {
                    isObstacleInFront = true;
                    
                    // Log obstacle info occasionally to avoid spam
                    if (Time.time > nextObstacleLogTime)
                    {
                        nextObstacleLogTime = Time.time + 1f;
                        
                        if (showDebugLogs)
                        {
                            Debug.Log($"Obstacle detected: {hit.collider.name}, slope: {slopeAngle}°, " +
                                      $"distance: {hit.distance}, layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
                        }
                    }
                    
                    // Calculate slide direction if enabled
                    if (useSliding)
                    {
                        moveDir = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;
                    }
                }
            }
        }
    }
    
    private void ApplyGravity()
    {
        if (isGrounded && playerVelocity.y < 0)
        {
            // Apply constant downforce when grounded
            playerVelocity.y = groundedGravity;
        }
        else if (useGravity)
        {
            // Apply gravity when in air
            playerVelocity.y += gravityValue * Time.deltaTime;
            playerVelocity.y = Mathf.Max(playerVelocity.y, -20f); // Limit terminal velocity
        }
    }

    private Vector3 GetMovementDirection()
    {
        // Use direct camera reference if possible
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return Vector3.zero;
        
        // Get camera directions (flattened)
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        
        // Remove Y component and normalize
        cameraForward.y = 0;
        cameraRight.y = 0;
        
        if (cameraForward.magnitude > 0.01f) cameraForward.Normalize();
        if (cameraRight.magnitude > 0.01f) cameraRight.Normalize();
        
        // Calculate move direction relative to camera
        Vector3 direction = (cameraForward * moveInput.y + cameraRight * moveInput.x);
        return direction.normalized;
    }

    private void HandleMovement()
    {
        // Skip if no input
        if (moveInput.magnitude <= 0.1f)
        {
            moveDirection = Vector3.zero;
            velocity = Vector3.zero;
            return;
        }
        
        // Get movement direction from camera
        Vector3 desiredMoveDirection = GetMovementDirection();
        
        // Keep track of velocity for animation
        velocity = desiredMoveDirection * moveInput.magnitude;
        
        // Apply movement speed
        moveDirection = desiredMoveDirection * moveSpeed;
        
        // Rotate towards movement direction
        Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    
    private void ApplyMovement()
    {
        // Combine horizontal movement with vertical velocity
        Vector3 moveVector = new Vector3(moveDirection.x, playerVelocity.y, moveDirection.z);
        
        // Try moving
        CollisionFlags flags = characterController.Move(moveVector * Time.deltaTime);
        
        // If we hit something, try alternative directions
        if ((flags & CollisionFlags.Sides) != 0 && moveDirection.magnitude > 0.1f)
        {
            AttemptSideMovement();
        }
    }
    
    private void AttemptSideMovement()
    {
        if (!useSliding) return;
        
        // Calculate the direction we're facing
        Vector3 forward = transform.forward;
        
        // Try multiple alternative angles if stuck
        float[] slideAngles = new float[] { 45f, -45f, 90f, -90f, 135f, -135f };
        
        foreach (float angle in slideAngles)
        {
            // Calculate new direction
            Vector3 slideDir = Quaternion.Euler(0, angle, 0) * forward;
            
            // Check if direction is clear
            bool clear = true;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, slideDir, obstacleCheckDistance))
            {
                clear = false;
            }
            
            if (clear)
            {
                // Attempt to slide in this direction
                characterController.Move(slideDir * moveSpeed * 0.3f * Time.deltaTime);
                
                if (showDebugLogs)
                {
                    Debug.Log($"Sliding in direction: {angle} degrees");
                }
                
                break;
            }
        }
    }
    
    private void UpdateAnimator()
    {
        if (animator != null)
        {
            // Update speed parameter
            animator.SetFloat("speed", velocity.magnitude);
            
            // Try to update grounded parameter (safe check to avoid errors)
            try {
                animator.SetBool("grounded", isGrounded);
            } catch {
                // Parameter doesn't exist, that's fine
            }
        }
    }
    
    // Attempt to get unstuck if stuck
    private void AttemptUnstuck()
    {
        Debug.Log("Attempting to unstuck player...");
        
        // Try to teleport slightly up and forward
        Vector3 unstuckPosition = transform.position + Vector3.up * 0.5f + transform.forward * 1f;
        
        // Check if the new position is clear
        if (!Physics.CheckSphere(unstuckPosition, characterController.radius * 1.5f))
        {
            // Temporarily disable character controller
            characterController.enabled = false;
            
            // Move to new position
            transform.position = unstuckPosition;
            
            // Re-enable character controller
            characterController.enabled = true;
            
            Debug.Log("Unstuck successful!");
        }
        else
        {
            Debug.Log("Could not find clear position to unstuck player");
        }
        
        // Output colliding objects
        if (collidingObjects.Count > 0)
        {
            Debug.Log("Currently colliding with:");
            foreach (Collider col in collidingObjects)
            {
                Debug.Log($"- {col.gameObject.name} (Layer: {LayerMask.LayerToName(col.gameObject.layer)})");
            }
        }
    }
    
    // Visualize important information
    private void OnDrawGizmos()
    {
        if (!showGizmos || !Application.isPlaying) return;
        
        // Show ground check
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * groundCheckDistance);
        
        // Show movement direction
        if (moveDirection.magnitude > 0.1f)
        {
            Gizmos.color = isObstacleInFront ? Color.red : Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up, moveDirection.normalized);
        }
        
        // Show obstacle detection rays
        if (moveInput.magnitude > 0.1f && Application.isPlaying)
        {
            Vector3 moveDir = GetMovementDirection();
            
            // Draw obstacle detection rays
            float angleStep = 360f / obstacleDetectionRays;
            for (int i = 0; i < obstacleDetectionRays; i++)
            {
                float angle = i * angleStep;
                // Only draw in forward half-circle
                if (angle > 90 && angle < 270) continue;
                
                Vector3 checkDir = Quaternion.Euler(0, angle, 0) * moveDir;
                Gizmos.color = isObstacleInFront ? Color.red : Color.yellow;
                Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, checkDir * obstacleCheckDistance);
            }
        }
        
        // Show character bounds
        if (characterController != null)
        {
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position + characterController.center, characterController.radius);
            
            // Draw character controller height
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Vector3 bottom = transform.position + characterController.center - Vector3.up * characterController.height * 0.5f;
            Vector3 top = transform.position + characterController.center + Vector3.up * characterController.height * 0.5f;
            Gizmos.DrawLine(bottom, top);
        }
    }
    
    // Called when character controller collides
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Add to colliding objects set
        collidingObjects.Add(hit.collider);
        
        if (showDebugLogs && Time.time > nextObstacleLogTime && moveInput.magnitude > 0.1f)
        {
            nextObstacleLogTime = Time.time + 1f;
            Debug.Log($"Character controller hit: {hit.gameObject.name} at point {hit.point}, normal: {hit.normal}");
        }
    }
}
