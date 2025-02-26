using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2.0f;    // Reduced from 1.5f for shorter jumps
    [SerializeField] private float gravity = -9.81f;     // Real earth gravity
    [SerializeField] private float fallMultiplier = 2.5f; // Makes falling feel better
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpCooldown = 0.1f;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;
    
    private CharacterController controller;
    private Transform cameraTransform;
    private Vector3 moveDirection = Vector3.zero;
    private bool canJump = true;
    private float jumpCooldownTimer;
    
    // Sprint and dash properties
    private bool isSprinting = false;
    private bool isDashing = false;
    private float currentDashTime = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
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

        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f; // Small downward force to maintain ground contact
        }
    }

    private void HandleSprint()
    {
        // Use left shift for sprinting instead of space
        isSprinting = Input.GetKey(KeyCode.LeftControl) && controller.isGrounded;
    }

    private void HandleMovement()
    {
        // Skip movement control during dash
        if (isDashing)
            return;
            
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 movement = (forward * vertical + right * horizontal).normalized;
        
        // Apply sprint multiplier if sprinting
        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        
        moveDirection = new Vector3(movement.x * currentSpeed, moveDirection.y, movement.z * currentSpeed);

        // Rotate character towards movement direction
        RotateCharacter(movement);
    }

    private void RotateCharacter(Vector3 movement)
    {
        if (movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, moveSpeed * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        // Jump on spacebar press (removed the !isSprinting check)
        if (controller.isGrounded && canJump && Input.GetKeyDown(KeyCode.Space))
        {
            // Calculate jump velocity using physics formula: v = sqrt(2 * h * g)
            moveDirection.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            canJump = false;
            jumpCooldownTimer = 0f;
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

        // Initiate dash on shift press
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0)
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