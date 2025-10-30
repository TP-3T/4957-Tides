using System;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using UnityEngine;

public class MapManager : GenericSingleton<MapManager>
{
    [SerializeField]
    private HexGrid hexGrid;

    [SerializeField]
    private Sea sea;

    [SerializeField]
    private GameEvent MapLoadFinishEvent;

    public void OnNewMap(UnityEngine.Object eventArgs)
    {
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
        MapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = true });
    }
}
