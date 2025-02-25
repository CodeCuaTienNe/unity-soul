using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2.0f;    // Reduced from 1.5f for shorter jumps
    [SerializeField] private float gravity = -9.81f;     // Real earth gravity
    [SerializeField] private float fallMultiplier = 2.5f; // Makes falling feel better
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpCooldown = 0.1f;
    
    private CharacterController controller;
    private Transform cameraTransform;
    private Vector3 moveDirection = Vector3.zero;
    private bool canJump = true;
    private float jumpCooldownTimer;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        HandleJumpCooldown();
        CheckGroundStatus();
        HandleMovement();
        HandleJump();
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

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 movement = (forward * vertical + right * horizontal).normalized;
        moveDirection = new Vector3(movement.x * moveSpeed, moveDirection.y, movement.z * moveSpeed);
    }

    private void HandleJump()
    {
        if (controller.isGrounded && canJump && Input.GetButtonDown("Jump"))
        {
            // Calculate jump velocity using physics formula: v = sqrt(2 * h * g)
            moveDirection.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
            canJump = false;
            jumpCooldownTimer = 0f;
        }
    }

    private void ApplyGravity()
    {
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
        controller.Move(moveDirection * Time.deltaTime);
    }
}