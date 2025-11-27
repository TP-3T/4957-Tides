using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static TTT.Helpers.MapDatabaseService;

public class MapBrowser : MonoBehaviour
{
    [SerializeField]
    public GameObject MapListItem;

    [SerializeField]
    public GameObject Content;

    void Start() { }

    public void FetchMapList()
    {
        // Fetch all maps store into list, iterate through the list and create a corresponding MapListItem and append to C
        // and add it to the MapsDisplay
        StartCoroutine(
            GetMapList(
                onSuccess: (mapList) => OnMapListFetched(mapList),
                onError: (error) => OnMapListError(error)
            )
        );
    }

    private void OnMapListFetched(List<MapInfo> mapInfoList)
    {
        Debug.Log(mapInfoList.Count);

        foreach (var mapInfo in mapInfoList)
        {
            GameObject mapItem = Instantiate(MapListItem, Content.transform);

            mapItem.GetComponentInChildren<TMP_Text>().text = mapInfo.MapName;

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
