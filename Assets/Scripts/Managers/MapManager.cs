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
using System.Collections.Generic;

// WO Notes regarding networking
// Basically we will need to tell the clients how to update their grid and sea meshes
// based off the game state data, that game state data will live in thie object on the server / host
// and be synchronised across all of the clients

public class MapManager : GenericSingleton<MapManager>
{
    /**
    Serialize fields for the HexMesh, instances that are required for each client
    Serialize fields for the SeaMesh, instances that are requried for each client
    */

    private AssetReference _hexGridAsset = new("P_HexGrid");

    private AssetReference _hexGridMeshAsset = new("P_HexMesh");

    private AssetReference _seaMeshAsset = new("P_SeaMesh");

    [SerializeField]
    private GameEvent MapLoadFinishEvent;

    private HexGrid _hexGrid;

    private Sea _sea;                           // Something that will be relevant in the future

    private MapData _gameMapData;

    private HexCell[,] _hexCells;

    private NetworkVariable<ulong> _hexGridId = new();

    private NetworkVariable<ulong> _hexMeshId = new();

    private const int CellsPerFrame = 100;

    public float SeaLevel;

    public float RisingRate;

    public Queue<HexCell> ToFlood;

    public Queue<HexCell> FloodQueue;

    public Queue<HexCell> FloodQueue2;

    public List<HexCell> Flooded;

    public override void OnNetworkSpawn()
    {
        RisingRate = 1.0f;
        SeaLevel = 0.0f;
        ToFlood = new();
        FloodQueue = new();
        FloodQueue2 = new();
        Flooded = new();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
    }

    public void OnNewMap(UnityEngine.Object eventArgs)
    {
        Debug.Log("This event was raised");

        NewMapEventArgs args = eventArgs as NewMapEventArgs;
        try
        {
            // Deserialized data (cringe)
            _gameMapData = JsonUtility.FromJson<MapData>(args.DataFile.text);

            if (_gameMapData == null)
            {
                throw new Exception($"{args.DataFile.name} is not a valid TTT Map object.");
            }

            _hexGrid.BuildMap(_gameMapData);

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

    private void SpawnGrid(GameObject hg)
    {
        GameObject hexGridGameObject = Instantiate(hg);
        HexGrid hexGrid = hexGridGameObject.GetComponent<HexGrid>();

        hexGrid.GetComponent<NetworkObject>().Spawn();
        hexGrid.BuildMap(_gameMapData);

        Debug.Log($"HEX GRID ID {hexGrid.NetworkObjectId}");

        _hexGridId.Value = hexGrid.NetworkObjectId;
    }

    private void SpawnGridMesh(GameObject hm)
    {
        // Get reference to HexMesh prefab
        GameObject hexMeshGameObject = Instantiate(hm);
        HexMesh hexMeshInstance = hexMeshGameObject.GetComponent<HexMesh>();

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

    public void StartRaiseSea()
    {
        StopAllCoroutines();
        StartCoroutine(RaiseSea());
    }

    /// <summary>
    /// Simulate rising on a per turn basis, not per frame.
    /// </summary>
    public IEnumerator RaiseSea()
    {
        this.SeaLevel += this.RisingRate;

        while (true)
        {
            // --- 1. Flood queue is empty, go through neighbours that were not eligable for flooding and see if they will be ---
            if (ToFlood.Count == 0)
            {
                Debug.Log("Flood fill cycle complete");

                while (this.FloodQueue.Count > 0)
                {
                    HexCell test = this.FloodQueue.Dequeue();

                    if (test.CellPosition.y <= (this.SeaLevel + this.RisingRate))
                        ToFlood.Enqueue(test);
                    else
                        this.FloodQueue2.Enqueue(test);
                }

                while (this.FloodQueue2.Count > 0)
                {
                    this.FloodQueue.Enqueue(this.FloodQueue2.Dequeue());
                }

                yield break;
            }

            // --- 2. Process the flooding queue, use specific number of cells (idk 100) ---
            int i = 0;

            Flooded.Clear();

            while (ToFlood.Count > 0 && i < CellsPerFrame)
            {
                HexCell cell = ToFlood.Dequeue();

                //if water level is higher and cell is a border cell.
                cell.FloodCell();

                Flooded.Add(cell);

                // hexMesh.ReTriangulateCell(
                //    cell, hexGrid.HexSize, hexGrid.HexOrientation);

                foreach (HexCell neighbor in _hexGrid.GetCellNeighbours(cell))
                {
                    if (neighbor.IsFlooded())
                        continue;
                    if (this.ToFlood.Contains(neighbor) || this.FloodQueue.Contains(neighbor))
                        continue;
                    if (neighbor.CellPosition.y <= this.SeaLevel)
                        ToFlood.Enqueue(neighbor);
                    else
                        this.FloodQueue.Enqueue(neighbor);
                }
                i++;
            }

            // --- 3. Retriangulate what has been flooded ---
            // hexMesh.ReTriangulateCells(Flooded.ToArray(), hexGrid.HexSize, hexGrid.HexOrientation);

            yield return null;
        }
    }
}
