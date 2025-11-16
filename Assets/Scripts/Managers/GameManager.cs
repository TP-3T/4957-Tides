using TTT.DataClasses.States;
using TTT.DataClasses.TileFeatures;
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
        public TextAsset LevelFile;

        [SerializeField]
        private GameEvent newMapEvent;

        private string[] Seasons = { "Spring", "Summer", "Fall", "Winter" };

        //serialize for now
        [SerializeField]
        private int Year = 1;

        [SerializeField]
        private string Season;

        [SerializeField]
        private int CO2;

        public GameEvent SeasonChanging;

        [SerializeField]
        public GameEvent _OnYearChangeEvent;

        [SerializeField]
        public GameEvent _FloodEvent;

        private InteractionMode interactionMode;

        private FeatureType buildingFeatureType;

        public GameEvent BuildingFeatureEvent;

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

        public void OnStartNetworkEvent(UnityEngine.Object eventArgs)
        {
            StartNetworkEventArgs args = eventArgs as StartNetworkEventArgs;
            if (args.IsHost)
            {
                Debug.Log("I am being spawned as a host");
                try
                {
                    NetworkManager.Singleton.StartHost();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to start host: {e.Message}");
                    return;
                }

                newMapEvent.Raise(new NewMapEventArgs() { DataFile = LevelFile });
            }
            else
            {
                // Debug.Log("I am being spawned as a client");
                NetworkManager.Singleton.StartClient();
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
            SeasonChanging.Raise();
            this.IncrementSeason();
        }

        public void OnYearChange(UnityEngine.Object eventArgs)
        {
            Debug.Log(
                "Year has changed, this should go in a AI manager or just query the AI here  - GameManager line 124"
            );

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
        /// Handles mesh click logic for buildmode to raise build event.
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
                var args = ScriptableObject.CreateInstance<BuildingFeatureArgs>();
                args.Location = clickedArgs.ClickedPoint;
                args.FeatureType = buildingFeatureType;

                if (buildingFeatureType == null)
                {
                    return;
                }

                BuildingFeatureEvent.Raise(args);
            }
        }
    }
    
}
