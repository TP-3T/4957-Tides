using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TTT.ClimateModel;
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
        public int Year { get; set; } = 1;

        [field: SerializeField]
        public string Season { get; private set; }

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

        // Initial climate values
        public static readonly float INITIAL_SEA_LEVEL_M = 0.0f;
        public static readonly float INITIAL_CO2_PPM = 309.41f;
        public static readonly float INITIAL_TEMPERATURE_DEG_C = 14.15561478f;

        [field: SerializeField]
        public NetworkVariable<float> SeaLevel { get; private set; } =
            new(INITIAL_TEMPERATURE_DEG_C);

        [field: SerializeField]
        public NetworkVariable<float> CO2_Pollution { get; private set; } =
            new(INITIAL_CO2_PPM);

        [field: SerializeField]
        public NetworkVariable<float> Temperature { get; private set; } =
            new(INITIAL_SEA_LEVEL_M);

        public override void Awake()
        {
            base.Awake();
            // Temperature.Value = INITIAL_TEMPERATURE_DEG_C;
            // CO2_Pollution.Value = INITIAL_CO2_PPM;
            // SeaLevel.Value = INITIAL_SEA_LEVEL_M;
        }

        // Start
        //  is called once
        //  before the first
        //  execution of
        //  Update after the MonoBehaviour is created
        void Start()
        {
            Season = Seasons[0];
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

            if (args.WasSuccessful)
            {
                // Seed the historical climate data into the climate model's internal queue
                ClimatePredictionModel.ResetWorldQueue();
            }
            else
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
            Year += 1;

            // --- Calculate future climate values ---

            WorldState currentWorldState = new()
            {
                Pollution = CO2_Pollution.Value,
                SeaLevel = SeaLevel.Value,
                Temp = Temperature.Value,
                Year = Year,
            };

            WorldState futureWorldState =
                ClimatePredictionModel.PredictFutureClimateDataForNextTurn(
                    currentWorldState
                );

            Temperature.Value = futureWorldState.Temp;
            SeaLevel.Value = futureWorldState.SeaLevel;
            // (CO2 not updated by climate prediction model)

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
