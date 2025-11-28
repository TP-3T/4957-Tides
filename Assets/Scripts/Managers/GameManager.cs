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
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Serialization;

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

        // public NetworkList<ulong> ConnectedPlayers = new ();
        // public NetworkVariable<ulong> CurrentPlayer = new ();

        public NetworkClient CurrentPlayer { get; set; }    // THIS LINE IS MY ENEMEY
        public NetworkVariable<ulong> CurrentPlayerId = new ();

        //serialize for now
        [field: SerializeField]
        public int Year { get; private set; } = 1;

        [field: SerializeField]
        public string Season { get; private set; }

        [field: SerializeField]
        public int CO2 { get; private set; } = 0;

        [field: SerializeField]
        public bool FTTaken { get; private set; } = false;  // First turn done

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

            CurrentPlayer = NetworkManager.Singleton.LocalClient;
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

        [Rpc(SendTo.ClientsAndHost)]
        public void OnTurnEndingClientRpc()
        {
            var self = NetworkManager.Singleton.LocalClient;
            Debug.Log($"[GameManager] client rpc, current player id {self.ClientId}");
            Debug.Log($"[GameManager] client rpc, current turn guy {CurrentPlayerId.Value}");
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void OnTurnEndingServerRpc()
        {
            //Get all the connected clients
            // var ConnectedClientsList =
            //     NetworkManager.Singleton.ConnectedClientsList.ToList();
            // var self = NetworkManager.Singleton.LocalClient;
            // var last = ConnectedClientsList.Last();

            // Debug.Log($"[GameManager] current player null {CurrentPlayer == null}");
            // Debug.Log($"[GameManager] me player null {self == null}");
            // Debug.Log($"[GameManager] current player count {ConnectedClientsList.Count}");
            // Debug.Log($"[GameManager] last player id {last.ClientId}");
            // //If I am not the last connected client
            // if (!(last.ClientId == self.ClientId))
            // {
            //     Debug.Log("[GameManager] I AM NOT THE LAST CLIENT");
            //     //Increment the current client
            //     var currentIndex = ConnectedClientsList.FindIndex(c => c.ClientId == CurrentPlayer.ClientId);
            //     Debug.Log($"[GameManager] current index {currentIndex}");
            //     Debug.Log($"[GameManager] current index floored {currentIndex % ConnectedClientsList.Count}");
            //     CurrentPlayer = ConnectedClientsList[currentIndex + 1 % ConnectedClientsList.Count];
            //     StartNextTurn(new());
            // }
            // else
            // {
            //     Debug.Log("[GameManager] I AM THE LAST CLIENT");
            //     CurrentPlayer = ConnectedClientsList.First();
            //     // end the season before saying the turn ended
            //     EndSeason();
            //     if (Season.Equals(Seasons[0]))
            //     {
            //         EndYear();
            //     }
            //     else
            //     {
            //         StartNextTurn(new());
            //     }
            // }

            Debug.Log($"{Season.Equals(Seasons[0])}");
            Debug.Log($"[GameManager] server rpc, current season {Season}");
            Debug.Log($"[GameManager] server rpc, first season {Seasons[0]}");

            ulong nextClient = (CurrentPlayerId.Value + 1) % ((ulong)NetworkManager.Singleton.ConnectedClientsList.Count);

            if (FTTaken && nextClient == 0)             // The next season
                EndSeason();

            if (FTTaken 
                && nextClient == 0
                && Season.Equals(Seasons[0]))           // The year is over
                EndYear();

            CurrentPlayerId.Value = nextClient; 
            FTTaken = true;

            OnTurnEndingClientRpc();
            StartNextTurn(new());
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

        /// <summary>
        /// Appease the SCROBJECT event handler.        👌😉
        /// </summary>
        /// <param name="_"></param>
        public void OnTurnEnding(Object _)
        {
            OnTurnEndingServerRpc();
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
