using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TTT.DataClasses.HexData;
using TTT.DataClasses.States;
using TTT.DataClasses.Terrain;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Unity.Collections;
using Unity.VisualScripting.IonicZip;

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

        #region:Events

        [SerializeField]
        private GameEvent _mapLoadFinishEvent;

        [SerializeField]
        private TerrainDictionary _allowedTerrains;

        [SerializeField]
        private GameEvent onFloodEnded;

        [SerializeField]
        private GameEvent _onFeatureBuild;
        [SerializeField]
        private GameEvent _onFeaturePlace;

        [SerializeField]
        private GameEvent _onFeatureDestroy;

        #endregion

        [SerializeField]
        private PlayerStats _playerStats;
        private MapData _gameMapData;
        private const int CellsPerFrame = 25;
        private float _hexMaxHeight = 0;
        private Dictionary<string, FeatureType> _featureTypesByUniqueId = new();

        private NetworkVariable<int> _hexGridWidth = new();
        private NetworkVariable<int> _hexGridHeight = new();
        private NetworkVariable<ulong> _hexMeshId = new();
        private NetworkVariable<ulong> _seaMeshId = new();
        public NetworkList<HexCell> HexCells = new(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public NetworkVariable<float> SeaLevel = new(0.0f);
        public NetworkVariable<float> RisingRate = new(1.0f);

        public Queue<HexCell> ToFlood = new();
        public Queue<HexCell> FloodQueue = new();
        public Queue<HexCell> AboveSeaLevelQueue = new();
        public bool DrawDebugLabels;

        private LineRenderer lineRenderer;

        private List<(Vector3 position, string featureId)> _pendingFeatures =
            new();

        private NetworkList<FeatureNet> _pendingFeaturesGoated = new();

        private bool _featuresLoaded = false;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            StartCoroutine(LoadFeatureTypes());
        }

        private void CacheFeatureType(FeatureType featureType)
        {
            if (
                featureType != null
                && !string.IsNullOrEmpty(featureType.UniqueID)
            )
            {
                _featureTypesByUniqueId[featureType.UniqueID] = featureType;
            }
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback +=
                OnClientConnect;
        }

        #region:RPC Definitions

        [Rpc(SendTo.ClientsAndHost)]
        private void DestroyFeatureClientRpc(Vector3 cellPosition)
        {
            BuildingFeatureArgs bfArgs = ScriptableObject.CreateInstance<BuildingFeatureArgs>();
            bfArgs.Location = cellPosition;

            _onFeatureDestroy.Raise(bfArgs);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void BuildFeatureClientRpc(ulong builder, FixedString32Bytes featureId, Vector3 cellPosition)
        {
            BuildingFeatureArgs bfArgs = ScriptableObject.CreateInstance<BuildingFeatureArgs>();

            _featureTypesByUniqueId.TryGetValue(
                featureId.ToString(),
                out FeatureType featureType
            );

            bfArgs.Location = cellPosition;
            bfArgs.FeatureType = featureType;
            bfArgs.OwnedByClient = (builder == NetworkManager.Singleton.LocalClientId);

            _onFeatureBuild.Raise(bfArgs);
        }

        [ClientRpc]
        private void TriangulateHexMeshClientRpc()
        {
            if (
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                    _hexMeshId.Value,
                    out NetworkObject hexMeshNetworkObject
                )
            )
            {
                HexMesh hexMeshInstance =
                    hexMeshNetworkObject.GetComponent<HexMesh>(); // Get the hex mesh in the scene

                hexMeshInstance.Triangulate(
                    HexCells,
                    MapManager.HexSize,
                    MapManager.HexOrientation
                );
            }

            // Spawn features asynchronously across multiple frames
            StartCoroutine(SpawnPendingFeaturesAsync());
        }

        [ClientRpc]
        private void TriangulateHexMeshClientRpc(HexCell[] cells)
        {
            if (
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                    _hexMeshId.Value,
                    out NetworkObject hexMeshNetworkObject
                )
            )
            {
                HexMesh hexMeshInstance =
                    hexMeshNetworkObject.GetComponent<HexMesh>();

                hexMeshInstance.ReTriangulateCells(
                    cells,
                    MapManager.HexSize,
                    MapManager.HexOrientation
                );
            }
        }

        [ClientRpc]
        private void TriangulateSeaMeshClientRpc()
        {
            if (
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                    _seaMeshId.Value,
                    out NetworkObject seaMeshNetworkObject
                )
            )
            {
                SeaMesh seaMeshInstance =
                    seaMeshNetworkObject.GetComponent<SeaMesh>();

                seaMeshInstance.Triangulate(
                    HexCells,
                    SeaLevel.Value,
                    MapManager.HexSize,
                    MapManager.HexOrientation
                );
            }
        }

        [ClientRpc]
        private void TriangulateSeaMeshClientRpc(HexCell cell)
        {
            if (
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                    _seaMeshId.Value,
                    out NetworkObject seaMeshNetworkObject
                )
            )
            {
                SeaMesh seaMeshInstance =
                    seaMeshNetworkObject.GetComponent<SeaMesh>();

                seaMeshInstance.TriangulateCell(
                    cell,
                    MapManager.HexSize,
                    MapManager.HexOrientation
                );
            }
        }

        [ClientRpc]
        private void TriangulateSeaMeshClientRpc(HexCell[] cells)
        {
            if (
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                    _seaMeshId.Value,
                    out NetworkObject seaMeshNetworkObject
                )
            )
            {
                SeaMesh seaMeshInstance =
                    seaMeshNetworkObject.GetComponent<SeaMesh>();

                seaMeshInstance.TriangulateCells(
                    cells,
                    SeaLevel.Value,
                    MapManager.HexSize,
                    MapManager.HexOrientation
                );
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void StartRaiseSeaServerRpc()
        {
            StopAllCoroutines();
            StartCoroutine(RaiseSea());
        }

        #endregion

        #region:SCRBOJECT Handlers

        public void OnMapMeshClicked(Vector3 point, Color newColor)
        {
            int index = GetCellIndexFromPosition(point);
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
            _playerStats.SetSelectedTile(hc);
        }

        public void OnNewMap(UnityEngine.Object eventArgs)
        {
            NewMapEventArgs args = eventArgs as NewMapEventArgs;

            // WO: Deserializer / Serializer class for game data will eventually do the job of this routine
            // Maybe...
            try
            {
                _pendingFeatures.Clear();

                // Deserialized data (cringe)
                _gameMapData = JsonConvert.DeserializeObject<MapData>(
                    args.DataFile.text
                );

                int width = _gameMapData.MapTile.Count;
                int height = _gameMapData.MapTile["0"].Count;

                _hexGridWidth.Value = width;
                _hexGridHeight.Value = height;

                HexCell[] hexCells = new HexCell[width * height];

                foreach (var xGroup in _gameMapData.MapTile)
                {
                    int x = int.Parse(xGroup.Key);

                    foreach (var zGroup in xGroup.Value)
                    {
                        int z = int.Parse(zGroup.Key);
                        TileData tileData = zGroup.Value;

                        if (tileData.Elevation < 0)
                        {
                            tileData.Elevation = 0;
                        }

                        if (tileData.Elevation > _hexMaxHeight)
                        {
                            _hexMaxHeight = tileData.Elevation;
                        }

                        OffsetCoordinates offset = new(x, z);

                        Vector3 hexCenter = HexMath.GetHexCenter(
                            HexSize,
                            tileData.Elevation + 1,
                            offset,
                            HexOrientation
                        );

                        CubeCoordinates cubeCoords = HexMath.OddOffsetToCube(
                            offset,
                            HexOrientation
                        );

                        Color cellColor = _allowedTerrains
                            .Get(tileData.TileType)
                            .Color;

                        HexCell hexCell = new HexCell()
                        {
                            CellCubeCoordinates = cubeCoords,
                            CellPosition = hexCenter,
                            CellColor = cellColor,
                            TerrainTypeId = tileData.TileType,
                        };

                        int index = x + z * width;

                        hexCells[index] = hexCell;

                        // po: queue features for spawning after mesh is created
                        if (!string.IsNullOrEmpty(tileData.Feature))
                        {
                            var n = new FeatureNet();
                            n.FeatureId = tileData.Feature;
                            n.FeaturePosition = hexCell.CellPosition;
                            _pendingFeaturesGoated.Add(n);
                        }
                    }
                }

                HexCells.Clear();

                foreach (HexCell hc in hexCells)
                {
                    HexCells.Add(hc);
                }

                SeaLevel.Value = _gameMapData.WorldState.SeaLevel;

                // Load pollution from map data into PlayerStats
                if (_playerStats != null)
                    _playerStats.LoadPollutionFromMapData(_gameMapData.WorldState.Pollution);

                ToFlood.Clear();
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

        public void OnFlood(UnityEngine.Object _)
        {
            Debug.Log("Flood Event Triggered - MapManager line 261");

            StartRaiseSeaServerRpc();
        }

        public void OnFeaturePlace(UnityEngine.Object args)
        {
            if (args is not BuildingFeatureArgs)
            {
                Debug.LogError("[MapManager] could not build feature");
                return;
            }
            BuildingFeatureArgs eventArgs = args as BuildingFeatureArgs;

            BuildFeatureClientRpc(
                NetworkManager.Singleton.LocalClientId,
                eventArgs.FeatureType.UniqueID,
                eventArgs.Location
            );
        }

        #endregion
    }
}
