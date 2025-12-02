using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls camera functionality
///
/// - Horizontal Motion
/// - Rotation
/// - Zoom
/// - Panning
/// - Dragging
/// </summary>
namespace TTT.Player
{
    public class CameraController : MonoBehaviour
    {
        const float MAX_SPEED = 20f;
        const float ACCELERATION = 10f;
        const float STEP_SIZE = 10f;
        const float DAMPING = 15f;
        const float ZOOM_DAMPING = 60f;
        const float ZOOM_MOMENTUM_DAMPING = 5f;
        const float ZOOM_DELTA_THRESHOLD = 0.1f;
        const float MIN_HEIGHT = 10f;
        const float MAX_HEIGHT = 100f;
        const float MAX_ROTATION_SPEED = 0.25f;
        const float ROTATION_X = 0f;
        const float ROTATION_Z = 0f;
        const float SCREEN_EDGE_TOLERANCE = 0.01f;
        const float SCREEN_EDGE_MAX = 1f;
        const float SCREEN_MIDPOINT_DIVISOR = 2f;
        const float MAGNITUDE_THRESHOLD = 0.001f;
        const float NO_VERTICAL_VELOCITY = 0f;
        const float NO_ZOOM_VELOCITY = 0f;
        const float TILTING_MOVEMENT_FACTOR = 0.7f;
        const float TERRAIN_RAYCAST_HEIGHT_OFFSET = 50f;
        const float TERRAIN_RAYCAST_DISTANCE = 100f;
        const float TERRAIN_HEIGHT_SMOOTH_SPEED = 3f;
        const float ZOOM_VELOCITY_BENCHMARK = 0.01f;
        const float DRAG_THRESHOLD = 0.1f; // Minimum distance to consider it a drag vs click

        // [SerializeField]
        private Transform cameraTransform;

        [SerializeField]
        private Camera playerCamera;

        [SerializeField]
        private CameraControlActions cameraActions;
        private InputAction movement;
        private float speed;
        private readonly bool useScreenEdge = false; // Toggle on and off //! po: is this not always false
        private float zoomHeight;
        private float zoomVelocity;
        private float dynamicMinHeight = MIN_HEIGHT;
        private float targetMinHeight = MIN_HEIGHT;
        private Vector3 horizontalVelocity;
        private Vector3 lastPosition;
        private Vector3 targetPosition;
        private Vector3 startDrag;
        private bool isDragging = false;

        /// <summary>
        /// Returns true if the camera is currently being dragged by the user.
        /// </summary>
        public bool IsDragging => isDragging;

        public void Start()
        {
            cameraActions = new();
            cameraActions.Camera.Enable();
            cameraTransform = playerCamera.transform;
            zoomHeight = cameraTransform.localPosition.y;
            cameraTransform.LookAt(this.transform);
            lastPosition = this.transform.position;
            movement = cameraActions.Camera.Movement;

            // Subscribe to the performed events of the camera actions.
            cameraActions.Camera.RotateCamera.performed += RotateCamera;
            cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        }

        /// <summary>
        /// Called once per frame to update.
        /// </summary>
        private void Update()
        {
            GetKeyboardMovement();

            CheckMouseAtScreenEdge();

            DragCamera();

            UpdateVelocity();

            CheckTerrainHeight();

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
            horizontalVelocity =
                (this.transform.position - lastPosition) / Time.deltaTime;

            horizontalVelocity.z = NO_VERTICAL_VELOCITY; //! po: z i think is depth not vertical, why is this z not y

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
                speed = Mathf.Lerp(
                    speed,
                    MAX_SPEED,
                    ACCELERATION * Time.deltaTime
                );

                transform.position +=
                    speed * Time.deltaTime * targetPosition.normalized;
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
            if (!Mouse.current.rightButton.isPressed)
            {
                return;
            }
            else
            {
                float mouseDeltaX;
                float rotationY;

                mouseDeltaX = inputVal.ReadValue<Vector2>().x;

                rotationY =
                    mouseDeltaX * MAX_ROTATION_SPEED
                    + transform.rotation.eulerAngles.y;

                transform.rotation = Quaternion.Euler(
                    ROTATION_X,
                    rotationY,
                    ROTATION_Z
                );
            }
        }

        /// <summary>
        /// Zooms the camera in and out based on mouse scroll input.
        /// </summary>
        /// <param name="inputVal"></param>
        private void ZoomCamera(InputAction.CallbackContext inputVal)
        {
            float zoomDelta;

            zoomDelta = inputVal.ReadValue<Vector2>().y;

            if (Mathf.Abs(zoomDelta) > ZOOM_DELTA_THRESHOLD)
            {
                zoomVelocity += zoomDelta * STEP_SIZE;
            }
        }

