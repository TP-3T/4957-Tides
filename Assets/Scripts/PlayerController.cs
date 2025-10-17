using TTT.Hex;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls the camera for a local player in a multiplayer game.
/// This script manages camera activation and provides movement
/// and rotation controls using WASD, QE, and the right mouse button.
/// It works by ensuring that only the owner of the networked
/// player object has an active camera, preventing conflicts.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    public UnityEvent<Vector3> OnPlayerClick = new UnityEvent<Vector3>();
    private Camera playerCamera;
    private HexGrid hexGrid;
    const int LeftMouseIndex = 0;
    const float moveSpeed = 50f;
    const int RightMouseIndex = 1;
    const float rotationSpeed = 2f;
    readonly Vector3 startingPosition = new Vector3(0, 10, -10);
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public float DesiredCellHeight = 1.0f;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// It gets a reference to the camera and disables it by default
    /// to ensure it's not active for remote players.
    /// </summary>
    void Awake()
    {
        //Get a reference to the camera component on this object itself
        playerCamera = GetComponent<Camera>();

        //Disable camera by default so it wont activate on other clients.
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
        }
    }

    /// <summary>
    /// Called when the networked object is spawned on the network.
    /// It checks if the object is owned by the local client. If it is,
    /// it enables the camera for that player and disables the default
    /// scene camera to avoid conflicts.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // ⭐ NEW: Server assigns a unique color when the player spawns.
        if (IsServer)
        {
            AssignUniquePlayerColor(OwnerClientId);
        }

        if (IsOwner)
        {
            hexGrid = GameObject.FindFirstObjectByType<HexGrid>();
            if (hexGrid == null)
            {
                Debug.Log("HexGrid not found yet. Subscribing to OnClientConnectedCallback.");
                NetworkManager.Singleton.OnClientConnectedCallback += FindHexGridAfterConnection;
            }
            transform.position = startingPosition;
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
                Debug.Log("Enable camera for local player");
            }
        }
    }

    private void AssignUniquePlayerColor(ulong clientId)
    {
        Color uniqueColor;
        // Simple color assignment logic based on client ID. You can make this more robust.
        switch (clientId % 4) // Cycle through 4 basic colors
        {
            case 0:
                uniqueColor = Color.red;
                break;
            case 1:
                uniqueColor = Color.blue;
                break;
            case 2:
                uniqueColor = Color.green;
                break;
            case 3:
                uniqueColor = Color.yellow;
                break;
            default:
                uniqueColor = Color.white;
                break;
        }
        PlayerColor.Value = uniqueColor;
        Debug.Log($"Assigned color {PlayerColor.Value} to Player {clientId}");
    }

    private void FindHexGridAfterConnection(ulong clientId)
    {
        // The event fires for *all* clients connecting, but we only care about the local player's logic.
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Unsubscribe immediately to prevent running again.
            NetworkManager.Singleton.OnClientConnectedCallback -= FindHexGridAfterConnection;

            // Search the scene again now that the server's spawn message (for the HexGrid)
            // has had time to process.
            hexGrid = GameObject.FindFirstObjectByType<HexGrid>();
        }
    }

    /// <summary>
    /// Called once per frame to handle real-time input and camera controls.
    /// It checks for local ownership before processing movement and rotation
    /// input from the keyboard (WASD, QE) and mouse.
    /// </summary>
    void Update()
    {
        // The camera controls should only run for the local player.
        if (!IsOwner)
        {
            return;
        }

        // WASD Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0;
        right.y = 0;

        Vector3 movement = (forward * verticalInput) + (right * horizontalInput);
        transform.position += movement * moveSpeed * Time.deltaTime;

        // Q and E Vertical Movement
        float verticalMove = 0f;
        if (Input.GetKey(KeyCode.E))
        {
            verticalMove = moveSpeed;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            verticalMove = -moveSpeed;
        }

        transform.position += Vector3.up * verticalMove * Time.deltaTime;

        // Mouse-based Rotation
        if (Input.GetMouseButton(RightMouseIndex)) // Right-click held down
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Rotate based on mouse movement
            transform.Rotate(Vector3.up, mouseX * rotationSpeed, Space.World);
            transform.Rotate(Vector3.right, -mouseY * rotationSpeed, Space.Self);
        }

        // Left click
        if (Input.GetMouseButtonDown(LeftMouseIndex))
        {
            // Debug.Log("Player clicked left mouse button");
            Ray mousePositionRay = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //Ray Cast Logic
            if (Physics.Raycast(mousePositionRay, out hit, Mathf.Infinity, HexGrid.GRID_LAYER_MASK))
            {
                if (hexGrid != null)
                {
                    hexGrid.HandlePlayerClickServerRpc(
                        hit.point,
                        PlayerColor.Value,
                        DesiredCellHeight
                    );
                }
            }
        }
    }
}
