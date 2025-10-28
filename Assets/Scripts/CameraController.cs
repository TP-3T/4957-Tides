using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls camera functionality
///
/// - Horizontal Motion
/// - Rotation
/// - Zoom
/// - Panning
/// </summary>
public class CameraController : MonoBehaviour
{
    const float MAX_SPEED = 20f;
    const float ACCELERATION = 10f;
    const float STEP_SIZE = 2f;
    const float DAMPING = 15f;
    const float ZOOM_DAMPING = 7.5f;
    const float ZOOM_SPEED = 2f;
    const float ZOOM_INPUT_MODIFIER = 100f;
    const float ZOOM_DELTA_THRESHOLD = 0.1f;
    const float MIN_HEIGHT = 10f;
    const float MAX_HEIGHT = 50f;
    const float MAX_ROTATION_SPEED = 0.25f;
    const float ROTATION_X = 0f;
    const float ROTATION_Z = 0f;
    const float SCREEN_EDGE_TOLERANCE = 0.05f;
    const float MAGNITUDE_THRESHOLD = 0.001f;
    const float NO_VERTICAL_VELOCITY = 0f;

    [SerializeField]
    private Transform cameraTransform;
    private CameraControlActions cameraActions;
    private InputAction movement;
    private float speed;
    private bool useScreenEdge = true; // Toggle on and off
    private float zoomHeight;
    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;
    private Vector3 targetPosition;

    /// <summary>
    /// Initializes the camera controller.
    /// </summary>
    private void Awake()
    {
        cameraActions = new CameraControlActions();

        if (cameraTransform == null)
        {
            cameraTransform = this.gameObject.transform;
        }
    }

    /// <summary>
    /// OnEnable is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        zoomHeight = cameraTransform.localPosition.y;
        cameraTransform.LookAt(this.transform);
        lastPosition = this.transform.position;
        movement = cameraActions.Camera.Movement;

        // Subscribe to the performed events of the camera actions.
        cameraActions.Camera.RotateCamera.performed += RotateCamera;
        cameraActions.Camera.ZoomCamera.performed += ZoomCamera;

