using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private float smoothTime = 0.15f;
    // Look-ahead has been removed

    [Header("Distance Setting")]
    [SerializeField] private float cameraDistance = 5f;

    // Collision settings removed as collision detection is disabled

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    [SerializeField] private float decreaseFactor = 1.0f;

    private Vector3 originalPosition;
    private float currentShakeDuration = 0f;
    private bool isShaking = false;

    private float currentRotationX;
    private float currentRotationY;
    private Vector3 currentVelocity = Vector3.zero;
    private Transform playerTransform;
    private Vector3 targetPosition;
    // Player velocity variables removed

    // Input variables
    private float mouseX;
    private float mouseY;

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


        // Initialization of player velocity tracking removed

        // Initialize camera position
        UpdateCameraPosition(true); // true = instant positioning
    }

    private void Update()
    {
        // Process input in Update for more consistent input handling
        ProcessInput();
    }

    private void ProcessInput()
    {
        // Get input values - using GetAxis which is generally processed in Update
        mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Toggle cursor lock with Escape key as a useful addition
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }

    private void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        // Look-ahead feature has been removed

        // Apply the input that was gathered in Update
        ApplyCameraRotation();

        // Update the camera position
        UpdateCameraPosition(false);

        // Don't reset position if we're in the middle of a shake
        if (isShaking)
        {
            // The DoShake coroutine is handling position
            return;
        }
    }

    // Player velocity calculation has been removed as it's no longer needed

    private void ApplyCameraRotation()
    {
        // Apply the cached input values from Update
        currentRotationX += mouseX;
        currentRotationY = Mathf.Clamp(currentRotationY - mouseY, minVerticalAngle, maxVerticalAngle);
    }



    private void UpdateCameraPosition(bool instant)
    {
        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);

        // Base position calculation including player offset
        Vector3 playerPos = playerTransform.position + Vector3.up * cameraOffset.y;

        // Calculate target camera position without look-ahead
        Vector3 desiredPosition = playerPos + rotation * new Vector3(0, 0, -cameraDistance);

        // Collision detection has been completely disabled
        // No obstacles will affect camera position

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

    public void ResetCamera()
    {
        // Reset rotation and position to defaults
        currentRotationX = 0f;
        currentRotationY = 0f;
        UpdateCameraPosition(true);
    }

    public void ShakeCamera()
    {
        ShakeCamera(shakeDuration, shakeMagnitude);
    }

    /// <summary>
    /// Initiates a camera shake effect with custom parameters
    /// </summary>
    /// <param name="duration">How long the shake effect lasts in seconds</param>
    /// <param name="magnitude">How intense the shake is</param>
    public void ShakeCamera(float duration, float magnitude)
    {
        originalPosition = transform.localPosition;
        currentShakeDuration = duration;
        StartCoroutine(DoShake(magnitude));
    }

    private IEnumerator DoShake(float magnitude)
    {
        isShaking = true;

        while (currentShakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * magnitude;

            // Store the position that was calculated in UpdateCameraPosition
            Vector3 cameraTargetPosition = transform.position;

            // Apply shake offset
            transform.position = cameraTargetPosition + shakeOffset;

            // Reduce shake duration over time
            currentShakeDuration -= Time.deltaTime * decreaseFactor;

            yield return null;
        }

        // Ensure we stop at exactly 0 to avoid tiny lingering effects
        currentShakeDuration = 0f;
        isShaking = false;
    }
}