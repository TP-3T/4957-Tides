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
using Newtonsoft.Json;
using TTT.DataClasses.HexData;

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
                // Deserialized data (cringe)
                _gameMapData = JsonConvert.DeserializeObject<MapData>(args.DataFile.text);
                
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
                        
                        if(tileData.Elevation < 0)
                        {
                            tileData.Elevation = 0;
                        }

                        OffsetCoordinates offset = new(x, z);

                        Vector3 hexCenter = 
                            HexMath.GetHexCenter(
                                HexSize,
                                tileData.Elevation + 1,
                                offset,
                                HexOrientation
                            );

                        CubeCoordinates cubeCoords = 
                            HexMath.OddOffsetToCube(
                                offset,
                                HexOrientation
                            );

                        Color cellColor = 
                            _allowedTerrains.Get(tileData.TileType).Color;
                        
                        HexCell hexCell = new HexCell()
                        {
                            CellCubeCoordinates = cubeCoords,
                            CellPosition = hexCenter,
                            CellColor = cellColor,
                            TerrainTypeId = tileData.TileType,
                        };
                        
                        int index = x + z * width;

                        hexCells[index] = hexCell;
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

        public void OnFlood(UnityEngine.Object eventArgs)
        {
            FloodEventArgs args = eventArgs as FloodEventArgs;

            Debug.Log("Flood Event Triggered - MapManager line 261");

            StartRaiseSeaServerRpc();
        }
    }
}
