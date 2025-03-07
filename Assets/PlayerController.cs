using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // Movement settings
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2.0f;  // New walk speed
    [SerializeField] private float runSpeed = 7.0f;   // Renamed from moveSpeed
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

    // Add these new variables to your class (after the existing movement settings)
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float jumpForce = 10f; // Direct force for more precise control
    [SerializeField] private float jumpAnimationDuration = 0.8f; // How long the jump animation should play
    [SerializeField] private float preJumpDelay = 0.1f; // Small delay before applying physics
    [SerializeField] private float landingDelay = 0.2f; // How long to play landing animation
    private bool isJumping = false;
    private bool isJumpAnimationPlaying = false;
    private float jumpCooldownTimer = 0f;
    private float jumpAnimationTimer = 0f;

    // Add these variables to your class (after existing animation-related variables)
    [Header("Animation Settings")]
    [SerializeField] private float jumpAnimationDelay = 0f;  // Keep at 0 to start immediately
    [SerializeField] private bool useJumpTrigger = true;     // Use trigger instead of bool for more precise control
    private bool jumpAnimationStarted = false;

    // Dash settings
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 5.0f;  // Keep current speed
    [SerializeField] private float dashDuration = 1f;  // Changed from 4f to 0.3f for a quick dash
    [SerializeField] private float dashCooldown = 2.0f;
    [SerializeField] private bool useDashTrail = false;
    [SerializeField] private bool useGravityDuringDash = true;  // New parameter
    private bool isDashing = false;
    private bool canDash = true;
    private float dashCooldownTimer = 0f;
    private float dashTimer = 0f;
    private Vector3 dashDirection;
    private TrailRenderer dashTrail;

    // Add these fields at the top of the class with other variables
    [Header("Hit Settings")]
    [SerializeField] private float hitAnimationDuration = 2f; // Duration of hit animation
    [SerializeField] private bool useAnimationEvents = false; // Optional: Use animation events instead of timer
    private bool isHitting = false;
    private float hitTimer = 0f;
    private bool canHit = true; // Add cooldown check

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
    private bool isRunning = false;  // New parameter to track running state
    private float currentMoveSpeed;  // New parameter to store current speed
    
    // Keep track of colliding objects for debugging
    private HashSet<Collider> collidingObjects = new HashSet<Collider>();
    private float nextObstacleLogTime = 0f;

    void Start()
    {
        InitializeComponents();
        currentMoveSpeed = walkSpeed; // Default to walking speed
        
        // Only create trail renderer if useDashTrail is true
        if (useDashTrail)
        {
            dashTrail = GetComponent<TrailRenderer>();
            if (dashTrail == null)
            {
                dashTrail = gameObject.AddComponent<TrailRenderer>();
                dashTrail.startWidth = 0.5f;
                dashTrail.endWidth = 0.0f;
                dashTrail.time = dashDuration;
                dashTrail.material = new Material(Shader.Find("Sprites/Default"));
                dashTrail.startColor = new Color(1f, 1f, 1f, 0.5f);
                dashTrail.endColor = new Color(1f, 1f, 1f, 0f);
                dashTrail.enabled = false;
            }
        }
        else
        {
            // Remove trail renderer if it exists and we don't want to use it
            dashTrail = GetComponent<TrailRenderer>();
            if (dashTrail != null)
            {
                Destroy(dashTrail);
            }
        }
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
    
    // New method to handle Run input
    public void onRun(InputAction.CallbackContext context)
    {
        // Set running state based on button press/release
        if (context.started || context.performed)
        {
            isRunning = true;
            currentMoveSpeed = runSpeed;
            
            if (showDebugLogs)
            {
                Debug.Log("Running started");
            }
        }
        else if (context.canceled)
        {
            isRunning = false;
            currentMoveSpeed = walkSpeed;
            
            if (showDebugLogs)
            {
                Debug.Log("Running stopped");
            }
        }
    }
    
    // New method to handle Dash input
    public void onDash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && !isDashing && isGrounded)
        {
            StartDash();
        }
    }
    
    // Method to start dash
    private void StartDash()
    {
        // Use movement input if available, otherwise use facing direction
        if (moveInput.magnitude > 0.1f)
        {
            dashDirection = GetMovementDirection().normalized;
        }
        else
        {
            // Use the direction the player is facing when no movement input
            dashDirection = transform.forward;
        }

        // Add a small forward offset to the dash direction
        dashDirection = (dashDirection + transform.forward * 0.5f).normalized;

        isDashing = true;
        canDash = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        
        if (showDebugLogs)
        {
            Debug.Log("Dash started");
        }
        
        // Enable trail effect if available
        if (dashTrail != null && useDashTrail)
        {
            dashTrail.enabled = true;
        }
        
        // Trigger dash animation
        if (animator != null)
        {
            animator.SetTrigger("Dash");
        }
    }
    
    // Replace onJump method with this improved version
    public void onJump(InputAction.CallbackContext context)
    {
        // Only jump on button press (not release), when grounded and not in cooldown
        if (context.performed && isGrounded && jumpCooldownTimer <= 0 && !isJumping)
        {
            StartJumpSequence();
        }
    }

    // New method to handle the jump sequence
    private void StartJumpSequence()
    {
        // Set jump state immediately
        isJumping = true;
        isJumpAnimationPlaying = true;
        jumpAnimationTimer = 0f;
        jumpCooldownTimer = jumpCooldown;
        
        // Start the jump animation immediately
        if (animator != null)
        {
            // Reset any animation state that might interfere
            animator.SetBool("isJumping", true);
            animator.speed = 1.0f;
            
            if (showDebugLogs)
            {
                Debug.Log("Jump animation started");
            }
        }
        
        // Apply physics after a tiny delay
        StartCoroutine(ApplyJumpPhysicsAfterDelay());
    }

    // Coroutine to apply jump physics with precise timing
    private IEnumerator ApplyJumpPhysicsAfterDelay()
    {
        // Wait for the pre-animation (anticipation phase)
        yield return new WaitForSeconds(preJumpDelay);
        
        // Calculate and apply jump force directly
        float jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravityValue) * jumpHeight);
        playerVelocity.y = jumpVelocity;
        
        if (showDebugLogs)
        {
            Debug.Log($"Jump physics applied with velocity: {jumpVelocity}");
        }
    }

    // Add this method to handle the new input system hit action
    public void onHit(InputAction.CallbackContext context)
    {
        if (context.performed && !isHitting)
        {
            StartHit();
        }
    }

    // Add this method to start the hit sequence
    private void StartHit()
    {
        if (!canHit) return;
        
        isHitting = true;
        canHit = false;
        hitTimer = hitAnimationDuration;
        
        if (animator != null)
        {
            // Use trigger instead of bool for better animation control
            animator.SetTrigger("Hit");
            animator.SetBool("isHitting", true);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"Hit started, duration: {hitAnimationDuration}");
        }
    }

    // Similarly, update the legacy input method
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
        
        // Check for run using legacy input system as backup
        bool runPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (runPressed != isRunning)
        {
            isRunning = runPressed;
            currentMoveSpeed = isRunning ? runSpeed : walkSpeed;
        }
        
        // Check for jump using legacy input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && jumpCooldownTimer <= 0 && !isJumping)
        {
            StartJumpSequence();
        }
        
        // Check for dash using legacy input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing && isGrounded)
        {
            StartDash();
        }

        // Add hit check using legacy input
        if (Input.GetMouseButtonDown(0) && !isHitting)
        {
            StartHit();
        }
    }

    // Update method changes for better jump animation tracking
    void Update()
    {
        if (!hasInitialized)
        {
            InitializeComponents();
            return;
        }
        
        // Update hit state
        if (isHitting)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0)
            {
                EndHit();
            }
            
            // Check if hit animation is actually complete
            if (animator != null)
            {
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
                if (currentState.IsName("Hit") && currentState.normalizedTime >= 1.0f)
                {
                    EndHit();
                }
            }
        }
        
        // Update jump cooldown timer
        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.deltaTime;
        }
        
        // Update dash timers
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
        }
        
        // Update dash cooldown
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
            }
        }
        
        // Update jump animation timer
        if (isJumpAnimationPlaying)
        {
            jumpAnimationTimer += Time.deltaTime;
            
            // Check if jump animation should end based on timer or landing
            if ((jumpAnimationTimer >= jumpAnimationDuration && isGrounded) || 
                (isGrounded && jumpAnimationTimer > jumpAnimationDuration * 0.6f && playerVelocity.y < 0.1f))
            {
                StartCoroutine(EndJumpAnimation());
                isJumpAnimationPlaying = false;
            }
        }
        
        // Try fallback input in case new Input System is not working
        CheckLegacyInput();
        
        // Check if we are grounded
        CheckGrounded();
        
        // Handle jump state completion when landing
        if (isGrounded && isJumping && playerVelocity.y < 0.1f && jumpAnimationTimer > jumpAnimationDuration * 0.6f)
        {
            isJumping = false;
        }
        
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

    // Modified EndJumpAnimation to handle the landing phase better
    private IEnumerator EndJumpAnimation()
    {
        // Wait for landing animation to complete
        yield return new WaitForSeconds(landingDelay);
        
        // Reset jump state
        isJumping = false;
        
        // Reset animation state
        if (animator != null)
        {
            animator.SetBool("isJumping", false);
        }
        
        if (showDebugLogs)
        {
            Debug.Log("Jump animation ended");
        }
    }

    // Update the animator method for better synchronization
    private void UpdateAnimator()
    {
        if (animator == null) return;
        
        // --- Movement Animation ---
        float normalizedSpeed = 0;
        
        // Only update movement speed when not jumping or in the landing phase
        if (velocity.magnitude > 0.1f && (!isJumping || (isGrounded && playerVelocity.y < 0.1f)))
        {
            normalizedSpeed = isRunning ? 1.0f : 0.5f;
        }
        
        // Smooth the speed parameter change
        float currentSpeed = animator.GetFloat("speed");
        float speedSmoothRate = isJumping ? 15f : 10f; // Faster transitions during jumps
        animator.SetFloat("speed", Mathf.Lerp(currentSpeed, normalizedSpeed, Time.deltaTime * speedSmoothRate));
        
        // Update jump parameters
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isGrounded", isGrounded);
        
        // Set the running state
        animator.SetBool("isRunning", isRunning);
        
        // Set dash state and speed up the animation
        if (isDashing)
        {
            animator.SetBool("isDashing", true);
            // Make sure this animation speed calculation uses the correct dashDuration value
            animator.speed = 2f / dashDuration;  
            
            // Check if dash animation is finished
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (currentState.IsName("Dash") && currentState.normalizedTime >= 1.0f)
            {
                EndDash();
            }
        }
        else
        {
            animator.SetBool("isDashing", false);
            animator.speed = 1f;  // Reset to normal speed when not dashing
        }
        
        // Add jump progress for animation blending (0 to 1 value through jump animation)
        float jumpProgress = isJumping ? Mathf.Clamp01(jumpAnimationTimer / jumpAnimationDuration) : 0f;
        animator.SetFloat("jumpProgress", jumpProgress);
        
        // Track vertical velocity for blend trees
        animator.SetFloat("verticalVelocity", playerVelocity.y);

        // Add hit animation state
        animator.SetBool("isHitting", isHitting);

        // Update hit state
        if (isHitting)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (currentState.IsName("Hit"))
            {
                // Keep the animation playing at normal speed
                animator.speed = 1.0f;
                
                // Optional: Debug log for animation progress
                if (showDebugLogs && Time.frameCount % 30 == 0) // Log every 30 frames
                {
                    Debug.Log($"Hit animation progress: {currentState.normalizedTime}");
                }
            }
        }
    }
    
    private void EndDash()
    {
        isDashing = false;
        if (dashTrail != null && useDashTrail)
        {
            dashTrail.enabled = false;
        }
        animator.SetBool("isDashing", false);
        animator.speed = 1f;
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
        
        // Keep track of velocity for animation - Normalize for direction but keep magnitude for speed blend
        velocity = desiredMoveDirection * moveInput.magnitude;
        
        // Apply current movement speed (walk or run)
        moveDirection = desiredMoveDirection * currentMoveSpeed;
        
        // Rotate towards movement direction
        Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
    
    private void ApplyMovement()
    {
        Vector3 moveVector;
        
        if (isDashing)
        {
            // Apply dash movement but maintain gravity influence
            float verticalVelocity = useGravityDuringDash ? playerVelocity.y : 0f;
            moveVector = new Vector3(dashDirection.x * dashSpeed, verticalVelocity, dashDirection.z * dashSpeed);
        }
        else
        {
            // Regular movement - combine horizontal movement with vertical velocity
            moveVector = new Vector3(moveDirection.x, playerVelocity.y, moveDirection.z);
        }
        
        // Try moving
        CollisionFlags flags = characterController.Move(moveVector * Time.deltaTime);
        
        // If we hit something, try alternative directions
        if ((flags & CollisionFlags.Sides) != 0 && moveDirection.magnitude > 0.1f && !isDashing)
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
                // Use currentMoveSpeed instead of the removed moveSpeed variable
                characterController.Move(slideDir * currentMoveSpeed * 0.3f * Time.deltaTime);
                
                if (showDebugLogs)
                {
                    Debug.Log($"Sliding in direction: {angle} degrees");
                }
                
                break;
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
        
        // Show dash direction if dashing
        if (isDashing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up, dashDirection * dashSpeed * 0.5f);
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
            // Define the top variable
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

    private void EndHit()
    {
        isHitting = false;
        if (animator != null)
        {
            animator.SetBool("isHitting", false);
        }
        
        // Add small delay before allowing next hit
        StartCoroutine(ResetHitCooldown());
        
        if (showDebugLogs)
        {
            Debug.Log("Hit ended");
        }
    }

    private IEnumerator ResetHitCooldown()
    {
        yield return new WaitForSeconds(0.1f); // Small buffer between hits
        canHit = true;
    }

    // Optional: Animation Event methods
    public void OnHitAnimationComplete()
    {
        if (useAnimationEvents)
        {
            EndHit();
        }
    }
}

