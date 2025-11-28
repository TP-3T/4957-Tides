using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TTT.DataClasses.HexData;
using TTT.DataClasses.Terrain;
using TTT.DataClasses.TileFeatures;
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

        [SerializeField]
        private GameEvent onFloodEnded;

        private NetworkVariable<int> _hexGridWidth = new();
        private NetworkVariable<int> _hexGridHeight = new();
        private NetworkVariable<ulong> _hexMeshId = new();
        private NetworkVariable<ulong> _seaMeshId = new();
        private MapData _gameMapData;
        private const int CellsPerFrame = 25;

        [SerializeField]
        private GameEvent BuildingFeatureEvent;

        private Dictionary<string, FeatureType> _featureTypesByUniqueId = new();

        public NetworkList<HexCell> HexCells = new(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public Queue<HexCell> ToFlood = new();
        public Queue<HexCell> FloodQueue = new();
        public Queue<HexCell> AboveSeaLevelQueue = new();
        public NetworkVariable<float> SeaLevel = new(0.0f);
        public NetworkVariable<float> RisingRate = new(1.0f);
        public bool DrawDebugLabels;

        private LineRenderer lineRenderer;

        private List<(Vector3 position, string featureId)> _pendingFeatures =
            new();
        private bool _featuresLoaded = false;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            StartCoroutine(LoadFeatureTypes());
        }

        private IEnumerator LoadFeatureTypes()
        {
            yield return AssetLoader<FeatureType>.LoadGroup(
                "building",
                CacheFeatureType
            );
            _featuresLoaded = true;
            Debug.Log($"Loaded {_featureTypesByUniqueId.Count} feature types");
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

        private void SpawnPendingFeatures()
        {
            // po: the idea is that
            // OnNewMap() parses json
            // then on each tile with feature != null
            //   adds (position, featureId) to pending features,
            // then spawnMapObjects() creates mesh prefabs
            // then TriangulateWhatever() makes visual mesh
            // then SpawnPendingFeatures()
            //    looks up feature id in feature types by unique id
            //    creates building feature args
            //    calls FeatureBuilder.OnLoadingMapFeature(args)
            //       where BuildAt() instantiates prefab

            if (!_featuresLoaded)
            {
                Debug.LogWarning("feature types didn't load");
            }

            int spawnedCount = 0;

            foreach (var (position, featureId) in _pendingFeatures)
            {
                if (
                    _featureTypesByUniqueId.TryGetValue(
                        featureId,
                        out FeatureType featureType
                    )
                )
                {
                    var args =
                        ScriptableObject.CreateInstance<BuildingFeatureArgs>();
                    args.Location = position;
                    args.FeatureType = featureType;
                    args.OwnedByClient = false;
                    Debug.Log(args);
                    BuildingFeatureEvent.Raise(args);
                    spawnedCount++;
                }
                else
                {
                    Debug.LogWarning(
                        $"skipped unknown feature '{featureId}' at {position}"
                    );
                }
            }

            if (spawnedCount > 0)
            {
                Debug.Log($"Spawned {spawnedCount} features from map data:");
            }

            _pendingFeatures.Clear();
        }

        private IEnumerator SpawnPendingFeaturesAsync()
        {
            if (!_featuresLoaded)
            {
                Debug.LogWarning("feature types didn't load");
                yield break;
            }

            int spawnedCount = 0;
            int spawnsPerFrame = 50; // Spawn 50 buildings per frame for smooth-ish loading
            Dictionary<string, int> featureTypeCounts =
                new Dictionary<string, int>();

            Debug.Log(
                $"Starting async spawn of {_pendingFeatures.Count} features..."
            );

            foreach (var (position, featureId) in _pendingFeatures)
            {
                if (
                    _featureTypesByUniqueId.TryGetValue(
                        featureId,
                        out FeatureType featureType
                    )
                )
                {
                    var args =
                        ScriptableObject.CreateInstance<BuildingFeatureArgs>();
                    args.Location = position;
                    args.FeatureType = featureType;
                    BuildingFeatureEvent.Raise(args);
                    spawnedCount++;

                    // Track counts by type
                    if (!featureTypeCounts.ContainsKey(featureId))
                        featureTypeCounts[featureId] = 0;
                    featureTypeCounts[featureId]++;

                    // Yield every X spawns to maintain framerate
                    //Kinda doesn't work :/
                    if (spawnedCount % spawnsPerFrame == 0)
                    {
                        yield return null; // Wait one frame
                    }
                }
                else
                {
                    Debug.LogWarning(
                        $"skipped unknown feature '{featureId}' at {position}"
                    ); //THis basically never happens but I put this here just in case :/
                }
            }

            if (spawnedCount > 0)
            {
                Debug.Log($"Finished spawning {spawnedCount} features:");
                foreach (var kvp in featureTypeCounts)
                {
                    Debug.Log($"  {kvp.Key}: {kvp.Value}");
                }
            }

            _pendingFeatures.Clear();
            _mapLoadFinishEvent.Raise(
                new NewMapFinishedEventArgs() { WasSuccessful = true }
            );
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartRaiseSeaServerRpc()
        {
            StopAllCoroutines();
            StartCoroutine(RaiseSea());
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnMapMeshClickedServerRpc(Vector3 point, Color newColor)
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
                            _pendingFeatures.Add((hexCenter, tileData.Feature));
                        }
                    }
                }

                HexCells.Clear();

                foreach (HexCell hc in hexCells)
                {
                    HexCells.Add(hc);
                }

                SeaLevel.Value = _gameMapData.WorldState.SeaLevel;

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

            OnMapMeshClickedServerRpc(args.ClickedPoint, args.PlayerColor);
        }

        public void OnFlood(UnityEngine.Object _)
        {
            Debug.Log("Flood Event Triggered - MapManager line 261");

            StartRaiseSeaServerRpc();
        }
    }
}
