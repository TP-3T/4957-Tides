using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using TTT.DataClasses.MapData;
using UnityEngine;
using UnityEngine.UI;
using static TTT.Helpers.MapDatabaseService;

namespace TTT.UI
{
    public class MapBrowser : MonoBehaviour
    {
        [SerializeField]
        private MainMenu MainMenu;

        [SerializeField]
        public GameObject MapListItem;

        [SerializeField]
        public GameObject DBContent;

        [SerializeField]
        public GameObject LocalContent;
        public int selectedMapId;

        public GameObject activeToggle;
        private MapData localMapData = new();
        private List<MapInfo> localMapInfoList = new();

        void Start() { }

        public void FetchMapList()
        {
            // Fetch all maps store into list, iterate through the list and create a corresponding MapListItem and append to C
            // and add it to the MapsDisplay
            StartCoroutine(
                GetMapList(
                    onSuccess: (mapList) =>
                        OnMapListFetched(mapList, DBContent),
                    onError: (error) => OnMapListError(error)
                )
            );
        }

        public void FetchLocalMapList()
        {
            string folderPath = "./Assets/Maps";
            string[] jsonFileNames = Directory.GetFiles(folderPath, "*.json");

            foreach (var jsonFileName in jsonFileNames)
            {
                string jsonString = File.ReadAllText(jsonFileName);

                Debug.Log(jsonFileName);

                localMapData = JsonConvert.DeserializeObject<MapData>(
                    jsonString
                );

                localMapInfoList.Add(
                    new MapInfo(
                        localMapData.MapID,
                        localMapData.MapName,
                        localMapData.SteamID
                    )
                );
            }

            OnMapListFetched(localMapInfoList, LocalContent);
        }

        // OnClick method for select button triggers GetActiveToggle
        //  IF there is an activeToggle (through isOn), then we get the text (mapName) from the component
        //  Use it to get the MapID through the dictionary
        //  query for MapData from brysons stuff using MapID
        //

        /// <summary>
        /// RUNS
        /// </summary>
        /// <returns></returns>
        public void GetActiveToggle()
        {
            // Retrieve all the MapListItems (gameobjects)
            // check for if the isOn property is checked
            Toggle[] dbToggles = DBContent.GetComponentsInChildren<Toggle>();
            Toggle[] localToggles =
                LocalContent.GetComponentsInChildren<Toggle>();

            Toggle[] toggles = dbToggles.Concat(localToggles).ToArray();

            Debug.Log($"Toggles found: {toggles.Length}");
            foreach (var toggle in toggles)
            {
                if (toggle.isOn)
                {
                    var controller =
                        toggle.GetComponent<MapListItemController>();
                    int mapId = controller.MapId;
                    selectedMapId = mapId;
                    Debug.Log($"Selected Map ID: {selectedMapId}");

                    if (toggle.transform.IsChildOf(DBContent.transform))
                    {
                        Debug.Log(
                            "Map selected from online DB, fetching data..."
                        );
                        StartCoroutine(
                            FetchMapByMapId(
                                selectedMapId,
                                onSuccess: (mapData) =>
                                {
                                    Debug.Log(
                                        $"Map Data fetched for Map ID: {selectedMapId}"
                                    );

                                    MapData mapJson =
                                        JsonConvert.DeserializeObject<MapData>(
                                            mapData
                                        );

                                    string localPath = $"{selectedMapId}";

                                    File.WriteAllText(
                                        "./Assets/Resources/"
                                            + localPath
                                            + ".json",
                                        JsonConvert.SerializeObject(
                                            mapJson,
                                            Formatting.Indented
                                        )
                                    );
                                    MainMenu.selectedMap = localPath;

                                    Debug.Log($"Map data saved to {localPath}");
                                },
                                onError: (error) =>
                                {
                                    Debug.LogError(
                                        $"Failed to fetch map data: {error}"
                                    );
                                }
                            )
                        );
                    }
                    else
                    {
                        Debug.Log(
                            "Map selected from local files, no fetch needed."
                        );
                    }
                }
            }
        }

        private void OnMapListFetched(
            List<MapInfo> mapInfoList,
            GameObject content
        )
        {
            Debug.Log(mapInfoList.Count);

            foreach (var mapInfo in mapInfoList)
            {
                GameObject mapItem = Instantiate(
                    MapListItem,
                    content.transform
                );

                var controller = mapItem.AddComponent<MapListItemController>();
                controller.MapId = mapInfo.MapId;

                mapItem.GetComponentInChildren<TMP_Text>().text =
                    mapInfo.MapName;

                Debug.Log("Map Info Created.0.001f");
            }
        }

        private void OnMapListError(string errorMessage)
        {
            Debug.LogError($"Failed to fetch map list: {errorMessage}");
        }

        // Update is called once per frame
        void Update() { }
    }
}
