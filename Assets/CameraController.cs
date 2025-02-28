using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private float lookAheadAmount = 0.1f; // Reduced for stability

    [Header("Zoom Settings")]
    [SerializeField] private float defaultCameraDistance = 5f;
    [SerializeField] private float minCameraDistance = 2f;
    [SerializeField] private float maxCameraDistance = 8f;
    [SerializeField] private float zoomSpeed = 4f;

    [Header("Collision Settings")]
    [SerializeField] private float collisionBuffer = 0.25f;
    [SerializeField] private LayerMask collisionLayers = -1;
    [SerializeField] private float collisionRadius = 0.2f;

    private float currentRotationX;
    private float currentRotationY;
    private Vector3 currentVelocity = Vector3.zero;
    private Transform playerTransform;
    private float currentCameraDistance;
    private Vector3 targetPosition;
    private Vector3 playerVelocity;
    private Vector3 lastPlayerPosition;

    private void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Find player transform
        if (transform.parent != null)
        {
            playerTransform = transform.parent;
            transform.parent = null; // Detach camera from player
        }
        else
        {
            // Try to find player by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Camera: Cannot find player! Tag your player as 'Player'");
                return;
            }
            playerTransform = player.transform;
        }

        currentCameraDistance = defaultCameraDistance;
        lastPlayerPosition = playerTransform.position;
        
        // Initialize camera position
        UpdateCameraPosition(true); // true = instant positioning
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // Calculate player movement direction for look-ahead feature
        CalculatePlayerVelocity();
        
        HandleCameraRotation();
        HandleCameraZoom();
        UpdateCameraPosition(false);
    }

    private void CalculatePlayerVelocity()
    {
        // Calculate frame-to-frame movement with time smoothing
        Vector3 frameMovement = playerTransform.position - lastPlayerPosition;
        playerVelocity = Vector3.Lerp(playerVelocity, frameMovement / Time.deltaTime, Time.deltaTime * 5f);
        lastPlayerPosition = playerTransform.position;
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        currentRotationX += mouseX;
        currentRotationY = Mathf.Clamp(currentRotationY - mouseY, minVerticalAngle, maxVerticalAngle);
    }

    private void HandleCameraZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            currentCameraDistance = Mathf.Clamp(
                currentCameraDistance - scrollInput * zoomSpeed,
                minCameraDistance,
                maxCameraDistance
            );
        }
    }

    private void UpdateCameraPosition(bool instant)
    {
        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        
        // Base position calculation including player offset
        Vector3 playerPos = playerTransform.position + Vector3.up * cameraOffset.y;
        
        // Calculate target camera position
        Vector3 desiredPosition = playerPos + rotation * new Vector3(0, 0, -currentCameraDistance);
        
        // Check for obstacles between player and camera
        RaycastHit hit;
        Vector3 directionToCamera = (desiredPosition - playerPos).normalized;
        float distanceToTarget = Vector3.Distance(playerPos, desiredPosition);
        
        if (Physics.SphereCast(playerPos, collisionRadius, directionToCamera, out hit, distanceToTarget, collisionLayers))
        {
            float adjustedDistance = hit.distance - collisionBuffer;
            desiredPosition = playerPos + directionToCamera * adjustedDistance;
        }

        // Apply position with proper smoothing
        if (instant)
        {
            transform.position = desiredPosition;
            currentVelocity = Vector3.zero;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                smoothTime
            );
        }

        // Point camera at player
        Vector3 lookTarget = playerPos;
        transform.LookAt(lookTarget);
    }

    // Public method to get the camera's rotation for player movement
    public Quaternion GetCameraRotation()
    {
        return Quaternion.Euler(0, currentRotationX, 0);
    }

    // Public method to get the camera's forward direction (without vertical component)
    public Vector3 GetCameraForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        return forward;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();
        return right;
    }

    public float GetCurrentYRotation()
    {
        return currentRotationX;
    }
}