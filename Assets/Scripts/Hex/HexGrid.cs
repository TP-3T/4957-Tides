using System;
using System.Collections.Generic;
using Features;
using Terrain;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Hex
{       
    public class HexGrid : NetworkBehaviour
    {
        public static readonly int GRID_LAYER_MASK = 1 << 10;

        private static readonly CubeCoordinates[] neighbourDirections = {
            new CubeCoordinates(1, 0, -1),
            new CubeCoordinates(-1, 0, 1),
            new CubeCoordinates(0, 1, -1),
            new CubeCoordinates(0, -1, 1),
            new CubeCoordinates(1, -1, 0),
            new CubeCoordinates(-1, 1, 0)
        };

        public bool DrawGizmos;
        public bool DrawDebugLabels;
        public float HexSize;
        public HexOrientation HexOrientation;
        public HexCell HexCell;
        public TextAsset MapSource;
        public HexCell[,] HexCells;
        public MapData GameMapData;

        private HexMesh hexMesh;
        private int padding;


        [SerializeField]
        private TerrainDictionary AllowedTerrains;

        void InitializeGrid()
        {
            if (hexMesh == null)
                hexMesh = GetComponentInChildren<HexMesh>();
            if (hexMesh == null)
                Debug.LogError("HexMesh failed to retrieve component  from children.");
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            BuildandCreateGrid();
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        /// <summary>
        /// Applies the specified color to the HexMesh material on the local client.
        /// </summary>
        private void ApplyColorToMesh(Color colorToApply)
        {
            if (hexMesh != null && hexMesh.GetComponent<MeshRenderer>() != null)
            {
                hexMesh.GetComponent<MeshRenderer>().material.color = colorToApply;
            }
        }

        // ClientRpc to tell all clients to apply the new color received from the server.
        [ClientRpc]
        private void ApplyColorToMeshClientRpc(Color colorToApply)
        {
            Debug.Log($"Applying new mesh color: {colorToApply}");
            ApplyColorToMesh(colorToApply);
        }


        void BuildandCreateGrid()
        {
            InitializeGrid();
            ClearMap();
            BuildMap();
        }

        void OnValidate()
        {
            InitializeGrid();
        }

        /// <summary>
        /// Draws an outline around the top of each hex cell.
        /// </summary>
        void OnDrawGizmos()
        {
            if (!DrawGizmos)
                return;

            foreach (HexCell hexCell in HexCells)
            {
                Vector3[] hexCorners = HexMath.GetHexCorners(HexSize, HexOrientation);
                for (int s = 0; s < hexCorners.Length; s++)
                {
                    Gizmos.DrawLine(
                        transform.position + hexCell.CellPosition + hexCorners[s % 6],
                        transform.position + hexCell.CellPosition + hexCorners[(s + 1) % 6]
                    );
                }
            }
        }

        /// <summary>
        /// Loads map tile data from JSON.
        /// </summary>
        void LoadMapTilesData()
        {
            GameMapData = JsonUtility.FromJson<MapData>(MapSource.text);

            if (GameMapData == null)
                Debug.LogError("Game map JSON failed to decode.");
        }

        /// <summary>
        /// Retrieves a HexCell from the HexCells array given its cube coordinates.
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public HexCell GetCellFromCubeCoordinates(CubeCoordinates coords)
        {
            if (HexOrientation == HexOrientation.pointyTop)
            {
                if (    (coords.q + padding) < 0
                    ||  (coords.r) < 0
                    ||  (coords.q + padding) >= (GameMapData.Height + padding)
                    ||  (coords.r) >= (GameMapData.Width))
                {
                    return null;
                }
                return HexCells[coords.r, coords.q + padding];
            }
            else
            {
                if (    (coords.r + padding) < 0
                    ||  (coords.q) < 0
                    ||  (coords.r + padding) >= (GameMapData.Height + padding)
                    ||  (coords.q) >= (GameMapData.Width))
                {
                    return null;
                }
                return HexCells[coords.r + padding, coords.q];
            }
        }

        /// <summary>
        /// Gets a HexCell from the HexCells array (takes care of adding paddings to indicies).
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public HexCell GetCellFromPosition(Vector3 position)
        {
            CubeCoordinatesF hcf = HexMath.PositionToCubeF(
                HexSize, position, HexOrientation);
            CubeCoordinates hc = HexMath.RoundCube(hcf);

            // Debug.Log(hc);

            return GetCellFromCubeCoordinates(hc);
        }

        /// <summary>
        /// Returns the neighbours of a HexCell.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public List<HexCell> GetCellNeighbours(HexCell c)
        {
            List<HexCell> neighbours = new List<HexCell>();

            foreach (CubeCoordinates dir in neighbourDirections)
            {
                CubeCoordinates neighborPos = c.CellCubeCoordinates + dir;

                HexCell n = GetCellFromCubeCoordinates(neighborPos);

                if (n != null)
                    neighbours.Add(n);
            }

            return neighbours;
        }

        /// <summary>
        /// Builds the map HexCells form JSON and initiates mesh triangulation.
        /// </summary>
        public void BuildMap()
        {
            if (hexMesh == null)
            {
                Debug.LogError("Hex mesh is null");
                return;
            }

            LoadMapTilesData();

            // Create the grid storage
            // HexCells = new HexCell[GameMapData.Width * GameMapData.Height];

            padding = ((GameMapData.Width & 1) == 0
                ? GameMapData.Width / 2
                : (GameMapData.Width + 1) / 2) - 1;

            if (HexOrientation == HexOrientation.pointyTop)
            {
                HexCells = new HexCell[
                    GameMapData.Height, GameMapData.Width + padding];
            }
            else
            {
                HexCells = new HexCell[
                    GameMapData.Height + padding, GameMapData.Width];
            }

            // Debug.Log(padding);
            // Debug.Log(HexCells);

            // Add HexCell prefabs according to mapdata
            foreach (var mapTileData in GameMapData.MapTilesData)
            {
                Vector3 hexCenter = HexMath.GetHexCenter(
                    HexSize, mapTileData.Height + 1, mapTileData.OffsetCoordinates, HexOrientation);

                CubeCoordinates hexCubeCoordinates = HexMath.OddOffsetToCube(
                    mapTileData.OffsetCoordinates, HexOrientation);

                HexCell hexCell = Instantiate(
                    HexCell, hexCenter, Quaternion.identity, this.transform);

                hexCell.CellPosition = hexCenter;
                hexCell.CellCubeCoordinates = hexCubeCoordinates;
                hexCell.MapTileData = mapTileData;

                string terrainUid = mapTileData.TileType;
                hexCell.TerrainType = AllowedTerrains.Get(terrainUid);

                // Debug.Log(@$"
                // {hexCell.CellPosition}, The real position
                // {hexCell.CellCubeCoordinates}, The cube position: q,r,s
                // {hexCell.MapTileData.OffsetCoordinates}, Logical map position (col, row): x,z 

                // HexCells[i++] = hexCell;

                if (HexOrientation == HexOrientation.pointyTop)
                {
                    HexCells[
                        hexCubeCoordinates.r, hexCubeCoordinates.q + padding] = hexCell;
                }
                else
                {
                    HexCells[
                        hexCubeCoordinates.r + padding, hexCubeCoordinates.q] = hexCell;
                }
            }

            // Debug.Log(NewHexCells[2,3]);

            hexMesh.Triangulate(HexCells, HexSize, HexOrientation);
        }

        /// <summary>
        /// Destroys the hex cells and clears the mesh.
        /// 
        /// For development.
        /// </summary>
        public void ClearMap()
        {
            if (hexMesh == null)
                return;

            hexMesh.ClearMesh();

            if (HexCell == null || HexCells == null)
            {
                Debug.Log("Hex cell array reference lost");
                return;
            }

            foreach (var hexCell in HexCells)
            {
                if (hexCell == null)
                    continue;

                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(hexCell.gameObject);
                else
                    Destroy(hexCell.gameObject);
            }
        }

        [ClientRpc]
        private void ApplyColorToMeshClientRpc(
            Vector3 playerClickPoint, Color playerColor, float desiredCellHeight)
        {
            HexCell hc = GetCellFromPosition(playerClickPoint);
            hc.CellColor = playerColor;
            // hc.CellPosition.y = desiredCellHeight;

            // Debug.Log(hc.CellColor);
            // Debug.Log(hc);

            hexMesh.ReTriangulateCell(hc, HexSize, HexOrientation);
        }

        //Now accepts the playerColor passed from the PlayerController.
        [ServerRpc(RequireOwnership = false)]
        public void HandlePlayerClickServerRpc(
            Vector3 playerClickPoint, Color playerColor, float desiredCellHeight)
        {
            // Color nextColor;
            // Color currentColor = this.meshColor.Value;

            // if (currentColor == Color.red)
            // {
            //     nextColor = Color.blue;
            // }
            // else
            // {
            //     nextColor = Color.red;
            // }

            // The current logic changes the entire grid mesh color to the player's color.

            HexCell hc = GetCellFromPosition(playerClickPoint);
            hc.CellColor = playerColor;

            hexMesh.ReTriangulateCell(hc, HexSize, HexOrientation);

            ApplyColorToMeshClientRpc(
                playerClickPoint, playerColor, desiredCellHeight);

            ToggleFeature(hc); // ! Temporary test stuff (feel free to delete)
        }

        /// <summary>
        /// ! Temporary test stuff (feel free to delete)
        /// </summary>
        [SerializeField]
        private FeatureType placeholderFeatureType;
        private GameObject placeholderFeature;

        // <summary>
        /// ! Temporary test stuff (feel free to delete)
        /// </summary>
        private void ToggleFeature(HexCell hexCell)
        {
            if (hexCell.FeatureType != null)
            {
                hexCell.FeatureType = null;
                Destroy(placeholderFeature);
                return;
            }

            Vector3 cellPos = hexCell.CellPosition;
            Vector3 featurePos = new(cellPos.x, cellPos.y, cellPos.z);

            hexCell.FeatureType = placeholderFeatureType;

            Destroy(placeholderFeature);
            placeholderFeature = Instantiate(hexCell.FeatureType.Prefab);
            featurePos.y += 0.5f * placeholderFeature.transform.localScale.y;
            placeholderFeature.transform.position = featurePos;
        }
    }
}
