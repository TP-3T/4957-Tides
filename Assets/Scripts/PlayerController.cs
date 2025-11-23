using System;
using System.Collections;
using System.Collections.Generic;
using TTT.DataClasses.States;
using TTT.GameEvents;
using TTT.Hex;
using TTT.Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

// [RequireComponent(typeof(Camera))]

/// <summary>
/// Controls the camera for a local player in a multiplayer game.
/// This script manages camera activation and provides movement
/// and rotation controls using WASD, QE, and the right mouse button.
/// It works by ensuring that only the owner of the networked
/// player object has an active camera, preventing conflicts.
/// </summary>
public class PlayerController : NetworkBehaviour
{
    const string MAIN_MENU_STR = "mainmenu_ui";
    const string LOADING_STR = "loading_ui";
    const string PLAYING_STR = "playing_ui";
    const string PAUSED_STR = "paused_ui";
    const string STATE_STR_ERR = "Element is invalid.";
    const int LeftMouseIndex = 0;

    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private GameEvent _mapMeshClicked;

    //* CB: Controls should be established within Unity and we should be listening to named key events so we're controller-agnostic.
    //*  We should look into the Unity Input System Package
    readonly Vector3 startingPosition = new(0, 10, -10);
    public NetworkVariable<Color> PlayerColor = new(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public float DesiredCellHeight = 1.0f;
    private Dictionary<SystemState, GameObject> UICanvases = new();

    // MainMenu Canvas => UI Group
    //      - MainMenuButton Component
    //      - Header Component

    /// <summary>
    /// Get desired state from element name
    /// switch statement to check string name of variable, if contains ie "main menu" delete current canvas, create main menu canvas, assign state to main menu with main menu canvas. This will happen ANYTIME STATE IS CHANGED.
    /// </summary>
    IEnumerator Start()
    {
        yield return AssetLoader<GameObject>.LoadGroup(
            "ui",
            (element) =>
            {
                String elementName = element.name;

                if (elementName.Contains(MAIN_MENU_STR))
                {
                    UICanvases.Clear();
                    UICanvases.Add(SystemState.MAIN_MENU, element);
                }
                else if (elementName.Contains(LOADING_STR))
                {
                    UICanvases.Clear();
                    UICanvases.Add(SystemState.LOADING, element);
                }
                else if (elementName.Contains(PAUSED_STR))
                {
                    UICanvases.Clear();
                    UICanvases.Add(SystemState.PAUSED, element);
                }
                else if (elementName.Contains(PLAYING_STR))
                {
                    UICanvases.Clear();
                    UICanvases.Add(SystemState.PLAYING, element);
                }
                else
                {
                    Debug.Log(STATE_STR_ERR);
                }
            }
        );
    }

    /// <summary>
    /// Called when the networked object is spawned on the network.
    /// It checks if the object is owned by the local client. If it is,
    /// it enables the camera for that player and disables the default
    /// scene camera to avoid conflicts.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // NEW: Server assigns a unique color when the player spawns.
        if (IsServer)
        {
            AssignUniquePlayerColor(OwnerClientId);
        }

        if (IsOwner)
        {
            transform.position = startingPosition;
            //! CB: We don't handle the not-null case. This causes silent errors.
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
                Debug.Log("Enable camera for local player");
            }
        }
    }

    private void AssignUniquePlayerColor(ulong clientId)
    {
        // TODO: Let players pick 4 colors they want to see.
        Color uniqueColor = (clientId % 4) switch
        {
            // Cycle through 4 basic colors
            0 => Color.red,
            1 => Color.blue,
            2 => Color.green,
            3 => Color.yellow,
            _ => Color.white,
        };
        PlayerColor.Value = uniqueColor;
        Debug.Log($"Assigned color {PlayerColor.Value} to Player {clientId}");
    }

    private void FindHexGridAfterConnection(ulong clientId)
    {
        // The event fires for *all* clients connecting, but we only care about the local player's logic.
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Unsubscribe immediately to prevent running again.
            NetworkManager.Singleton.OnClientConnectedCallback -=
                FindHexGridAfterConnection;
        }
    }

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
            Ray mousePositionRay = playerCamera.ScreenPointToRay(
                Input.mousePosition
            );
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
                // Debug.DrawLine(transform.position, raycastHit.point, Color.red);

                _mapMeshClicked.Raise(
                    new MapMeshClickedEventArgs
                    {
                        ClickedPoint = raycastHit.point,
                        PlayerColor = PlayerColor.Value,
                        PlayerId = OwnerClientId,
                    }
                );
            }
        }
    }
}
