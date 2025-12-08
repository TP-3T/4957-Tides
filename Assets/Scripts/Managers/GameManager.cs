using System.Collections.Generic;
using System.Linq;
using TTT.ClimateModel;
using TTT.DataClasses;
using TTT.DataClasses.HexData;
using TTT.DataClasses.PlayerResources;
using TTT.DataClasses.States;
using TTT.GameEvents;
using TTT.Helpers;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace TTT.Managers
{
    public class GameManager : GenericNetworkSingleton<GameManager>
    {
        [field: SerializeField]
        public List<PlayerResource> PlayerResources { get; private set; }

        [field: SerializeField]
        public PlayerStats PlayerStats { get; private set; }

        [field: SerializeField]
        public InteractionMode InteractionMode { get; private set; }

        public NetworkVariable<ulong> CurrentPlayerId = new();

        [field: SerializeField]
        public int Year { get; private set; } = 1;

        [SerializeField]
        public bool FTTaken { get; private set; } = false;

        [field: SerializeField]
        public Seasons Season { get; private set; }

        [SerializeField]
        private GameEvent startTurnEvent;

        [SerializeField]
        private GameEvent endTurnEvent;

        [SerializeField]
        private GameEvent endingSeasonEvent;

        [SerializeField]
        private GameEvent endingYearEvent;

        [SerializeField]
        private GameEvent BuildingFeatureEvent;

        [SerializeField]
        private GameEvent SystemStateChange;

        // Initial climate values
        public static readonly float INITIAL_SEA_LEVEL_M = 1.0f;
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

        void Start()
        {
            NetworkManager.Singleton.enabled = true;
            Season = Seasons.Spring;
            SystemStateChange.Raise(
                new StateSystemChangeEventArgs()
                {
                    NewState = SystemState.MAIN_MENU,
                }
            );
        }

        public void OnGlobalInformationChanged(
            GlobalInformation oldI,
            GlobalInformation newI
        )
        {
            // Debug.Log(
            //     $"[GameManager] global information modified. old {oldI}, new {newI}"
            // );
        }

        #region:Utility

        /// <summary>
        /// Call from a client RPC and get information of clients (multiplayer debugging).
        /// </summary>
        private void NetworkingInformationLog()
        {
            var self = NetworkManager.Singleton.LocalClient;
            // Debug.Log($"[GameManager] client rpc, connected players {NetworkManager.Singleton.ConnectedClientsList.Count}");
            // Debug.Log($"[GameManager] client rpc, current player id {self.ClientId}");
            // Debug.Log($"[GameManager] client rpc, current turn guy {CurrentPlayerId}");
        }

        /// <summary>
        /// Increments the season, and the year if applicable.
        /// </summary>
        private void EndSeason()
        {
            endingSeasonEvent.Raise();
            Season = Season.NextEnumValue();
        }

        private void EndYear()
        {
            // Debug.Log(
            //     "Year has changed, this should go in a AI manager or just query the AI here  - GameManager line 124"
            // );

            Year += 1;

            // Calculate and apply sea level change based on pollution
            if (PlayerStats != null)
            {
                // float seaLevelIncrease = PlayerStats.CalculateSeaLevelFromPollution();
                // MapManager.Instance.SeaLevel.Value += seaLevelIncrease;
                // Debug.Log($"Sea level increased by {seaLevelIncrease} due to pollution");
            }

            endingYearEvent.Raise();
        }

        #endregion

        #region:RPC Definitions

        [Rpc(SendTo.ClientsAndHost)]
        public void OnTurnEndingClientRpc(int year, FixedString32Bytes season)
        {
            NetworkingInformationLog();
            // Debug.Log($"[GameManager] on client turn ending matches current {self == nextClient}");
            // Debug.Log($"[GameManager] on client turn ending client rpc {NetworkManager.Singleton.LocalClientId}, start turn");
            // startTurnEvent.Raise(new NextTurnEventArgs() {});

            endTurnEvent.Raise(
                new EndTurnEventArgs()
                {
                    Year = year,
                    Season = season.ToString(),
                }
            );
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void StartNextTurnClientRpc(RpcParams paramS = default)
        {
            // Debug.Log(
            //     $"[GameManager] client rpc IT SHOULD ONLY BE ME {NetworkManager.Singleton.LocalClientId}"
            // );
            startTurnEvent.Raise();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void OnTurnEndingServerRpc()
        {
            // Debug.Log($"{Season.Equals(Seasons[0])}");
            // Debug.Log($"[GameManager] server rpc, current season {Season}");
            // Debug.Log($"[GameManager] server rpc, first season {Seasons[0]}");

            ulong nextPlayerId =
                (CurrentPlayerId.Value + 1)
                % ((ulong)NetworkManager.Singleton.ConnectedClientsList.Count);

            if (FTTaken && nextPlayerId == 0) // The next season
                EndSeason();
            if (FTTaken && nextPlayerId == 0 && Season.Equals(Seasons.Spring)) // The year is over
                EndYear();

            CurrentPlayerId.Value = nextPlayerId;
            FTTaken = true;

            OnTurnEndingClientRpc(Year, Season.ToString());
            StartNextTurnClientRpc(
                RpcTarget.Single(nextPlayerId, RpcTargetUse.Temp)
            );
        }

        #endregion

        #region: SCROBJECT Handlers

        public void OnStartNetworkEvent(Object eventArgs)
        {
            StartNetworkEventArgs args = eventArgs as StartNetworkEventArgs;
            try
            {
                // Clean up IP string to remove any hidden characters and invalid chars using a for loop
                string ipRaw = args.Ip;
                var ipBuilder = new System.Text.StringBuilder();
                for (int i = 0; i < ipRaw.Length; i++)
                {
                    char c = ipRaw[i];
                    if (char.IsDigit(c) || c == '.' || c == ':')
                    {
                        ipBuilder.Append(c);
                    }
                }
                string cleanIp = ipBuilder.ToString().Trim();

                // Configure transport with IP and Port from args using SetConnectionData
                var transport =
                    NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

                if (args.IsHost)
                {
                    // For host, use "0.0.0.0" as listen address to accept connections on all interfaces
                    transport.SetConnectionData(cleanIp, args.Port, cleanIp);
                    // Debug.Log($"Starting host on {cleanIp}:{args.Port}");
                    NetworkManager.Singleton.StartHost();
                    CurrentPlayerId.Value = NetworkManager
                        .Singleton
                        .LocalClientId;
                }
                else
                {
                    // For client, use the IP as the listen address parameter (not actually used by client)
                    transport.SetConnectionData(cleanIp, args.Port, cleanIp);
                    // Debug.Log($"Starting client connecting to {cleanIp}:{args.Port}");
                    NetworkManager.Singleton.StartClient();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start network: {e.Message}");
                return;
            }
        }

        /// <summary>
        /// Appease the SCROBJECT event handler 👌😉
        /// </summary>
        /// <param name="_"></param>
        public void OnTurnEnding(Object _)
        {
            OnTurnEndingServerRpc();
        }

        public void OnPlayerLose(Object _)
        {
            // Debug.Log("Player has lost the game.");
        }

        public bool CanEndTurn()
        {
            // NetworkingInformationLog();

            bool hasEnoughResources = PlayerResources.All(resources =>
                resources.AmountOwned >= 0
            );

            return hasEnoughResources;
        }

        public void OnNewMapFinish(Object eventArgs)
        {
            NewMapFinishedEventArgs args = eventArgs as NewMapFinishedEventArgs;

            if (args.WasSuccessful)
            {
                // Seed the historical climate data into the climate model's internal queue
                ClimatePredictionModel.ResetWorldQueue();
                SystemStateChange.Raise(
                    new StateSystemChangeEventArgs()
                    {
                        NewState = SystemState.PLAYING,
                    }
                );
            }
            else
            {
                Debug.LogWarning("MAP FAILED TO LOAD! RETURNING TO MAIN MENU!");
                SystemStateChange.Raise(
                    new StateSystemChangeEventArgs()
                    {
                        NewState = SystemState.MAIN_MENU,
                    }
                );
            }
        }

        #endregion
    }
}
