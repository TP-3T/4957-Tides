using System;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using TTT.DataClasses.HexData;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using System.Collections.Generic;
using TTT.Terrain;

namespace TTT.Managers
{
    /// <summary>
    /// Business logic / game related logic and networking stuff shall live here.
    /// </summary>
    public partial class MapManager : GenericSingleton<MapManager>
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

        [SerializeField]
        private TextAsset _jsonMap;
        [SerializeField]
        private GameEvent _mapLoadFinishEvent;
        [SerializeField]
        private TerrainDictionary _allowedTerrains;

        private NetworkVariable<int> _hexGridWidth = new();
        private NetworkVariable<int> _hexGridHeight = new();
        private NetworkVariable<ulong> _hexMeshId = new();
        private NetworkVariable<ulong> _seaMeshId = new();
        private AssetReference _hexGridMeshAsset = new("P_HexMesh");
        private AssetReference _seaMeshAsset = new("P_SeaMesh");
        private MapData _gameMapData;
        private const int CellsPerFrame = 100;

        public NetworkList<HexCell> HexCells = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public Queue<HexCell> ToFlood = new();
        public Queue<HexCell> FloodQueue = new();
        public Queue<HexCell> FloodQueue2 = new();
        public NetworkVariable<float> SeaLevel = new(0.0f);
        public NetworkVariable<float> RisingRate = new(1.0f);
        public bool DrawDebugLabels;

        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        }

        private IEnumerator SpawnMapObjects()
        {
            yield return AssetLoader<GameObject>.Load(_hexGridMeshAsset, SpawnGridMesh);
            yield return AssetLoader<GameObject>.Load(_seaMeshAsset, SpawnSeaMesh);
        }

        private void SpawnGridMesh(GameObject hm)
        {
            // Get reference to HexMesh prefab
            GameObject hexMeshGameObject = Instantiate(hm);
            HexMesh hexMeshInstance = hexMeshGameObject.GetComponent<HexMesh>();

            // Instance HexMesh prefab based off of the build data
            hexMeshInstance.GetComponent<NetworkObject>().Spawn();
            _hexMeshId.Value = hexMeshInstance.NetworkObjectId;

            TriangulateMeshInstanceClientRpc();
        }

        private void SpawnSeaMesh(GameObject sm)
        {
            // Get reference to SeaMesh prefab
            GameObject seaMeshGameObject = Instantiate(sm);
            SeaMesh seaMeshInstance = seaMeshGameObject.GetComponent<SeaMesh>();

            // Instance SeaMesh prefab based off of the
            seaMeshInstance.GetComponent<NetworkObject>().Spawn();
            _seaMeshId.Value = seaMeshInstance.NetworkObjectId;
        }

        [ClientRpc]
        private void TriangulateMeshInstanceClientRpc()
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

        [ClientRpc]
        private void TriangulateMeshInstanceClientRpc(HexCell[] cells)
        {
            NetworkObject hexMeshNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_hexMeshId.Value];
            HexMesh hexMeshInstance = hexMeshNetworkObject.GetComponent<HexMesh>();

            hexMeshInstance.ReTriangulateCells(cells, MapManager.HexSize, MapManager.HexOrientation);
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartRaiseSeaServerRpc()
        {
            StopAllCoroutines();
            StartCoroutine(RaiseSea());
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

        public void OnNewMap(UnityEngine.Object eventArgs)
        {
            NewMapEventArgs args = eventArgs as NewMapEventArgs;

            // WO: Deserializer / Serializer class for game data will eventually do the job of this routine
            // Maybe...
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

                    Color cc = _allowedTerrains.Get(mapTileData.TileType).Color;

                    HexCell hexCell = new()
                    {
                        CellCubeCoordinates = hc,
                        CellPosition = hexCenter,
                        CellColor = cc
                    };

                    int cubeCoordinateIndex = GetCellIndexFromCubeCoordinates(hc);

                    hexCells[cubeCoordinateIndex] = hexCell;
                }

                foreach (HexCell hc in hexCells)
                {
                    HexCells.Add(hc);
                }

                ToFlood.Enqueue(HexCells[0]);       // There was some idea for this

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
            if (IsClient)
            {
                TriangulateMeshInstanceClientRpc();
            }
        }

        public void OnMapMeshClicked(UnityEngine.Object eventArgs)
        {
            MapMeshClickedEventArgs args = eventArgs as MapMeshClickedEventArgs;

            OnMapMeshCickedServerRpc(args.ClickedPoint, args.PlayerColor);
        }

        public void OnFlood(UnityEngine.Object eventArgs)
        {
            FloodEventArgs args = eventArgs as FloodEventArgs;

            StartRaiseSeaServerRpc();
        }
    }
}
