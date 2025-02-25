using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
[SerializeField] private float minVerticalAngle = -30f;
[SerializeField] private float maxVerticalAngle = 60f;
[SerializeField] private float rotationSpeed = 2f; // Reduced for smoother rotation
[SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2f, -4f); // Adjusted for better view
[SerializeField] private float smoothTime = 0.05f; // Reduced for more responsive following

[Header("Zoom Settings")]
[SerializeField] private float defaultCameraDistance = 5f;
[SerializeField] private float minCameraDistance = 2f;
[SerializeField] private float maxCameraDistance = 8f;
[SerializeField] private float zoomSpeed = 4f;


    private float currentRotationX;
    private float currentRotationY;
    private Vector3 currentVelocity;
    private Transform playerTransform;
    private float currentCameraDistance;
    private Vector3 targetPosition;

    private void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerTransform = transform.parent;
        if (playerTransform == null)
        {
            Debug.LogError("Camera must be a child of the player object!");
            return;
        }

        currentCameraDistance = defaultCameraDistance;
        // Initialize camera position
        UpdateCameraPosition();
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        HandleCameraRotation();
        HandleCameraZoom();
        UpdateCameraPosition();
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        currentRotationX += mouseX;
        currentRotationY = Mathf.Clamp(currentRotationY - mouseY, minVerticalAngle, maxVerticalAngle);

        // Calculate the rotation around the player
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        targetPosition = playerTransform.position + rotation * cameraOffset.normalized * currentCameraDistance;
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

    private void UpdateCameraPosition()
{
    // Check for obstacles between player and camera
    RaycastHit hit;
    Vector3 desiredPosition = targetPosition;
    Vector3 directionToCamera = (targetPosition - playerTransform.position).normalized;
    float distanceToTarget = Vector3.Distance(playerTransform.position, targetPosition);
    
    if (Physics.SphereCast(playerTransform.position + Vector3.up * cameraOffset.y, 0.2f, directionToCamera, out hit, distanceToTarget))
    {
        desiredPosition = hit.point + hit.normal * 0.2f; // Push camera slightly away from collision
    }

    // Update position with smoothing
    transform.position = Vector3.SmoothDamp(
        transform.position,
        desiredPosition,
        ref currentVelocity,
        smoothTime
    );

    // Make camera look at player's upper body
    transform.LookAt(playerTransform.position + Vector3.up * cameraOffset.y);
}

    public float GetCurrentYRotation()
    {
        return currentRotationX;
    }
}