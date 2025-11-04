using System;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using Unity.Netcode;
using UnityEngine;

public class MapManager : GenericSingleton<MapManager>
{
    [SerializeField]
    private HexGrid hexGrid;

    [SerializeField]
    private Sea sea;

    [SerializeField]
    private GameEvent MapLoadFinishEvent;

    void Start()
    {
        hexGrid = GetComponentInChildren<HexGrid>();
        sea = GetComponentInChildren<Sea>();
    }

    public void OnNewMap(UnityEngine.Object eventArgs)
    {
        Debug.Log("I am being raised.");

        NewMapEventArgs args = eventArgs as NewMapEventArgs;
        try
        {
            hexGrid.BuildNewMap(args.DataFile);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            MapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = false });
        }

        // sea.GetComponent<NetworkObject>().Spawn();
        // hexGrid.GetComponent<NetworkObject>().Spawn();

        MapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = true });
    }
}
