using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float rotationSpeed = 10.0f; // Add this new field
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2.0f;    
    [SerializeField] private float gravity = -9.81f;     
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpCooldown = 0.1f;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;
    
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference dashAction;
    
    private CharacterController controller;
    private Transform cameraTransform;
    private Vector3 moveDirection = Vector3.zero;
    private bool canJump = true;
    private float jumpCooldownTimer;

    // Animation
    private Animator animator;
    private int isWalkingHash;
    private int isRunningHash;

    // Movement input values
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool sprintPressed;
    private bool dashPressed;

    // Sprint and dash properties
    private bool isSprinting = false;
    private bool isDashing = false;
    private float currentDashTime = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
    }

    private void OnEnable()
    {
        // Enable all input actions
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        if (sprintAction != null) sprintAction.action.Enable();
        if (dashAction != null) dashAction.action.Enable();
        
        // Register callbacks - using safer pattern
        RegisterInputCallbacks();
    }

    private void OnDisable()
    {
        // Unregister callbacks
        UnregisterInputCallbacks();
        
        // Disable all input actions
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
        if (sprintAction != null) sprintAction.action.Disable();
        if (dashAction != null) dashAction.action.Disable();
    }
    
    private void RegisterInputCallbacks()
    {
        if (moveAction != null)
        {
            moveAction.action.performed += OnMovePerformed;
            moveAction.action.canceled += OnMoveCanceled;
        }
        
        if (jumpAction != null)
        {
            jumpAction.action.performed += OnJumpPerformed;
        }
        
        if (sprintAction != null) 
        {
            sprintAction.action.performed += OnSprintPerformed;
            sprintAction.action.canceled += OnSprintCanceled;
        }
        
        if (dashAction != null)
        {
            dashAction.action.performed += OnDashPerformed;
        }
    }
    
    private void UnregisterInputCallbacks()
    {
        if (moveAction != null)
        {
            moveAction.action.performed -= OnMovePerformed;
            moveAction.action.canceled -= OnMoveCanceled;
        }
        
        if (jumpAction != null)
        {
            jumpAction.action.performed -= OnJumpPerformed;
        }
        
        if (sprintAction != null)
        {
            sprintAction.action.performed -= OnSprintPerformed;
            sprintAction.action.canceled -= OnSprintCanceled;
        }
        
        if (dashAction != null)
        {
            dashAction.action.performed -= OnDashPerformed;
        }
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    // Input System callback methods - restructured to be safer
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        try
        {
            moveInput = context.ReadValue<Vector2>();
        }
        catch (System.InvalidOperationException)
        {
            // Fallback for when the binding isn't properly set as a Vector2
            Debug.LogWarning("Input binding for movement isn't configured correctly as a Vector2. Please check your Input Action asset.");
            moveInput = Vector2.zero;
        }
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpPressed = true;
    }

    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        sprintPressed = true;
    }
    
    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        sprintPressed = false;
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        dashPressed = true;
    }

    private void Update()
    {
        HandleJumpCooldown();
        HandleDashCooldown();
        CheckGroundStatus();
        HandleSprint();
        HandleMovement();
        HandleJump();
        HandleDash();
        ApplyGravity();
        MoveCharacter();
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        bool isMoving = moveInput.magnitude > 0.1f;
        animator.SetBool(isWalkingHash, isMoving);
        animator.SetBool(isRunningHash, isMoving && isSprinting);
    }

    private void HandleJumpCooldown()
    {
        if (!canJump)
        {
            jumpCooldownTimer += Time.deltaTime;
            if (jumpCooldownTimer >= jumpCooldown)
            {
                canJump = true;
                jumpCooldownTimer = 0f;
            }
        }
    }

    private void HandleDashCooldown()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void CheckGroundStatus()
    {
        // Using SphereCast for better ground detection
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * groundCheckRadius;
        bool isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, 
            out hit, groundCheckDistance, groundLayer);

        // Cannot assign to controller.isGrounded directly as it's read-only
        // Instead, track ground state in our own variable
        bool wasGrounded = controller.isGrounded;
        
        // Apply grounding effect when on ground
        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f; // Small downward force to maintain ground contact
        }
        
        // Handle landing event if needed
        if (!wasGrounded && isGrounded)
        {
            // Player just landed - could trigger landing effects here
        }
    }

    private void HandleSprint()
    {
        isSprinting = sprintPressed && controller.isGrounded;
    }

    private void HandleMovement()
    {
        // Skip movement control during dash
        if (isDashing)
            return;
            
        // Get camera forward and right, but remove Y component to keep movement on ground plane
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        // Zero out the Y component to ensure horizontal movement only
        forward.y = 0;
        right.y = 0;
        
        // Make sure to normalize these vectors to get proper direction
        forward.Normalize();
        right.Normalize();

        // Calculate movement direction based on input
        Vector3 movement = forward * moveInput.y + right * moveInput.x;
        
        // Only normalize if the magnitude is not zero to avoid NaN errors
        if (movement.magnitude > 0.1f)
        {
            movement.Normalize();
        }
        
        // Apply sprint multiplier if sprinting
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        
        // Set horizontal velocity while preserving vertical velocity
        moveDirection = new Vector3(movement.x * currentSpeed, moveDirection.y, movement.z * currentSpeed);

        // Rotate character towards movement direction
        RotateCharacter(movement);
    }

    private void RotateCharacter(Vector3 movement)
    {
        // Only rotate if we have significant movement
        if (movement.magnitude > 0.1f)
        {
            // Create a look rotation based on movement direction
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            
            // Smooth rotation transition
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (controller.isGrounded && canJump && jumpPressed)
        {
            // Calculate jump velocity using physics formula: v = sqrt(2 * h * g)
            moveDirection.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            canJump = false;
            jumpCooldownTimer = 0f;
            jumpPressed = false; // Reset jump press
        }
    }

    private void HandleDash()
    {
        if (isDashing)
        {
            currentDashTime += Time.deltaTime;
            
            if (currentDashTime >= dashDuration)
            {
                isDashing = false;
                currentDashTime = 0f;
                // Return y velocity to prevent floating
                moveDirection.y = controller.isGrounded ? -2f : moveDirection.y;
            }
            return;
        }

        // Initiate dash
        if (dashPressed && dashCooldownTimer <= 0)
        {
            // Get movement direction or forward direction if stationary
            Vector3 moveDir = new Vector3(moveDirection.x, 0, moveDirection.z);
            if (moveDir.magnitude < 0.1f)
                moveDir = transform.forward;
            else
                moveDir.Normalize();
                
            dashDirection = moveDir * (dashDistance / dashDuration);
            isDashing = true;
            dashCooldownTimer = dashCooldown;
            dashPressed = false; // Reset dash press
        }
    }

    private void ApplyGravity()
    {
        // Don't apply gravity during dash
        if (isDashing)
            return;
            
        if (!controller.isGrounded)
        {
            // Apply stronger gravity when falling
            float currentGravity = gravity * (moveDirection.y < 0 ? fallMultiplier : 1f);
            moveDirection.y += currentGravity * Time.deltaTime;
            
            // Cap falling speed
            moveDirection.y = Mathf.Max(moveDirection.y, gravity * fallMultiplier);
        }
    }

    private void MoveCharacter()
    {
        if (isDashing)
        {
            controller.Move(dashDirection * Time.deltaTime);
        }
        else
        {
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
}