        // The name of the action map is "Camera". Enables the action map.
        cameraActions.Camera.Enable();
    }

    /// <summary>
    /// OnDisable is called when the object becomes disabled.
    ///
    /// Turns off action map if object is disabled to not have unwanted behaviour.
    /// </summary>
    private void OnDisable()
    {
        // Unsubscribe from the performed events of the camera actions.
        cameraActions.Camera.RotateCamera.performed -= RotateCamera;
        cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;

        cameraActions.Disable();
    }

    /// <summary>
    /// Called once per frame to update.
    /// </summary>
    private void Update()
    {
        GetKeyboardMovement();

        UpdateVelocity();

        UpdateCameraPos();

        UpdateBasePosition();
    }

    /// <summary>
    /// Updates the camera velocity based on its position.
    ///
    /// Velocity = displacement / time
    /// </summary>
    private void UpdateVelocity()
    {
        horizontalVelocity = (this.transform.position - lastPosition) / Time.deltaTime;

        horizontalVelocity.z = NO_VERTICAL_VELOCITY;

        lastPosition = this.transform.position;
    }

    /// <summary>
    /// Handles keyboard movement input for the camera.
    ///
    /// The GetCameraRight and GetCameraLeft are used to determine the relative
    /// directions based on the camera's current orientation.
    ///
    /// normalizing the input makes the vector of length 1, preventing unwanted
    /// speed increases and making speed consistent. (e.g., moving diagonally
    /// doesn't affect speed)
    ///
    /// If the input magnitude is greater than a small threshold (0.1f),
    /// the target position is updated by adding the input value.
    /// </summary>
    private void GetKeyboardMovement()
    {
        Vector3 inputValue;

        inputValue =
            movement.ReadValue<Vector2>().x * GetCameraRight()
            + movement.ReadValue<Vector2>().y * GetCameraForward();

        inputValue = inputValue.normalized;

        if (inputValue.sqrMagnitude > MAGNITUDE_THRESHOLD)
        {
            // adding to target position, not moving to it.
            targetPosition += inputValue;
        }
    }

    /// <summary>
    /// Gets the right direction relative to the camera's orientation.
    /// </summary>
    /// <returns>Vector3 right</returns>
    private Vector3 GetCameraRight()
    {
        Vector3 right;

        right = cameraTransform.right;
        right.y = NO_VERTICAL_VELOCITY;

        return right;
    }

    /// <summary>
    /// Gets the left direction relative to the camera's orientation.
    /// </summary>
    /// <returns>Vector3 forward</returns>
    private Vector3 GetCameraForward()
    {
        Vector3 forward;

        forward = cameraTransform.forward;
        forward.y = NO_VERTICAL_VELOCITY;

        return forward;
    }

    /// <summary>
    /// Updates the base position of the camera based on target position.
    /// </summary>
    private void UpdateBasePosition()
    {
        if (targetPosition.sqrMagnitude > MAGNITUDE_THRESHOLD)
        {
            speed = Mathf.Lerp(speed, MAX_SPEED, ACCELERATION * Time.deltaTime);

            transform.position += speed * Time.deltaTime * targetPosition.normalized;
        }
        else
        {
            horizontalVelocity = Vector3.Lerp(
                horizontalVelocity,
                Vector3.zero,
                DAMPING * Time.deltaTime
            );

            transform.position += horizontalVelocity * Time.deltaTime;
        }

        targetPosition = Vector3.zero;
    }

    /// <summary>
    /// Rotates the camera based on mouse input.
    /// </summary>
    /// <param name="inputVal"></param>
    private void RotateCamera(InputAction.CallbackContext inputVal)
    {
        if (!Mouse.current.middleButton.isPressed)
        {
            return;
        }
        else
        {
            float mouseDeltaX;
            float rotationY;

            mouseDeltaX = inputVal.ReadValue<Vector2>().x;

            rotationY = mouseDeltaX * MAX_ROTATION_SPEED + transform.rotation.eulerAngles.y;

            transform.rotation = Quaternion.Euler(ROTATION_X, rotationY, ROTATION_Z);
        }
    }

    /// <summary>
    /// Zooms the camera in and out based on mouse scroll input.
    /// </summary>
    /// <param name="inputVal"></param>
    private void ZoomCamera(InputAction.CallbackContext inputVal)
    {
        float zoomDelta;

        zoomDelta = inputVal.ReadValue<Vector2>().y / ZOOM_INPUT_MODIFIER;
        // zoomDelta = inputVal.ReadValue<Vector2>().y;

        Debug.Log("Zoom Delta: " + zoomDelta);

        if (Mathf.Abs(zoomDelta) > ZOOM_DELTA_THRESHOLD)
        {
            Debug.Log("INSIDE ZOOM");
            zoomHeight = cameraTransform.localPosition.y + zoomDelta * STEP_SIZE;

            if (zoomHeight < MIN_HEIGHT)
            {
                zoomHeight = MIN_HEIGHT;
            }
            else if (zoomHeight > MAX_HEIGHT)
            {
                zoomHeight = MAX_HEIGHT;
            }
        }
    }

    /// <summary>
    /// Updates the camera position for zooming.
    /// </summary>
    private void UpdateCameraPos()
    {
        Vector3 zoomTarget;

        zoomTarget = new Vector3(
            cameraTransform.localPosition.x,
            zoomHeight,
            cameraTransform.localPosition.z
        );

        zoomTarget -= ZOOM_SPEED * (zoomHeight - cameraTransform.localPosition.y) * Vector3.forward;

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            zoomTarget,
            ZOOM_DAMPING * Time.deltaTime
        );

        cameraTransform.LookAt(this.transform);
    }
}
