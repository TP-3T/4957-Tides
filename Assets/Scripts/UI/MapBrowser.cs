using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using TTT.DataClasses.HexData;
using UnityEngine;
using UnityEngine.UI;
using static TTT.Helpers.MapDatabaseService;

namespace TTT.UI
{
    public class MapBrowser : MonoBehaviour
    {
        private static readonly WaitForSeconds _waitForSeconds = new(3);

        [SerializeField]
        private MainMenu MainMenu;

        [SerializeField]
        public GameObject MapListItem;

        [SerializeField]
        public GameObject DBContent;

        [SerializeField]
        public GameObject LocalContent;

        [SerializeField]
        private Button SelectButton;
        public int selectedMapId;

        [SerializeField]
        private GameObject LoadingPanel;

        private MapData localMapData = new();
        private List<MapInfo> localMapInfoList = new();
        private List<GameObject> instantiatedMapItems = new();

        private string folderPath;

        void Awake()
        {
            folderPath = Application.persistentDataPath;
        }

        private void OnEnable()
        {
            FetchMapList();
            StartCoroutine(FetchLocalMapList());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            ClearMapItems();
        }

        private void ClearMapItems()
        {
            foreach (var item in instantiatedMapItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            instantiatedMapItems.Clear();
        }

        public void FetchMapList()
        {
            // Fetch all maps store into list, iterate through the list and create a corresponding MapListItem and append to C
            // and add it to the MapsDisplay
            StartCoroutine(
                GetMapList(
                    onSuccess: (mapList) =>
                    {
                        OnMapListFetched(mapList, DBContent);
                        instantiatedMapItems.ForEach(item =>
                        {
                            item.GetComponent<MapListItemController>().MapSize.enabled =
                                false;
                        });
                    },
                    onError: (error) => OnMapListError(error)
                )
            );
        }

        public IEnumerator FetchLocalMapList()
        {
            string[] jsonFileNames = Directory.GetFiles(folderPath, "*.json");

            foreach (var jsonFileName in jsonFileNames)
            {
                string jsonString = File.ReadAllText(jsonFileName);

                Debug.Log(jsonFileName);

                localMapData = JsonConvert.DeserializeObject<MapData>(
                    jsonString
                );
                var newInfo = new MapInfo(
                    localMapData.MapID,
                    localMapData.MapName,
                    localMapData.SteamID
                );
                localMapInfoList.Add(newInfo);
                var item = OnMapListItemFetched(newInfo, LocalContent);
                item.MapSize.text =
                    $"{localMapData.MapTile.Count} x {localMapData.MapTile["0"].Count}";
                yield return null;
            }
        }

        public void OnSelectMap()
        {
            StartCoroutine(NewMethod());
        }

        private IEnumerator NewMethod()
        {
            SelectButton.gameObject.SetActive(false);
            LoadingPanel.SetActive(true);
            var loadingText =
                LoadingPanel.GetComponentInChildren<TextMeshProUGUI>();
            var filePath = Path.Combine(folderPath, $"{selectedMapId}.json");
            if (!File.Exists(filePath))
            {
                loadingText.text = "Downloading...";
                var getter = FetchMapByMapId(
                    selectedMapId,
                    onSuccess: mapData => SaveMap(mapData, filePath),
                    onError: error =>
                        StartCoroutine(LogError(loadingText, error))
                );
                while (getter.MoveNext())
                {
                    yield return null;
                }
            }
            else
            {
                MainMenu.selectedMap = filePath;
                StartCoroutine(LogError(loadingText, "Map loaded!"));
            }
        }

        private void SaveMap(string mapData, string filePath)
        {
            MapData mapJson = JsonConvert.DeserializeObject<MapData>(mapData);

            File.WriteAllText(
                filePath,
                JsonConvert.SerializeObject(mapJson, Formatting.Indented)
            );
            MainMenu.selectedMap = filePath;
            var loadingText =
                LoadingPanel.GetComponentInChildren<TextMeshProUGUI>();
            StartCoroutine(LogError(loadingText, "Map loaded!"));
        }

        private IEnumerator LogError(TextMeshProUGUI where, string what)
        {
            where.text = what;
            yield return _waitForSeconds;
            LoadingPanel.SetActive(false);
        }

        private void OnMapListFetched(
            List<MapInfo> mapInfoList,
            GameObject content
        )
        {
            mapInfoList.ForEach(item => OnMapListItemFetched(item, content));
        }

        private MapListItemController OnMapListItemFetched(
            MapInfo mapInfo,
            GameObject content
        )
        {
            GameObject mapItem = Instantiate(MapListItem, content.transform);
            instantiatedMapItems.Add(mapItem);

            var controller = mapItem.GetComponent<MapListItemController>();

            controller.MapID.text = mapInfo.MapId.ToString();
            controller.MapName.text = mapInfo.MapName;
            Button selectButton = mapItem.GetComponent<Button>();

            selectButton.onClick.RemoveAllListeners();

            selectButton.onClick.AddListener(() =>
            {
                selectedMapId = mapInfo.MapId;
                SelectButton.gameObject.SetActive(true);
            });

            return controller;
        }

        private void OnMapListError(string errorMessage)
        {
            Debug.LogError($"Failed to fetch map list: {errorMessage}");
        }
    }
}
