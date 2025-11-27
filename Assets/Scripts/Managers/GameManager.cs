using System.Collections.Generic;
using System.IO;
using System.Linq;
using TTT.DataClasses.PlayerResources;
using TTT.DataClasses.States;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using TTT.Helpers;
using Unity.Netcode;
using UnityEngine;

namespace TTT.Managers
{
    public class GameManager : GenericNetworkSingleton<GameManager>
    {
        [field: SerializeField]
        public List<PlayerResource> PlayerResources { get; set; }

        [SerializeField]
        private InteractionMode interactionMode;

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
        private int Temperature;

        [SerializeField]
        private GameEvent endTurnEvent;

        [SerializeField]
        private GameEvent endingSeasonEvent;

        [SerializeField]
        private GameEvent endingYearEvent;

        [SerializeField]
        private GameEvent _FloodEvent;

        [SerializeField]
        private GameEvent BuildingFeatureEvent;

        [SerializeField]
        private FeatureType buildingFeatureType;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.Season = Seasons[0];
            this.CO2 = 0;
            // NetworkManager.Singleton.OnServerStarted += ServerStartHandler;
        }

        public void OnStartNetworkEvent(UnityEngine.Object eventArgs)
        {
            StartNetworkEventArgs args = eventArgs as StartNetworkEventArgs;
            try
            {
                if (args.IsHost)
                {
                    StartGameHost();
                }
                else
                {
                    StartGameClient();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start host: {e.Message}");
                return;
            }
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
                Debug.Log("Wow, map was loaded!");
            }
        }

        public void OnTurnEnding(Object _)
        {
            // Placeholder since we don't know who actually is the last player
            static bool IsLastPlayer() => true;

            if (!IsLastPlayer())
            {
                endTurnEvent.Raise();
            }
            else
            {
                // end the season before saying the turn ended
                endingSeasonEvent.Raise();
                IncrementSeason();
            }
        }

        /// <summary>
        /// Increments the season, and the year if applicable.
        /// </summary>
        public void IncrementSeason()
        {
            int currentSeasonIndex = System.Array.IndexOf(Seasons, Season);

            // % to wrap around to the beginning after winter
            int nextSeasonIndex = (currentSeasonIndex + 1) % Seasons.Length;
            Season = Seasons[nextSeasonIndex];

            if (Season != Seasons[0])
            {
                endTurnEvent.Raise();
            }
            else
            {
                // end the year before saying the turn ended
                endingYearEvent.Raise();
                IncrementYear();
            }
        }

        public void IncrementYear()
        {
            Year += 1;

            Debug.Log(
                "Year has changed, this should go in a AI manager or just query the AI here  - GameManager line 124"
            );

            // do flooding before saying the turn ended
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
        /// Gets the current temperature.
        /// </summary>
        public int GetTemperature()
        {
            return this.Temperature;
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

            interactionMode = InteractionMode.BUILDING;
            buildingFeatureType = featureType;
        }

        /// <summary>
        /// Starts the Inspect mode, disabling building.
        /// </summary>
        /// <param name="_"></param>
        public void StartInspectMode(UnityEngine.Object _)
        {
            interactionMode = InteractionMode.INSPECTING;
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

            if (interactionMode == InteractionMode.BUILDING)
            {
                // raise build event
                var args =
                    ScriptableObject.CreateInstance<BuildingFeatureArgs>();
                args.Location = clickedArgs.ClickedPoint;
                args.FeatureType = buildingFeatureType;
                args.OwnedByClient = true;

                if (buildingFeatureType == null)
                {
                    return;
                }

                BuildingFeatureEvent.Raise(args);
            }
        }

        public void OnExitBuildMode(Object _)
        {
            Debug.Log("Exiting build mode.");
            interactionMode = InteractionMode.INSPECTING;
            buildingFeatureType = null;

        }

        public void OnPlayerLose(Object _)
        {
            Debug.Log("Player has lost the game.");
        }

        public bool CanEndTurn()
        {
            bool hasEnoughResources = PlayerResources.All(resources =>
                resources.AmountOwned >= 0
            );

            return hasEnoughResources;
        }
    }
}
