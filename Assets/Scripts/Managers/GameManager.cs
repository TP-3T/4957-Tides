using System.Collections;
using TTT.GameEvents;
using TTT.Helpers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using TTT.UI;
using TTT.Dispatch;

namespace TTT.Managers
{
    public class GameManager : GenericSingleton<GameManager>, IGameEventListener<NextTurn>
    {
        [SerializeField]
        private TextAsset LevelFile;

        [SerializeField]
        private GameEvent newMapEvent;

        private string[] Seasons = { "Spring", "Summer", "Fall", "Winter" };

        //serialize for now
        [SerializeField] private int Year = 1;
        [SerializeField] private string Season;


        // private AssetReference SeaPrefab = new("P_Sea");

        // private AssetReference HexGrid = new("P_HexGrid");

        // private GameObject sea;
        // private GameObject hexGrid;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.Season = Seasons[Year];
            // NetworkManager.Singleton.OnServerStarted += ServerStartHandler;
        }

        /// <summary>
        /// Unity built-in method, gets called once at the very beginning.
        /// </summary>
        new void Awake()
        {
            GameEventDispatch.RegisterListener(this);
        }

        /// <summary>
        /// Unity built-in method, gets called when the object is being destroyed.
        /// </summary>
        void OnDestroy()
        {
            GameEventDispatch.UnregisterListener(this);
        }

        public void OnStartNetworkEvent(Object eventArgs)
        {
            StartNetworkEventArgs args = eventArgs as StartNetworkEventArgs;
            if (args.IsHost)
            {
                NetworkManager.Singleton.StartHost();
            }
            else
            {
                NetworkManager.Singleton.StartClient();
            }

            newMapEvent.Raise(new NewMapEventArgs() { DataFile = LevelFile });
        }

        // private void ServerStartHandler()
        // {
        //     StartCoroutine(LoadAssets());
        // }

        // private IEnumerator LoadAssets()
        // {
        //     yield return AssetLoader<GameObject>.Load(HexGrid, SpawnGrid);
        //     yield return AssetLoader<GameObject>.Load(SeaPrefab, SpawnSea);
        // }

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
            }
        }

        /// <summary>
        /// Increments the year by one.
        /// </summary>
        public void IncrementYear()
        {
            this.Year += 1;
        }

        /// <summary>
        /// Method from INextTurnListener interface. Called when Next Turn event is dispatched.
        /// </summary>
        /// <param name="nt">The Next Turn Event</param>
        public void OnEventRaised(NextTurn nt)
        {
            // increment season here
            Debug.Log("1. Increment Season -GameManager" + nt.ToString());
            this.IncrementSeason();
        }
    }
}
