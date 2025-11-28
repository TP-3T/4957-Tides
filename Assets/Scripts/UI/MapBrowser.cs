using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using Codice.Client.BaseCommands;
using Newtonsoft.Json;
using TMPro;
using TTT.DataClasses.MapData;
using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEngine;
using UnityEngine.UI;
using static TTT.Helpers.MapDatabaseService;

namespace TTT.UI
{
    public class MapBrowser : MonoBehaviour
    {
        [SerializeField]
        public GameObject MapListItem;

        [SerializeField]
        public GameObject DBContent;

        [SerializeField]
        public GameObject LocalContent;
        public int selectedMapId;

        public GameObject activeToggle;

        private Dictionary<string, int> DBMapIds = new();
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
        // public string GetActiveToggle()
        // {
        //     // Retrieve all the MapListItems (gameobjects)
        //     // check for if the isOn property is checked

        //     // need to check select



        //     // List<GameObject> listItems = DBContent.GetCompo
        // }

        private void OnMapListFetched(
            List<MapInfo> mapInfoList,
            GameObject content
        )
        {
            Debug.Log(mapInfoList.Count);

            foreach (var mapInfo in mapInfoList)
            {
                DBMapIds.Add(mapInfo.MapName, mapInfo.MapId);

                GameObject mapItem = Instantiate(
                    MapListItem,
                    content.transform
                );

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
