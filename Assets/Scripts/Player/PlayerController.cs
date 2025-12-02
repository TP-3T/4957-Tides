using TMPro;
using TTT.DataClasses.States;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using TTT.Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the camera for a local player in a multiplayer game.
/// This script manages camera activation and provides movement
/// and rotation controls using WASD, QE, and the right mouse button.
/// It works by ensuring that only the owner of the networked
/// player object has an active camera, preventing conflicts.
/// </summary>
namespace TTT.Player
{
    public class PlayerController : NetworkBehaviour
    {
        const int LeftMouseIndex = 0;
        const float CLICK_THRESHOLD = 50f; // Max pixel movement to still be considered a click

        [SerializeField]
        private Camera playerCamera;

        [SerializeField]
        private CameraController cameraController;

        [SerializeField]
        private FeatureRuntimeSet playerBuildings;

        [SerializeField]
        private TextMeshProUGUI statusText;

        [SerializeField]
        private int maxCO2 = 500;

        [SerializeField]
        private int maxTemperature = 50;

        public FeatureType FeatureType;

        [SerializeField]
        private GameEvent InteractModeChange;

        [SerializeField]
        private GameEvent _mapMeshClicked;

        [SerializeField]
        private GameEvent BuildingFeatureEvent;

        // [SerializeField]
        // private Canvas currentUI;
        public InteractionMode Mode;

        [SerializeField]
        public PlayerStats PlayerStats;

        [SerializeField]
        private GameObject GameUI;

        [SerializeField]
        private GameObject MainMenu;

        [SerializeField]
        private GameObject LoseUI;

        [SerializeField]
        private GameObject CurrentUI;

        public GameEvent playerLoseEvent;

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
                if (playerCamera != null)
                {
                    playerCamera.enabled = true;
                    Debug.Log("Enable camera for local player");
                }
                else
                {
                    Debug.LogError(
                        $"PlayerController on {gameObject.name} playerCamera is not assigned"
                    );
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
            Debug.Log(
                $"Assigned color {PlayerColor.Value} to Player {clientId}"
            );
        }

        //? po: is this ever used or called anywhere??
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

        private void Start()
        {
            CurrentUI = Instantiate(MainMenu);
            Mode = InteractionMode.INSPECTING;
        }

        private void SetCurrentUI(GameObject newUI)
        {
            if (CurrentUI != null)
            {
                Extensions.SmartDestroy(CurrentUI);
            }
            CurrentUI = Instantiate(newUI);
            CurrentUI.transform.parent = this.transform;
        }

        //? CB: There must be an event driven way to handle this.
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
                    Debug.Log("Mouse over UI, not processing world click");
                    return;
                }

                // Check if mouse moved significantly (drag) vs stayed in place (click)
                float mouseMovement = Vector3.Distance(
                    mouseDownPosition,
                    Input.mousePosition
                );
                if (mouseMovement > CLICK_THRESHOLD)
                {
                    Debug.Log(mouseMovement);
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
                    if (Mode.Equals(InteractionMode.BUILDING))
                    {
                        var building =
                            ScriptableObject.CreateInstance<BuildingFeatureArgs>();
                        building.Location = raycastHit.point;
                        building.FeatureType = FeatureType;
                        building.OwnedByClient = true;
                        BuildingFeatureEvent.Raise(building);
                    }
                    else if (Mode.Equals(InteractionMode.INSPECTING))
                    {
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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Mode = InteractionMode.INSPECTING;
            }
        }

        public void OnInteractModeChange(object args)
        // po: this listens to an event raised by OnClick() in BuildingShopSlot
        {
            if (args != null)
            {
                var newMode = args as InteractionModeChangeEventArgs;
                Mode = newMode.NewMode;
            }
            else
            {
                Debug.Log(
                    "args passed into OnInteractModeChange are not the expected type"
                );
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
            if (
                GameManager.Instance.CO2_Pollution.Value > maxCO2
                || GameManager.Instance.Temperature.Value > maxTemperature
                || playerBuildings.GetItems().Length <= 0
            )
            {
                playerLoseEvent.Raise();
                OnLose();
            }
        }

        public void OnLose()
        {
            SetCurrentUI(LoseUI);
        }
    }
}
