using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TTT.ClimateModel;
using TTT.DataClasses.ClimateModel;
using TTT.DataClasses.HexData;
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

        private readonly string[] Seasons =
        {
            "Spring",
            "Summer",
            "Fall",
            "Winter",
        };

        private readonly ClimatePredictionModel _climatePredictionModel = new();

        private readonly Queue<WorldState> _climateModelWorldStatesQueue =
            new();

        [field: SerializeField]
        public NetworkClient CurrentPlayer { get; private set; }

        //serialize for now
        [field: SerializeField]
        public int Year { get; private set; } = 1;

        [field: SerializeField]
        public string Season { get; private set; }

        [field: SerializeField]
        public float CO2 { get; private set; } = 0; //TODO: find out where this is being updated and when

        [field: SerializeField]
        public float Temperature { get; set; }

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

        public ClimateModelOutputDTO UpdateClimateDataForNextTurn()
        {
            // If queue is empty (on game start)
            if (_climateModelWorldStatesQueue.Count == 0)
            {
                // Populate queue with historical data (worldstate data before the start of the game the model can use to predict)
                PopulateClimateModelWorldStatesQueue();
            }

            // get current turn values
            ClimateModelInputDTO inputDTO = new()
            {
                currAtmosphericCO2ConcentrationPpm = CO2,
                currSeaLevelMetres = MapManager.Instance.SeaLevel.Value,
                currTemperatureCelsius = Temperature,
            };

            // Get new values from climate prediction model
            ClimateModelOutputDTO outputDTO =
                _climatePredictionModel.PredictFutureTempAndSeaLevel(
                    inputDTO,
                    _climateModelWorldStatesQueue
                );

            UpdateWorldStatesQueue(outputDTO);

            return outputDTO;
        }

        private void UpdateWorldStatesQueue(ClimateModelOutputDTO outputDTO)
        {
            // Dequeue WorldState from 10 years ago
            _climateModelWorldStatesQueue.Dequeue();

            // Create a WorldState object using the predicted next year values (values for winter -> spring)
            WorldState worldStateNextYear = new()
            {
                Pollution = 0.0f, //TODO - ask Corey when these are calculated (before or after this method is called)
                SeaLevel = (float)outputDTO.futureSeaLevelMetres,
                Temp = (float)outputDTO.futureTemperatureCelsius,
                Year = 0, //TODO - ask Corey when these are calculated (before or after this method is called)
            };

            // Enqueue WorldState for the next year
            _climateModelWorldStatesQueue.Enqueue(worldStateNextYear);
        }

        //TODO: move this to the function that creates the map and game
        // delete this method once the above todo completed
        private void PopulateClimateModelWorldStatesQueue()
        {
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.18f,
                    SeaLevel = 14.18079369f,
                    Temp = -22.64326f,
                    Year = 1940,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.34f,
                    SeaLevel = 14.35222333f,
                    Temp = -12.24326f,
                    Year = 1941,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.34f,
                    SeaLevel = 14.35222333f,
                    Temp = -12.24326f,
                    Year = 1942,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.84f,
                    SeaLevel = 14.35732167f,
                    Temp = -16.94326f,
                    Year = 1943,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 313.88f,
                    SeaLevel = 14.18043223f,
                    Temp = -5.54326f,
                    Year = 1944,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 314.63f,
                    SeaLevel = 14.60734743f,
                    Temp = -5.84326f,
                    Year = 1945,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 314.63f,
                    SeaLevel = 14.25638287f,
                    Temp = -16.84326f,
                    Year = 1946,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 314.66f,
                    SeaLevel = 14.4138078f,
                    Temp = -10.64326f,
                    Year = 1947,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 314.88f,
                    SeaLevel = 14.48694413f,
                    Temp = -8.94326f,
                    Year = 1948,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 315.95f,
                    SeaLevel = 14.17366973f,
                    Temp = -1.94326f,
                    Year = 1949,
                }
            );
            _climateModelWorldStatesQueue.Enqueue(
                new()
                {
                    Pollution = 315.67f,
                    SeaLevel = 14.36671727f,
                    Temp = 1.35674f,
                    Year = 1950,
                }
            );
        }
    }
}
