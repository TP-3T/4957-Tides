using TTT.GameEvents;
using TTT.Helpers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TTT.Managers
{
    public class GameManager : GenericNetworkSingleton<GameManager>
    {
        [SerializeField]
        private TextAsset LevelFile;

        [SerializeField]
        private GameEvent newMapEvent;

        private string[] Seasons = { "Spring", "Summer", "Fall", "Winter" };

        //serialize for now
        [SerializeField] private int Year = 1;
        [SerializeField] private string Season;
        [SerializeField] private int CO2;
        [SerializeField] public GameEvent _OnYearChangeEvent;

        [SerializeField] public GameEvent _FloodEvent;


        // private AssetReference SeaPrefab = new("P_Sea");

        // private AssetReference HexGrid = new("P_HexGrid");

        // private GameObject sea;
        // private GameObject hexGrid;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.Season = Seasons[0];
            this.CO2 = 0;
            // NetworkManager.Singleton.OnServerStarted += ServerStartHandler;
        }

        public void OnStartNetworkEvent(Object eventArgs)
        {
            StartNetworkEventArgs args = eventArgs as StartNetworkEventArgs;
            if (args.IsHost)
            {
                Debug.Log("I am being spawned as a host");
                NetworkManager.Singleton.StartHost();

                newMapEvent.Raise(new NewMapEventArgs()
                {
                    DataFile = LevelFile
                });
            }
            else
            {
                Debug.Log("I am being spawned as a client");
                NetworkManager.Singleton.StartClient();
            }
        }

        // private void SpawnSea(GameObject obj)
        // {
        //     sea = Instantiate(obj);
        //     sea.GetComponent<NetworkObject>().Spawn();
        // }

        // private void SpawnGrid(GameObject obj)
        // {
        //     hexGrid = Instantiate(obj);
        //     hexGrid.GetComponent<NetworkObject>().Spawn();
        //     newMapEvent.Raise(new NewMapEventArgs() { DataFile = LevelFile });
        // }

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

        /// <summary>
        /// Increments the season, and the year if applicable.
        /// </summary>
        public void IncrementSeason()
        {
            int currentSeasonIndex = System.Array.IndexOf(Seasons, Season);

            // % to wrap around to the beginning after winter
            int nextSeasonIndex = (currentSeasonIndex + 1) % this.Seasons.Length;

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
            Debug.Log("Last Player Made Turn, increment season - GameManager line 118");
            this.IncrementSeason();
        }

        public void OnYearChange(UnityEngine.Object eventArgs)
        {
            Debug.Log("Year has changed, this should go in a AI manager or just query the AI here  - GameManager line 124");

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


        // /// <summary>
        // /// Method from INextTurnListener interface. Called when Next Turn event is dispatched.
        // /// </summary>
        // /// <param name="nt">The Next Turn Event</param>
        // public void OnEventRaised(NextTurn nt)
        // {
        //     // increment season here
        //     Debug.Log("1. Increment Season -GameManager" + nt.ToString());
        //     this.IncrementSeason();
        // }
    }
}
