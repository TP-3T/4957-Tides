using System.Collections;
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
        public List<PlayerResource> PlayerResources { get; private set; }

        [field: SerializeField]
        public InteractionMode InteractionMode { get; private set; }

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

        [field: SerializeField]
        public string Season { get; private set; }

        [field: SerializeField]
        public int CO2 { get; private set; } = 0;

        [field: SerializeField]
        public int Temperature { get; private set; }

        [SerializeField]
        private GameEvent endTurnEvent;

        [SerializeField]
        private GameEvent startTurnEvent;

        [SerializeField]
        private GameEvent endingSeasonEvent;

        [SerializeField]
        private GameEvent endingYearEvent;

        [SerializeField]
        private GameEvent BuildingFeatureEvent;

        private FeatureType buildingFeatureType;
        public NetworkClient CurrentPlayer { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Season = Seasons[0];
            CO2 = 0;
            // NetworkManager.Singleton.OnServerStarted += ServerStartHandler;
        }

        public void OnStartNetworkEvent(Object eventArgs)
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

        public void OnNewMapFinish(Object eventArgs)
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

        public void OnTurnEnd(object _)
        {
            startTurnEvent.Raise();
        }

        public void OnTurnEnding(Object _)
        {
            StartCoroutine(ProcessEndTurn());
        }

        private IEnumerator ProcessEndTurn()
        {
            //Get all the connected clients
            var ConnectedClientsList =
                NetworkManager.Singleton.ConnectedClientsList.ToList();
            var self = NetworkManager.Singleton.LocalClient;

            //If I am not the last connected client
            if (!ConnectedClientsList.Last().Equals(self))
            {
                //Increment the current client
                var currentIndex = ConnectedClientsList.IndexOf(CurrentPlayer);
                CurrentPlayer = ConnectedClientsList[currentIndex + 1];
            }
            else
            {
                CurrentPlayer = ConnectedClientsList.First();
                // end the season before saying the turn ended
                EndSeason();
                if (Season.Equals(Seasons[0]))
                {
                    var endYear = EndYear();
                    while (endYear.MoveNext())
                    {
                        yield return null;
                    }
                }
            }
            endTurnEvent.Raise();
            startTurnEvent.Raise();
        }

        /// <summary>
        /// Increments the season, and the year if applicable.
        /// </summary>
        public void EndSeason()
        {
            endingSeasonEvent.Raise();
            int currentSeasonIndex = System.Array.IndexOf(Seasons, Season);

            // % to wrap around to the beginning after winter
            int nextSeasonIndex = (currentSeasonIndex + 1) % Seasons.Length;
            Season = Seasons[nextSeasonIndex];
        }

        public IEnumerator EndYear()
        {
            Debug.Log(
                "Year has changed, this should go in a AI manager or just query the AI here  - GameManager line 124"
            );
            Year += 1;
            endingYearEvent.Raise();
            var seaRaise = MapManager.Instance.RaiseSea();
            while (seaRaise.MoveNext())
            {
                yield return null;
            }
        }

        /// <summary>
        /// Starts the Inspect mode, disabling building.
        /// </summary>
        /// <param name="_"></param>
        public void OnInteractModeChange(Object args)
        {
            if (args is not InteractionModeChangeEventArgs newMode)
            {
                Debug.LogError(
                    "Game Manager received invalid interactModeChange args!"
                );
            }
            else
            {
                InteractionMode = newMode.NewMode;
            }
        }

        /// <summary>
        /// Handles mesh click logic for BuildMode to raise build event.
        /// </summary>
        /// <param name="eventArgs"></param>
        public void OnMeshClicked(Object eventArgs)
        {
            if (eventArgs is not MapMeshClickedEventArgs clickedArgs)
            {
                return;
            }

            if (InteractionMode == InteractionMode.BUILDING)
            {
                if (buildingFeatureType == null)
                {
                    Debug.LogError(
                        "Tried to build, but there was no selected building!"
                    );
                }

                // raise build event
                var args =
                    ScriptableObject.CreateInstance<BuildingFeatureArgs>();
                args.Location = clickedArgs.ClickedPoint;
                args.FeatureType = buildingFeatureType;

                BuildingFeatureEvent.Raise(args);
            }
        }

        public void OnBuild(object args)
        {
            if (args is not BuildingFeatureEventArgs BuildArgs)
            {
                Debug.LogError(
                    "Game Manager received invalid interactModeChange args!"
                );
            }
            else if (!InteractionMode.Equals(InteractionMode.BUILDING))
            {
                Debug.LogError(
                    "Game Manager received invalid interactModeChange args!"
                );
            }
            else
            {
                var building =
                    ScriptableObject.CreateInstance<BuildingFeatureArgs>();
                building.Location = BuildArgs.Location;
                building.FeatureType = buildingFeatureType;
            }
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
