using System;
using System.Collections;
using System.Collections.Generic;
using TTT.DataClasses.States;
using TMPro;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using TTT.Hex;
using TTT.Managers;
using TTT.ModularData;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
    const float CLICK_THRESHOLD = 5f; // Max pixel movement to still be considered a click

    [SerializeField]
    private Camera playerCamera;

    [SerializeField]
    private CameraController cameraController;

    [SerializeField]
    private GameEvent _mapMeshClicked;

    [SerializeField]
    private GameEvent inspectModeEvent;

    [SerializeField]
    private FeatureRuntimeSet playerBuildings;

    [SerializeField]
    private TextMeshProUGUI statusText;

    [SerializeField]
    private int maxC02 = 500;

    [SerializeField]
    private int maxTemperature = 50;

    [SerializeField]
    private GameEvent _PlayerLoseEvent;

    void Start()
    {
        GameManager = FindAnyObjectByType<GameManager>();
    }

    private GameManager GameManager;

    //* CB: Controls should be established within Unity and we should be listening to named key events so we're controller-agnostic.
    //*  We should look into the Unity Input System Package
    readonly Vector3 startingPosition = new(0, 10, -10);
    public NetworkVariable<Color> PlayerColor = new(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public float DesiredCellHeight = 1.0f;

    private Vector3 mouseDownPosition;

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
        // Track mouse down position
        if (Input.GetMouseButtonDown(LeftMouseIndex))
        {
            mouseDownPosition = Input.mousePosition;
        }

        // Only process tile selection on mouse up, and only if it wasn't a drag
        if (Input.GetMouseButtonUp(LeftMouseIndex))
        {
            // Don't process world clicks when clicking on UI
            if (IsMouseOverUI())
            {
                return;
            }

            // Check if mouse moved significantly (drag) vs stayed in place (click)
            float mouseMovement = Vector3.Distance(
                mouseDownPosition,
                Input.mousePosition
            );
            if (mouseMovement > CLICK_THRESHOLD)
            {
                // This was a drag, not a click - don't select tile
                return;
            }

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

    /// <summary>
    /// Check if the pointer is over a UI element.
    /// </summary>
    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void CheckIfPlayerHasLost()
    {
        bool playerLost = false;

        if (
            GameManager.GetCO2() > maxC02
            || GameManager.GetTemperature() > maxTemperature
            || playerBuildings.Count() <= 0
        )
        {
            playerLost = true;
        }

        if (playerLost)
        {
            _PlayerLoseEvent.Raise();
            OnLose();
            return;
        }
    }

    public void OnLose()
    {
        DisableUI();

        statusText.gameObject.SetActive(true);
        statusText.text = "Spectating";

        // feel free to remove this if needed, not important
        GameObject cube = GameObject.Find("Cube");
        cube.SetActive(false);
    }

    public void DisableUI()
    {
        inspectModeEvent.Raise();

        GameObject uiCanvas = GameObject.Find("GameUI");
        if (uiCanvas != null)
        {
            // disabling the parent would prevent the status text from appearing
            // so we enable all children individually instead
            foreach (Transform child in uiCanvas.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
