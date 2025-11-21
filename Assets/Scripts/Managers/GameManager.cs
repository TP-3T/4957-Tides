using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codice.CM.Common.Tree;
using TTT.DataClasses.States;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using TTT.GameEvents.Assets.Scripts.GameEvents.Args;
using TTT.Helpers;
using Unity.Netcode;
using UnityEngine;

namespace TTT.Managers
{
    public class GameManager : GenericNetworkSingleton<GameManager>
    {
        [SerializeField]
        private GameEvent newMapEvent;

        private readonly string[] Seasons =
        {
            "Spring",
            "Summer",
            "Fall",
            "Winter",
        };

        //serialize for now
        [field: SerializeField]
        public int Year { get; private set; } = 1;

        [SerializeField]
        private string Season;

        [SerializeField]
        private int CO2;

        [SerializeField]
        private GameEvent SeasonChanging;

        [SerializeField]
        private GameEvent _OnYearChangeEvent;

        [SerializeField]
        private GameEvent _FloodEvent;
        [SerializeField]
        private GameEvent _currentClient;
        [SerializeField]
        private GameEvent _connectedClients;

        [SerializeField]
        private TTT.DataClasses.States.InteractionMode interactionMode;

        [SerializeField]
        private FeatureType buildingFeatureType;

        [SerializeField]
        private GameEvent BuildingFeatureEvent;


        private List<ulong> _allPlayers = new List<ulong>();
        private ulong _currentPlauer;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.Season = Seasons[0];
            this.CO2 = 0;
            // NetworkManager.Singleton.OnServerStarted += ServerStartHandler;
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback +=
                OnClientConnect;
        }

        public void OnStartNetworkEvent(UnityEngine.Object eventArgs)
        {
            StartNetworkEventArgs args = eventArgs as StartNetworkEventArgs;
            // try
            // {
                if (args.IsHost)
                {
                    StartGameHost();
                }
                else
                {
                    StartGameClient();
                }
            // }
            // catch (System.Exception e)
            // {
            //     Debug.LogError($"Failed to start host: {e.Message}");
            //     return;
            // }
        }

        private void StartGameClient()
        {
            NetworkManager.Singleton.StartClient();
        }

        private void StartGameHost()
        {
            NetworkManager.Singleton.StartHost();

            if (LoadExternalJson.TryGetDataJson(out TextAsset newMap))
            {
                newMapEvent.Raise(new NewMapEventArgs() { DataFile = newMap });
            }
            else
            {
                throw new IOException("Could not load file.");
            }
        }

        public void OnNewMapFinish(UnityEngine.Object eventArgs)
        {
            NewMapFinishedEventArgs args = eventArgs as NewMapFinishedEventArgs;

            if (!args.WasSuccessful)
            {
                Debug.LogWarning(
                    "MAP FAILED TO LOAD! WE SHOULD REVERT TO THE MAIN MENU FROM HERE!"
                );
            }
            else
            {
                // Debug.Log("Wow, map was loaded!");
            }
        }

        [ClientRpc]
        public void RegisterClientClientRpc(ulong[] clients)
        {
            Debug.Log($"there are {clients.Count()} in the game rn");

            _connectedClients.Raise(new ConnectedCilentsEventArgs()
            {
                clientIds = clients
            });
        }

        public void OnClientConnect(ulong clientId)
        {
            Debug.Log($"new client connected {clientId}");
            _allPlayers.Add(clientId);
            ulong[] clientIds = _allPlayers.ToArray();
            RegisterClientClientRpc(clientIds);
        }

        /// <summary>
        /// Increments the season, and the year if applicable.
        /// </summary>
        public void IncrementSeason()
        {
            int currentSeasonIndex = System.Array.IndexOf(Seasons, Season);

            // % to wrap around to the beginning after winter
            int nextSeasonIndex =
                (currentSeasonIndex + 1) % this.Seasons.Length;

            this.Season = this.Seasons[nextSeasonIndex];

            if (this.Season == this.Seasons[0])
            {
                this.IncrementYear();
                _OnYearChangeEvent.Raise();
            }
        }

        /// <summary>
        /// Increments the year by one.
        /// </summary>
        public void IncrementYear()
        {
            this.Year += 1;
        }

        public void OnLastPlayerTurnEvent(UnityEngine.Object eventArgs)
        {
            SeasonChanging.Raise();
            this.IncrementSeason();
        }

        public void OnYearChange(UnityEngine.Object eventArgs)
        {
            Debug.Log(
                "Year has changed, this should go in a AI manager or just query the AI here  - GameManager line 124"
            );

            _FloodEvent.Raise();
        }

        public int GetYear()
        {
            return this.Year;
        }

        public string GetSeason()
        {
            return this.Season;
        }

        public int GetCO2()
        {
            return this.CO2;
        }

        /// <summary>
        /// Starts the build mode event, disabling certain features.
        /// </summary>
        /// <param name="eventArgs"></param>
        public void StartBuildMode(UnityEngine.Object eventArgs)
        {
            if (eventArgs is not FeatureType featureType)
            {
                return;
            }

            interactionMode = DataClasses.States.InteractionMode.BUILDING;
            buildingFeatureType = featureType;
        }

        /// <summary>
        /// Starts the Inspect mode, disabling building.
        /// </summary>
        /// <param name="_"></param>
        public void StartInspectMode(UnityEngine.Object _)
        {
            interactionMode = DataClasses.States.InteractionMode.INSPECTING;
        }

        /// <summary>
        /// TODO
        /// - This will be a client RPC which will show the building that XYZ has placed.
        /// </summary>
        /// <param name="eventArgs"></param>
        public void MeshClicked(UnityEngine.Object eventArgs)
        {
            if (eventArgs is not MapMeshClickedEventArgs clickedArgs)
            {
                return;
            }

            if (interactionMode == DataClasses.States.InteractionMode.BUILDING)
            {
                // raise build event
                var args =
                    ScriptableObject.CreateInstance<BuildingFeatureArgs>();
                args.Location = clickedArgs.ClickedPoint;
                args.FeatureType = buildingFeatureType;

                if (buildingFeatureType == null)
                {
                    return;
                }

                BuildingFeatureEvent.Raise(args);
            }
        }

        /// <summary>
        /// Handles mesh click logic for BuildMode to raise build event.
        /// </summary>
        /// <param name="eventArgs"></param>
        public void OnMeshClicked(UnityEngine.Object eventArgs)
        {
            if (eventArgs is not MapMeshClickedEventArgs clickedArgs)
            {
                return;
            }

            if (interactionMode == DataClasses.States.InteractionMode.BUILDING)
            {
                MeshClicked(eventArgs);
            }
        }
    }
}
