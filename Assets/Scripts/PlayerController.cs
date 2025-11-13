using TTT.GameEvents;
using TTT.Hex;
// using Unity.Netcode;
using UnityEngine;

// [RequireComponent(typeof(Camera))]

/// <summary>
/// Controls the camera for a local player in a multiplayer game.
/// This script manages camera activation and provides movement
/// and rotation controls using WASD, QE, and the right mouse button.
/// It works by ensuring that only the owner of the networked
/// player object has an active camera, preventing conflicts.
/// </summary>
public class PlayerController : MonoBehaviour
{
    const int LeftMouseIndex = 0;

    [SerializeField] public Camera playerCamera;

    [SerializeField]
    private GameEvent _mapMeshClicked;

    //* CB: Controls should be established within Unity and we should be listening to named key events so we're controller-agnostic.
    //*  We should look into the Unity Input System Package
    public float DesiredCellHeight = 1.0f;

    /// <summary>
    /// Called once per frame to handle real-time input and camera controls.
    /// It checks for local ownership before processing movement and rotation
    /// input from the keyboard (WASD, QE) and mouse.
    /// </summary>
    void Update()
    {
        // Left click
        if (Input.GetMouseButtonDown(LeftMouseIndex))
        {
            Ray mousePositionRay = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (
                Physics.Raycast(
                    mousePositionRay,
                    out RaycastHit raycastHit,
                    Mathf.Infinity,
                    HexMesh.LayerMask
                )
            )
            {
                // Raise some event will deal with this later
                Debug.DrawLine(transform.position, raycastHit.point, Color.red);

                _mapMeshClicked.Raise(
                    new MapMeshClickedEventArgs
                    {
                        ClickedPoint = raycastHit.point,
                        PlayerColor = Color.blue
                    }
                );
            }
        }
    }
}