        /// <summary>
        /// Updates the camera position for zooming.
        /// </summary>
        private void UpdateCameraPos()
        {
            Vector3 zoomTarget;

            // VELOCITY
            if (Mathf.Abs(zoomVelocity) > ZOOM_VELOCITY_BENCHMARK)
            {
                zoomHeight -= zoomVelocity * Time.deltaTime;

                // MOMENTUM DECAY
                zoomVelocity = Mathf.Lerp(
                    zoomVelocity,
                    NO_ZOOM_VELOCITY,
                    ZOOM_MOMENTUM_DAMPING * Time.deltaTime
                );
            }
            else
            {
                zoomVelocity = NO_ZOOM_VELOCITY;
            }

            // zoomHeight = Mathf.Clamp(zoomHeight, MIN_HEIGHT, MAX_HEIGHT);
            zoomHeight = Mathf.Clamp(zoomHeight, dynamicMinHeight, MAX_HEIGHT);

            zoomTarget = new Vector3(
                cameraTransform.localPosition.x,
                zoomHeight,
                cameraTransform.localPosition.z
            );

            float heightDifference =
                zoomHeight - cameraTransform.localPosition.y;
            float backwardOffset = heightDifference * TILTING_MOVEMENT_FACTOR;

            zoomTarget -= backwardOffset * Vector3.forward;

            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                zoomTarget,
                ZOOM_DAMPING * Time.deltaTime
            );

            cameraTransform.LookAt(this.transform);
        }

        /// <summary>
        /// Checks if the mouse is at the edge of the screen to pan the camera.
        /// </summary>
        private void CheckMouseAtScreenEdge()
        {
            if (useScreenEdge)
            // po: this is for a future toggle in settings, according to rodrigo
            {
                Vector2 mousePos;
                Vector2 screenCenter;
                Vector2 screenDelta;
                Vector3 moveDirection;

                mousePos = Mouse.current.position.ReadValue();

                screenCenter = new Vector2(
                    Screen.width / SCREEN_MIDPOINT_DIVISOR,
                    Screen.height / SCREEN_MIDPOINT_DIVISOR
                );

                screenDelta = (mousePos - screenCenter) / screenCenter;

                moveDirection = Vector3.zero;

                if (
                    Mathf.Abs(screenDelta.x)
                        > SCREEN_EDGE_MAX - SCREEN_EDGE_TOLERANCE
                    || Mathf.Abs(screenDelta.y)
                        > SCREEN_EDGE_MAX - SCREEN_EDGE_TOLERANCE
                )
                {
                    moveDirection =
                        GetCameraRight() * screenDelta.x
                        + GetCameraForward() * screenDelta.y;

                    moveDirection.y = NO_VERTICAL_VELOCITY;

                    targetPosition += moveDirection.normalized;
                }
            }
        }

        /// <summary>
        /// Drags the camera based on mouse input.
        ///
        /// The drag operation consists of three main steps:
        /// 1. Start Drag: When the left mouse button is initially pressed,
        ///   the world point under the mouse cursor is recorded as the starting point of the drag.
        /// 2. Continuous Drag: While the left mouse button is held down, the current world point under the mouse cursor is calculated.
        ///   The displacement vector from the starting point to the current point is computed.
        ///   This displacement is then applied to the camera's position, moving the camera in the opposite direction of the mouse movement.
        /// 3. End Drag: When the left mouse button is released, the drag operation ends, and the starting point is reset.
        /// </summary>
        private void DragCamera()
        {
            Ray ray;
            Plane plane;

            ray = playerCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );
            plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint;

                hitPoint = ray.GetPoint(distance);

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    startDrag = hitPoint;
                    isDragging = false; // Reset on new press
                }
                else if (Mouse.current.leftButton.isPressed)
                {
                    Vector3 dragDisplacement = startDrag - hitPoint;

                    // Check if movement exceeds threshold to consider it a drag
                    if (dragDisplacement.magnitude > DRAG_THRESHOLD)
                    {
                        isDragging = true;
                    }

                    transform.position += dragDisplacement;
                }
                else if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    startDrag = Vector3.zero;
                    isDragging = false; // Reset when released
                }
            }
        }

        /// <summary>
        /// Checks the terrain height below the camera and adjusts its position if necessary.
        /// </summary>
        private void CheckTerrainHeight()
        {
            Vector3 raycastOrigin;
            Vector3 raycastDirection;
            float terrainHeight;
            int layerMask;

            raycastOrigin =
                transform.position + Vector3.up * TERRAIN_RAYCAST_HEIGHT_OFFSET;
            raycastDirection = Vector3.down;
            // layerMask = ~LayerMask.GetMask("Ignore Raycast");
            layerMask = 1 << 10;

            // debug ray
            // Debug.DrawRay(raycastOrigin, raycastDirection * TERRAIN_RAYCAST_DISTANCE, Color.red);

            if (
                Physics.Raycast(
                    raycastOrigin,
                    raycastDirection,
                    out RaycastHit hit,
                    TERRAIN_RAYCAST_DISTANCE,
                    layerMask
                )
            )
            {
                terrainHeight = hit.point.y;
                targetMinHeight = terrainHeight + MIN_HEIGHT;
                // Debug.Log("We are hitting");
                // Debug.DrawLine(raycastOrigin, hit.point, Color.green);
            }
            else
            {
                // Debug.Log("We are folding");
                targetMinHeight = MIN_HEIGHT;
            }

            dynamicMinHeight = Mathf.Lerp(
                dynamicMinHeight,
                targetMinHeight,
                TERRAIN_HEIGHT_SMOOTH_SPEED * Time.deltaTime
            );
        }
    }
}
