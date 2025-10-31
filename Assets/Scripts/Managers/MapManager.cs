using System;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using TTT.DataClasses.HexData;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using TTT.Managers;

public class MapManager : GenericSingleton<MapManager>
{
    /**
    Serialize fields for the HexMesh, instances that are required for each client
    Serialize fields for the SeaMesh, instances that are requried for each client
    */

    [SerializeField]
    private AssetReference _hexGridMeshAsset = new("P_HexMesh");

    [SerializeField]
    private AssetReference _seaMeshAsset = new("P_SeaMesh");

    [SerializeField]
    private TextAsset _jsonMap;

    [SerializeField]
    private HexGrid _hexGrid;

    [SerializeField]
    private GameEvent MapLoadFinishEvent;

    [SerializeField]
    private Sea _sea;

    [SerializeField]
    private MapData _gameMapData;

    private HexCell[,] _hexCells;

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
    }

    public void OnNewMap(UnityEngine.Object eventArgs)
    {
        NewMapEventArgs args = eventArgs as NewMapEventArgs;

        try
        {
            _hexGrid = GetComponentInChildren<HexGrid>();
            _sea = GetComponentInChildren<Sea>();

            // Deserialized data (cringe)
            _gameMapData = JsonUtility.FromJson<MapData>(args.DataFile.text);

            if (_gameMapData == null)
            {
                throw new Exception($"{args.DataFile.name} is not a valid TTT Map object.");
            }

            _hexGrid.BuildMap(_gameMapData, out _hexCells);

            // MapBuildRpc();
            // MapMeshBuildRpc();      // On connect of any client

            StartCoroutine(SpawnMapObjects());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            MapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = false });
        }

    }

    private IEnumerator SpawnMapObjects()
    {
        yield return AssetLoader<GameObject>.Load(_hexGridMeshAsset, SpawnGrid);

        MapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = true });
    }

    private void SpawnGrid(GameObject hm)
    {
        // Get reference to HexMesh prefab
        GameObject hexMeshGameObject    = Instantiate(hm);
        HexMesh    hexMeshInstance      = hexMeshGameObject.GetComponent<HexMesh>();

        // Instance HexMesh prefab based off of the build data
        hexMeshInstance.GetComponent<NetworkObject>().Spawn();

        // Triangulate that bad boy
        hexMeshInstance.Triangulate(_hexCells, HexGrid.HexSize, HexGrid.HexOrientation);
    }

    // private void SpawnSea(SeaMesh sm)
    // {
    //     // Get reference to SeaMesh prefab
    //     var inst = Instantiate(sm);

    //     // Instance SeaMesh prefab based off of the build data
    //     inst.GetComponent<NetworkObject>().Spawn();
    // }

    public void OnClientConnect(ulong clientId)
    {
        Debug.Log($"Hello mr {clientId}");

        // Need to get client instance of the mesh 

        HexMesh c_hexMesh = FindFirstObjectByType<HexMesh>();       // Get the hex mesh in the scene

        Debug.Log(c_hexMesh);

        // How do I trangulate from this location?
        c_hexMesh.Triangulate(_hexCells, HexGrid.HexSize, HexGrid.HexOrientation);
    }

    public void OnNextTurn(UnityEngine.Object eventArgs)
    {
        Debug.Log("The next turn event has been raised.");
    }

    [Rpc(SendTo.Everyone)]
    public void TestRpc()
    {
        Debug.Log("This is a test.");
    }

    [Rpc(SendTo.Everyone)]
    public void MapBuildRpc()
    {
        // Actual game data (based)

        Debug.Log("The map has been built.");
    }
}
