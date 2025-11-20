using System;
using System.Collections;
using System.Collections.Generic;
using TTT.DataClasses.HexData;
using TTT.DataClasses.Terrain;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TTT.Managers
{
    [RequireComponent(typeof(LineRenderer))]
    /// <summary>
    /// Business logic / game related logic and networking stuff shall live here.
    /// </summary>
    public partial class MapManager : GenericNetworkSingleton<MapManager>
    {
        public static readonly CubeCoordinates[] NeighbourDirections =
        {
            new(1, 0, -1),
            new(-1, 0, 1),
            new(0, 1, -1),
            new(0, -1, 1),
            new(1, -1, 0),
            new(-1, 1, 0),
        };
        public static readonly float HexSize = 3.0f;
        public static readonly HexOrientation HexOrientation =
            HexOrientation.pointyTop;

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
        private readonly AssetReference _hexGridMeshAsset = new("P_HexMesh");
        private readonly AssetReference _seaMeshAsset = new("P_SeaMesh");
        private MapData _gameMapData;
        private const int CellsPerFrame = 25;

        public NetworkList<HexCell> HexCells = new(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public Queue<HexCell> ToFlood = new();
        public Queue<HexCell> FloodQueue = new();
        public Queue<HexCell> FloodQueue2 = new();
        public NetworkVariable<float> SeaLevel = new(0.0f);
        public NetworkVariable<float> RisingRate = new(1.0f);
        public bool DrawDebugLabels;

        [SerializeField]
        public GameEvent _OnLastPlayerTurnEvent;

        private LineRenderer lineRenderer;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback +=
                OnClientConnect;
        }

        private IEnumerator SpawnMapObjects()
        {
            yield return AssetLoader<GameObject>.Load(
                new("P_HexMesh"),
                SpawnGridMesh
            );
            yield return AssetLoader<GameObject>.Load(
                new("P_SeaMesh"),
                SpawnSeaMesh
            );
        }

        private void SpawnGridMesh(GameObject hm)
        {
            // Get reference to HexMesh prefab
            GameObject hexMeshGameObject = Instantiate(hm);
            HexMesh hexMeshInstance = hexMeshGameObject.GetComponent<HexMesh>();

            // Instance HexMesh prefab based off of the build data
            hexMeshInstance.GetComponent<NetworkObject>().Spawn();
            hexMeshInstance.transform.position += new Vector3(
                0.0f,
                -0.01f,
                0.0f
            );
            _hexMeshId.Value = hexMeshInstance.NetworkObjectId;

            TriangulateHexMeshClientRpc();
        }

        private void SpawnSeaMesh(GameObject sm)
        {
            // Get reference to SeaMesh prefab
            GameObject seaMeshGameObject = Instantiate(sm);
            SeaMesh seaMeshInstance = seaMeshGameObject.GetComponent<SeaMesh>();

            // Instance SeaMesh prefab based off of the
            seaMeshInstance.GetComponent<NetworkObject>().Spawn();
            _seaMeshId.Value = seaMeshInstance.NetworkObjectId;

            TriangulateSeaMeshClientRpc(); // for the host, this should eventually not be necessary
        }

        [ClientRpc]
        private void TriangulateHexMeshClientRpc()
        {
            NetworkObject hexMeshNetworkObject = NetworkManager
                .Singleton
                .SpawnManager
                .SpawnedObjects[_hexMeshId.Value];
            HexMesh hexMeshInstance =
                hexMeshNetworkObject.GetComponent<HexMesh>(); // Get the hex mesh in the scene

            hexMeshInstance.Triangulate(
                HexCells,
                MapManager.HexSize,
                MapManager.HexOrientation
            );

            _mapLoadFinishEvent.Raise(
                new NewMapFinishedEventArgs() { WasSuccessful = true }
            );
        }

        [ClientRpc]
        private void TriangulateHexMeshClientRpc(HexCell cell)
        {
            NetworkObject hexMeshNetworkObject = NetworkManager
                .Singleton
                .SpawnManager
                .SpawnedObjects[_hexMeshId.Value];
            HexMesh hexMeshInstance =
                hexMeshNetworkObject.GetComponent<HexMesh>();

            hexMeshInstance.ReTriangulateCell(
                cell,
                MapManager.HexSize,
                MapManager.HexOrientation
            );
        }

        [ClientRpc]
        private void TriangulateHexMeshClientRpc(HexCell[] cells)
        {
            NetworkObject hexMeshNetworkObject = NetworkManager
                .Singleton
                .SpawnManager
                .SpawnedObjects[_hexMeshId.Value];
            HexMesh hexMeshInstance =
                hexMeshNetworkObject.GetComponent<HexMesh>();

            hexMeshInstance.ReTriangulateCells(
                cells,
                MapManager.HexSize,
                MapManager.HexOrientation
            );
        }

        [ClientRpc]
        private void TriangulateSeaMeshClientRpc()
        {
            NetworkObject seaMeshNetworkObject = NetworkManager
                .Singleton
                .SpawnManager
                .SpawnedObjects[_seaMeshId.Value];
            SeaMesh seaMeshInstance =
                seaMeshNetworkObject.GetComponent<SeaMesh>();

            seaMeshInstance.Triangulate(
                HexCells,
                SeaLevel.Value,
                MapManager.HexSize,
                MapManager.HexOrientation
            );
        }

        [ClientRpc]
        private void TriangulateSeaMeshClientRpc(HexCell cell)
        {
            NetworkObject seaMeshNetworkObject = NetworkManager
                .Singleton
                .SpawnManager
                .SpawnedObjects[_seaMeshId.Value];
            SeaMesh seaMeshInstance =
                seaMeshNetworkObject.GetComponent<SeaMesh>();

            seaMeshInstance.TriangulateCell(
                cell,
                MapManager.HexSize,
                MapManager.HexOrientation
            );
        }

        [ClientRpc]
        private void TriangulateSeaMeshClientRpc(HexCell[] cells)
        {
            NetworkObject seaMeshNetworkObject = NetworkManager
                .Singleton
                .SpawnManager
                .SpawnedObjects[_seaMeshId.Value];
            SeaMesh seaMeshInstance =
                seaMeshNetworkObject.GetComponent<SeaMesh>();

            seaMeshInstance.TriangulateCells(
                cells,
                SeaLevel.Value,
                MapManager.HexSize,
                MapManager.HexOrientation
            );
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartRaiseSeaServerRpc()
        {
            StopAllCoroutines();
            StartCoroutine(RaiseSea());
        }

        public void OnMapMeshClicked(Vector3 point, Color newColor)
        {
            // Debug.Log($"{point.x}, {point.y}, {point.z}");
            Debug.Log(GetCubeCoordinatesFromPosition(point));

            int index = GetCellIndexFromPosition(point);

            Debug.Log($"BIG INDEX: {index}");

            HexCell hc = HexCells[index];
            Vector3[] corners = HexMath.GetHexCorners(HexSize, HexOrientation);
            for (int i = 0; i < 6; i++)
            {
                lineRenderer.SetPosition(
                    i,
                    new Vector3(
                        hc.CellPosition.x + corners[i].x,
                        hc.CellPosition.y + 0.5f,
                        hc.CellPosition.z + corners[i].z
                    )
                );
            }
            lineRenderer.SetPosition(
                6,
                new Vector3(
                    hc.CellPosition.x + corners[0].x,
                    hc.CellPosition.y + 0.5f,
                    hc.CellPosition.z + corners[0].z
                )
            );
        }

        public void OnNewMap(UnityEngine.Object eventArgs)
        {
            NewMapEventArgs args = eventArgs as NewMapEventArgs;

            // WO: Deserializer / Serializer class for game data will eventually do the job of this routine
            // Maybe...
            try
            {
                // Deserialized data (cringe)
                _gameMapData = JsonUtility.FromJson<MapData>(
                    args.DataFile.text
                );
                _hexGridWidth.Value = _gameMapData.Width;
                _hexGridHeight.Value = _gameMapData.Height;

                HexCell[] hexCells = new HexCell[
                    _gameMapData.MapTilesData.Count
                ];

                foreach (MapTileData mapTileData in _gameMapData.MapTilesData)
                {
                    if (mapTileData.Height < 0)
                        mapTileData.SetHeight(0);

                    Vector3 hexCenter =
                        HexMath.GetHexCenter(
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
                        CellColor = cc,
                        TerrainTypeId = mapTileData.TileType,
                    };

                    int cubeCoordinateIndex = GetCellIndexFromCubeCoordinates(
                        hc
                    );

                    hexCells[cubeCoordinateIndex] = hexCell;
                }

                foreach (HexCell hc in hexCells)
                {
                    HexCells.Add(hc);
                }

                ToFlood.Enqueue(HexCells[0]); // There was some idea for this

                StartCoroutine(SpawnMapObjects());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _mapLoadFinishEvent.Raise(
                    new NewMapFinishedEventArgs() { WasSuccessful = false }
                );
            }
        }

        public void OnClientConnect(ulong clientId)
        {
            if (IsClient)
            {
                TriangulateHexMeshClientRpc();
                TriangulateSeaMeshClientRpc();
            }
        }

        public void OnMapMeshClicked(UnityEngine.Object eventArgs)
        {
            MapMeshClickedEventArgs args = eventArgs as MapMeshClickedEventArgs;

            OnMapMeshClicked(args.ClickedPoint, args.PlayerColor);
        }

        public void OnFlood(UnityEngine.Object eventArgs)
        {
            FloodEventArgs args = eventArgs as FloodEventArgs;

            Debug.Log("Flood Event Triggered - MapManager line 261");

            StartRaiseSeaServerRpc();
        }

        public void OnNextTurnClick(UnityEngine.Object eventArgs)
        {
            // needs current player info
            Debug.Log("Next Turn Clicked - MapManager line 262");

            // if not last players turn, switch the player context to the next player
            // next player turn event or something

            //if last player turn then
            _OnLastPlayerTurnEvent.Raise();
        }
    }
}
