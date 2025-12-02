using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codice.Client.BaseCommands;
using NUnit.Framework.Constraints;
using TTT.DataClasses;
using TTT.DataClasses.PlayerResources;
using TTT.DataClasses.States;
using TTT.GameEvents;
using TTT.Helpers;
using Unity.Netcode;
using UnityEditor.SearchService;
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

        private readonly string[] Seasons =
        {
            "Spring",
            "Summer",
            "Fall",
            "Winter",
        };

        public NetworkVariable<ulong> CurrentPlayerId = new ();

        [field: SerializeField]
        public int Year { get; private set; } = 1;
        [field: SerializeField]
        public string Season { get; private set; }
        [field: SerializeField]
        public int CO2 { get; private set; } = 0;
        [field: SerializeField]
        public int Temperature { get; private set; }
        [SerializeField]
        public bool FTTaken { get; private set; } = false;

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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Season = Seasons[0];
            CO2 = 0;
        }

        public void OnGlobalInformationChanged(GlobalInformation oldI, GlobalInformation newI)
        {
            Debug.Log($"[GameManager] global information modified. old {oldI}, new {newI}");
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

            CurrentPlayerId.Value = NetworkManager.Singleton.LocalClientId;
        }

        /// <summary>
        /// Increments the season, and the year if applicable.
        /// </summary>
        private void EndSeason()
        {
            endingSeasonEvent.Raise();
            int currentSeasonIndex = System.Array.IndexOf(Seasons, Season);
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

        #endregion

        #region:RPC Definitions

        [Rpc(SendTo.ClientsAndHost)]
        public void OnTurnEndingClientRpc()
        {
            NetworkingInformationLog();
            // Debug.Log($"[GameManager] on client turn ending matches current {self == nextClient}");
            // Debug.Log($"[GameManager] on client turn ending client rpc {NetworkManager.Singleton.LocalClientId}, start turn");
            // startTurnEvent.Raise(new NextTurnEventArgs() {});

            endTurnEvent.Raise(new EndTurnEventArgs()
            {
                Year = Year,
                Season = Season
            });
        }

        [Rpc(SendTo.SpecifiedInParams)]
        public void StartNextTurnCilentRpc(RpcParams paramS = default)
        {
            Debug.Log($"[GameManager] cilent rpc IT SHOULD ONLY BE ME {NetworkManager.Singleton.LocalClientId}");
            startTurnEvent.Raise();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void OnTurnEndingServerRpc()
        {
            // Debug.Log($"{Season.Equals(Seasons[0])}");
            // Debug.Log($"[GameManager] server rpc, current season {Season}");
            // Debug.Log($"[GameManager] server rpc, first season {Seasons[0]}");

            ulong nextPlayerId = (CurrentPlayerId.Value + 1) % ((ulong)NetworkManager.Singleton.ConnectedClientsList.Count);

            if (FTTaken && nextPlayerId == 0)   // The next season
                EndSeason();
            if (FTTaken
                && nextPlayerId == 0
                && Season.Equals(Seasons[0]))   // The year is over
                EndYear();

            CurrentPlayerId.Value = nextPlayerId;
            FTTaken = true;

            OnTurnEndingClientRpc();
            StartNextTurnCilentRpc(RpcTarget.Single(nextPlayerId, RpcTargetUse.Temp));
        }

        #endregion

        #region: SCROBJECT Handlers

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
            Debug.Log("Player has lost the game.");
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

            if (!args.WasSuccessful)
            {
                Debug.LogWarning(
                    "MAP FAILED TO LOAD! WE SHOULD REVERT TO THE MAIN MENU FROM HERE!"
                );
            }
        }

        #endregion
    }
}
