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

    private AssetReference _hexGridMeshAsset = new("P_HexMesh");

    private AssetReference _seaMeshAsset = new("P_SeaMesh");

    [SerializeField]
    private TextAsset _jsonMap;

    [SerializeField]
    private GameEvent MapLoadFinishEvent;

    private HexGrid _hexGrid;

    private Sea _sea;                           // Something that will be relevant in the future

    private MapData _gameMapData;

    private HexCell[,] _hexCells;

    private NetworkList<HexCell> _hexCellsNetwork = new NetworkList<HexCell>();

    private NetworkVariable<ulong> _hexMeshId = new NetworkVariable<ulong>();

    private HexMesh _hexMesh;

    private const int CellsPerFrame = 100;

    public float SeaLevel;

    public float RisingRate;

    public Queue<HexCell> ToFlood;

    public Queue<HexCell> FloodQueue;

    public Queue<HexCell> FloodQueue2;

    public List<HexCell> Flooded;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        RisingRate = 1.0f;
        SeaLevel = 0.0f;
        ToFlood = new();
        FloodQueue = new();
        FloodQueue2 = new();
        Flooded = new();

        _hexGrid = new();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
    }

    public void OnNewMap(UnityEngine.Object eventArgs)
    {
        NewMapEventArgs args = eventArgs as NewMapEventArgs;

        if (IsServer)
        {
            try
            {
                // Deserialized data (cringe)
                _gameMapData = JsonUtility.FromJson<MapData>(args.DataFile.text);

                if (_gameMapData == null)
                {
                    throw new Exception($"{args.DataFile.name} is not a valid TTT Map object.");
                }

                _hexGrid.BuildMap(_gameMapData, out _hexCells, _hexCellsNetwork);

                StartCoroutine(SpawnMapObjects());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                MapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = false });
            }
        }
    }

    public void OnClientConnect(ulong clientId)
    {
        Debug.Log($"Hello mr {clientId}");

        // How do I trangulate from this location?
        NetworkObject hexMeshInstance = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_hexMeshId.Value];
        HexMesh hexMesh = hexMeshInstance.GetComponent<HexMesh>();
        hexMesh.Triangulate(_hexCellsNetwork, HexGrid.HexSize, HexGrid.HexOrientation);
    }

    public void OnNextTurn(UnityEngine.Object eventArgs)
    {
        Debug.Log("The next turn event has been raised.");
    }

    private void SpawnGrid(GameObject hm)
    {
        // Get reference to HexMesh prefab
        GameObject hexMeshGameObject = Instantiate(hm);
        HexMesh hexMesh = hexMeshGameObject.GetComponent<HexMesh>();

        // Instance HexMesh prefab based off of the build data
        hexMesh.GetComponent<NetworkObject>().Spawn();
        hexMesh.Triangulate(_hexCellsNetwork, HexGrid.HexSize, HexGrid.HexOrientation);

        Debug.Log($"HEX MESH ID {hexMesh.NetworkObjectId}");

        _hexMeshId.Value = hexMesh.NetworkObjectId;                // Save mesh object id
    }

    // private void SpawnSea(SeaMesh sm)
    // {
    //     // Get reference to SeaMesh prefab
    //     var inst = Instantiate(sm);

    //     // Instance SeaMesh prefab based off of the build data
    //     inst.GetComponent<NetworkObject>().Spawn();
    // }

    private IEnumerator SpawnMapObjects()
    {
        yield return AssetLoader<GameObject>.Load(_hexGridMeshAsset, SpawnGrid);

        MapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = true });
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

                foreach (HexCell neighbor in _hexGrid.GetCellNeighbours(_hexCells, cell))
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
            _hexMesh.ReTriangulateCells(Flooded.ToArray(), HexGrid.HexSize, HexGrid.HexOrientation);

            yield return null;
        }
    }

    [Rpc(SendTo.Everyone)]
    public void StartRaiseSeaRpc()
    {
        StopAllCoroutines();
        StartCoroutine(RaiseSea());
    }

    [Rpc(SendTo.Everyone)]
    public void TestRpc()
    {
        Debug.Log("This is a test.");
    }
}
