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
using Codice.Client.BaseCommands.Download;
using UnityEditor.PackageManager;

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

    public static readonly CubeCoordinates[] NeighbourDirections =
    {
        new CubeCoordinates(1, 0, -1),
        new CubeCoordinates(-1, 0, 1),
        new CubeCoordinates(0, 1, -1),
        new CubeCoordinates(0, -1, 1),
        new CubeCoordinates(1, -1, 0),
        new CubeCoordinates(-1, 1, 0),
    };
    public static readonly float HexSize = 3.0f;
    public static readonly HexOrientation HexOrientation = HexOrientation.pointyTop;

    private AssetReference _hexGridMeshAsset = new("P_HexMesh");
    private AssetReference _seaMeshAsset = new("P_SeaMesh");

    [SerializeField]
    private TextAsset _jsonMap;
    [SerializeField]
    private GameEvent _mapLoadFinishEvent;
    private MapData _gameMapData;
    private NetworkVariable<int> _hexGridWidth = new();
    private NetworkVariable<int> _hexGridHeight = new();
    private NetworkVariable<ulong> _hexMeshId = new();
    private const int CellsPerFrame = 100;

    public NetworkList<HexCell> HexCells = new(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool DrawDebugLabels;
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

    private IEnumerator SpawnMapObjects()
    {
        yield return AssetLoader<GameObject>.Load(_hexGridMeshAsset, SpawnGridMesh);
    }

    private void SpawnGridMesh(GameObject hm)
    {
        // Get reference to HexMesh prefab
        GameObject hexMeshGameObject = Instantiate(hm);
        HexMesh hexMeshInstance = hexMeshGameObject.GetComponent<HexMesh>();

        // Instance HexMesh prefab based off of the build data
        hexMeshInstance.GetComponent<NetworkObject>().Spawn();
        _hexMeshId.Value = hexMeshInstance.NetworkObjectId;

        TriangulateMeshInstance();
    }

    // private void SpawnSea(SeaMesh sm)
    // {
    //     // Get reference to SeaMesh prefab
    //     var inst = Instantiate(sm);

    //     // Instance SeaMesh prefab based off of the build data
    //     inst.GetComponent<NetworkObject>().Spawn();
    // }

    private void TriangulateMeshInstance()
    {
        NetworkObject hexMeshNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_hexMeshId.Value];
        HexMesh hexMeshInstance = hexMeshNetworkObject.GetComponent<HexMesh>();       // Get the hex mesh in the scene

        hexMeshInstance.Triangulate(HexCells, MapManager.HexSize, MapManager.HexOrientation);

        _mapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = true });
    }

    [ClientRpc]
    private void TriangulateMeshInstanceClientRpc(HexCell cell)
    {

        NetworkObject hexMeshNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_hexMeshId.Value];
        HexMesh hexMeshInstance = hexMeshNetworkObject.GetComponent<HexMesh>();

        hexMeshInstance.ReTriangulateCell(cell, MapManager.HexSize, MapManager.HexOrientation);
    }

    private void TriangulateMeshInstance(HexCell[] cells)
    {
    }

    private CubeCoordinates GetCubeCoordinatesFromPosition(Vector3 position)
    {
        CubeCoordinatesF cf = HexMath.PositionToCubeF(MapManager.HexSize, position, MapManager.HexOrientation);
        CubeCoordinates cc = HexMath.RoundCube(cf);
        return cc;
    }

    private int GetCellIndexFromCubeCoordinates(
        CubeCoordinates hc)
    {
        if (MapManager.HexOrientation == HexOrientation.pointyTop)
        {
            return ((Mathf.RoundToInt(hc.r / 2) + hc.q) + (hc.r * _gameMapData.Width));
        }
        else
        {
            throw new Exception("This math has lazily not been implemented yet, get on it you git!");
        }
    }

    private int GetCellIndexFromPosition(Vector3 position)
    {
        CubeCoordinates cc = GetCubeCoordinatesFromPosition(position);
        int ci = GetCellIndexFromCubeCoordinates(cc);
        return ci;
    }

    private HexCell GetCellFromCubeCoordinates(
        CubeCoordinates hc, out bool success)
    {
        if (MapManager.HexOrientation == HexOrientation.pointyTop)
        {
            int cubeCoordinateIndex = GetCellIndexFromCubeCoordinates(hc);
            success = true;
            return HexCells[cubeCoordinateIndex];
        }
        else
        {
            success = false;
            throw new Exception("The math for this is not implemented");
        }
    }

    public HexCell GetCellFromPosition(Vector3 position, out bool success)
    {
        CubeCoordinates hc = GetCubeCoordinatesFromPosition(position);
        return GetCellFromCubeCoordinates(hc, out success);
    }

    public List<HexCell> GetCellNeighbours(
        HexCell c)
    {
        List<HexCell> neighbours = new List<HexCell>();

        foreach (CubeCoordinates dir in MapManager.NeighbourDirections)
        {
            bool success;
            CubeCoordinates neighborPos = c.CellCubeCoordinates + dir;
            HexCell n = GetCellFromCubeCoordinates(neighborPos, out success);

            if (success)
                neighbours.Add(n);
        }

        return neighbours;
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

                foreach (HexCell neighbor in GetCellNeighbours(cell))
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

    [Rpc(SendTo.Everyone)]
    public void StartRaiseSeaRpc()
    {
        StopAllCoroutines();
        StartCoroutine(RaiseSea());
    }

    public void OnNewMap(UnityEngine.Object eventArgs)
    {
        NewMapEventArgs args = eventArgs as NewMapEventArgs;

        try
        {
            // Deserialized data (cringe)
            _gameMapData = JsonUtility.FromJson<MapData>(args.DataFile.text);
            _hexGridWidth.Value = _gameMapData.Width;
            _hexGridHeight.Value = _gameMapData.Height;

            HexCell[] hexCells = new HexCell[_gameMapData.MapTilesData.Count];

            foreach (MapTileData mapTileData in _gameMapData.MapTilesData)
            {
                if (mapTileData.Height < 0)
                    mapTileData.SetHeight(0);

                Vector3 hexCenter = HexMath.GetHexCenter(
                    MapManager.HexSize,
                    mapTileData.Height + 1,
                    mapTileData.OffsetCoordinates,
                    MapManager.HexOrientation
                ) + Vector3.zero;

                CubeCoordinates hc = HexMath.OddOffsetToCube(
                    mapTileData.OffsetCoordinates,
                    MapManager.HexOrientation
                );

                HexCell hexCell = new()
                {
                    CellCubeCoordinates = hc,
                    CellPosition = hexCenter,
                    CellColor = Color.white
                };

                int cubeCoordinateIndex = GetCellIndexFromCubeCoordinates(hc);

                hexCells[cubeCoordinateIndex] = hexCell;
            }

            foreach (HexCell hc in hexCells)
            {
                HexCells.Add(hc);
            }

            StartCoroutine(SpawnMapObjects());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            _mapLoadFinishEvent.Raise(new NewMapFinishedEventArgs() { WasSuccessful = false });
        }
    }

    public void OnClientConnect(ulong clientId)
    {
        Debug.Log($"Hello mr {clientId}");

        if (IsClient)
        {
            TriangulateMeshInstance();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnMapMeshCickedServerRpc(Vector3 point, Color newColor)
    {
        int index = GetCellIndexFromPosition(point);
        HexCell hc = HexCells[index];
        hc.CellColor = newColor;
        HexCells[index] = hc;

        TriangulateMeshInstanceClientRpc(HexCells[index]);
    }

    public void OnMapMeshClicked(UnityEngine.Object eventArgs)
    {
        MapMeshClickedEventArgs args = eventArgs as MapMeshClickedEventArgs;

        OnMapMeshCickedServerRpc(args.ClickedPoint, args.PlayerColor);
    }
}
