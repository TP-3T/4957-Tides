using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TTT.DataClasses.HexData;
using TTT.DataClasses.PlayerResources;
using TTT.DataClasses.States;
using TTT.GameEvents;
using TTT.Helpers;
using Unity.Netcode;
using UnityEngine;

namespace TTT.Managers
{
    public class GameManager : GenericNetworkSingleton<GameManager>
    {
        private Queue<WorldState> AIDataQueue = new();

        [field: SerializeField]
        public List<PlayerResource> PlayerResources { get; private set; }

        [field: SerializeField]
        public PlayerStats PlayerStats { get; private set; }

        [field: SerializeField]
        public InteractionMode InteractionMode { get; private set; }

        private readonly string[] Seasons =
        {
            "Spring",
            "Summer",
            "Fall",
            "Winter",
        };

        [field: SerializeField]
        public NetworkClient CurrentPlayer { get; private set; }

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
        private GameEvent startTurnEvent;

        [SerializeField]
        private GameEvent endTurnEvent;

        [SerializeField]
        private GameEvent endingSeasonEvent;

        [SerializeField]
        private GameEvent endingYearEvent;

        [SerializeField]
        private GameEvent newMapEvent;

        [SerializeField]
        private GameEvent BuildingFeatureEvent;

        public override void Awake()
        {
            base.Awake();
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 313.18f,
                    SeaLevel = 14.18079369f,
                    Temp = -22.64326f,
                    Year = 1940,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 313.34f,
                    SeaLevel = 14.35222333f,
                    Temp = -12.24326f,
                    Year = 1941,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 313.34f,
                    SeaLevel = 14.35222333f,
                    Temp = -12.24326f,
                    Year = 1942,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 313.84f,
                    SeaLevel = 14.35732167f,
                    Temp = -16.94326f,
                    Year = 1943,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 313.88f,
                    SeaLevel = 14.18043223f,
                    Temp = -5.54326f,
                    Year = 1944,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 314.63f,
                    SeaLevel = 14.60734743f,
                    Temp = -5.84326f,
                    Year = 1945,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 314.63f,
                    SeaLevel = 14.25638287f,
                    Temp = -16.84326f,
                    Year = 1946,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 314.66f,
                    SeaLevel = 14.4138078f,
                    Temp = -10.64326f,
                    Year = 1947,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 314.88f,
                    SeaLevel = 14.48694413f,
                    Temp = -8.94326f,
                    Year = 1948,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 315.95f,
                    SeaLevel = 14.17366973f,
                    Temp = -1.94326f,
                    Year = 1949,
                }
            );
            AIDataQueue.Enqueue(
                new()
                {
                    Pollution = 315.67f,
                    SeaLevel = 14.36671727f,
                    Temp = 1.35674f,
                    Year = 1950,
                }
            );
        }

        // Start
        //  is called once
        //  before the first
        //  execution of
        //  Update after the MonoBehaviour is created
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
        }

        public void OnTurnEnding(Object _)
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
                StartNextTurn(new());
            }
            else
            {
                CurrentPlayer = ConnectedClientsList.First();
                // end the season before saying the turn ended
                EndSeason();
                if (Season.Equals(Seasons[0]))
                {
                    EndYear();
                }
                else
                {
                    StartNextTurn(new());
                }
            }
        }

        /// <summary>
        /// Increments the season, and the year if applicable.
        /// </summary>
        private void EndSeason()
        {
            endingSeasonEvent.Raise();
            int currentSeasonIndex = System.Array.IndexOf(Seasons, Season);

            // % to wrap around to the beginning after winter
            int nextSeasonIndex = (currentSeasonIndex + 1) % Seasons.Length;
            Season = Seasons[nextSeasonIndex];
        }

        private void EndYear()
        {
            Debug.Log(
                "Year has changed, this should go in a AI manager or just query the AI here  - GameManager line 124"
            );
            Year += 1;
            
            // Calculate and apply sea level change based on pollution
            if (PlayerStats != null)
            {
                float seaLevelIncrease = PlayerStats.CalculateSeaLevelFromPollution();
                MapManager.Instance.SeaLevel.Value += seaLevelIncrease;
                Debug.Log($"Sea level increased by {seaLevelIncrease} due to pollution");
            }
            
            endingYearEvent.Raise();
        }

        public void StartNextTurn(object _)
        {
            endTurnEvent.Raise();
            startTurnEvent.Raise();
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
