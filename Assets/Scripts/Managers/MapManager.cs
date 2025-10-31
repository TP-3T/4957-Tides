using System;
using Mono.Cecil;
using PlasticPipe.PlasticProtocol.Messages;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using TTT.DataClasses.HexData;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MapManager : GenericSingleton<MapManager>
{
    /**
    Serialize fields for the HexMesh, instances that are required for each client
    Serialize fields for the SeaMesh, instances that are requried for each client
    */

    [SerializeField]
    private GameEvent MapLoadFinishEvent;

    [SerializeField]
    private HexGrid _hexGrid;

    [SerializeField]
    private Sea _sea;

    [SerializeField]
    private MapData _gameMapData;

    private HexCell[,] _hexCells;

    void Start()
    {
        // sea     = Addressables.LoadAssetAsync<>();
        // hexGrid = Addressables.LoadAssetAsync<>();
    }

    public void OnNewMap(UnityEngine.Object eventArgs)
    {
        Debug.Log("I am being raised.");

        NewMapEventArgs args = eventArgs as NewMapEventArgs;

        try
        {
            _hexGrid = GetComponentInChildren<HexGrid>();
            _sea = GetComponentInChildren<Sea>();

            if (IsServer)
            {
                Debug.Log("Server building the map");

                // Deserialized data (cringe)
                _gameMapData = JsonUtility.FromJson<MapData>(args.DataFile.text);

                if (_gameMapData == null)
                {
                    throw new Exception($"{args.DataFile.name} is not a valid TTT Map object.");
                }


                // Actual game data (based)
                _hexGrid.BuildMap(_gameMapData, _hexCells);

                MapBuildRpc();
            }
            else if (IsClient)
            {
                Debug.Log("Client recevies or listens for the map");

            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            MapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = false });
        }

        MapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = true });
    }

    public void OnNextTurn(UnityEngine.Object eventArgs)
    {
        Debug.Log("The next turn event has been raised.");
    }

    [Rpc(SendTo.Everyone)]
    public void MapBuildRpc()
    {
        Debug.Log("The map has been built.");
    }
}
