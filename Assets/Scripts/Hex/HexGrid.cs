using System.Collections.Generic;
using CodiceApp.EventTracking.Plastic;
using TTT.DataTypes;
using TTT.Features;
using TTT.GameEvents;
using TTT.Terrain;
using Unity.Netcode;
using UnityEngine;

namespace TTT.Hex
{
    public class HexGrid : NetworkBehaviour
    {
        // Key: Player's Network Client ID Value: The HexCell the player has selected
        private Dictionary<ulong, HexCell> playerSelections = new Dictionary<ulong, HexCell>();

        // Key: The HexCell object Value: The original Color of the cell (before ANY player selected it)
        private Dictionary<HexCell, Color?> cellOriginalColors = new Dictionary<HexCell, Color?>();

        public static readonly int GRID_LAYER_MASK = 1 << 10;

        private static readonly CubeCoordinates[] neighbourDirections =
        {
            new CubeCoordinates(1, 0, -1),
            new CubeCoordinates(-1, 0, 1),
            new CubeCoordinates(0, 1, -1),
            new CubeCoordinates(0, -1, 1),
            new CubeCoordinates(1, -1, 0),
            new CubeCoordinates(-1, 1, 0),
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
                if (
                    (coords.q + padding) < 0
                    || (coords.r) < 0
                    || (coords.q + padding) >= (GameMapData.Height + padding)
                    || (coords.r) >= (GameMapData.Width)
                )
                {
                    return null;
                }
                return HexCells[coords.r, coords.q + padding];
            }
            else
            {
                if (
                    (coords.r + padding) < 0
                    || (coords.q) < 0
                    || (coords.r + padding) >= (GameMapData.Height + padding)
                    || (coords.q) >= (GameMapData.Width)
                )
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
            CubeCoordinatesF hcf = HexMath.PositionToCubeF(HexSize, position, HexOrientation);
            CubeCoordinates hc = HexMath.RoundCube(hcf);

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

            padding =
                ((GameMapData.Width & 1) == 0 ? GameMapData.Width / 2 : (GameMapData.Width + 1) / 2)
                - 1;

            if (HexOrientation == HexOrientation.pointyTop)
            {
                HexCells = new HexCell[GameMapData.Height, GameMapData.Width + padding];
            }
            else
            {
                HexCells = new HexCell[GameMapData.Height + padding, GameMapData.Width];
            }

            // Add HexCell prefabs according to mapdata
            foreach (var mapTileData in GameMapData.MapTilesData)
            {
                Vector3 hexCenter = HexMath.GetHexCenter(
                    HexSize,
                    mapTileData.Height + 1,
                    mapTileData.OffsetCoordinates,
                    HexOrientation
                );

                CubeCoordinates hexCubeCoordinates = HexMath.OddOffsetToCube(
                    mapTileData.OffsetCoordinates,
                    HexOrientation
                );

                HexCell hexCell = Instantiate(
                    HexCell,
                    hexCenter,
                    Quaternion.identity,
                    this.transform
                );

                hexCell.CellPosition = hexCenter;
                hexCell.CellCubeCoordinates = hexCubeCoordinates;
                hexCell.MapTileData = mapTileData;

                string terrainUid = mapTileData.TileType;
                hexCell.TerrainType = AllowedTerrains.Get(terrainUid);

                if (HexOrientation == HexOrientation.pointyTop)
                {
                    HexCells[hexCubeCoordinates.r, hexCubeCoordinates.q + padding] = hexCell;
                }
                else
                {
                    HexCells[hexCubeCoordinates.r + padding, hexCubeCoordinates.q] = hexCell;
                }
            }

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

        // --- HexGrid.cs: Replace existing ApplyColorToMeshClientRpc with this ---

        [ClientRpc]
        private void UpdateCellVisualsClientRpc(Vector3 cellPosition, Color colorToApply)
        {
            HexCell hc = GetCellFromPosition(cellPosition);

            // Apply the color dictated by the server.
            hc.CellColor = colorToApply;

            // Force a re-render of this specific cell's mesh on the client.
            if (hexMesh != null)
            {
                hexMesh.ReTriangulateCell(hc, HexSize, HexOrientation);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void HandlePlayerClickServerRpc(
            Vector3 playerClickPoint,
            Color playerColor,
            float desiredCellHeight,
            // This allows the server to automatically get the player's unique ID
            ServerRpcParams rpcParams = default
        )
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            HexCell newCell = GetCellFromPosition(playerClickPoint);

            // Get the currently selected cell for THIS player.
            playerSelections.TryGetValue(clientId, out HexCell currentlySelectedCell);

            // --- STEP 1: DESELECTION LOGIC (Revert the old selection) ---
            if (currentlySelectedCell != null && currentlySelectedCell != newCell)
            {
                // 1. Get the original color to revert to.
                cellOriginalColors.TryGetValue(currentlySelectedCell, out Color? originalColor);
                Color colorToRevert = originalColor ?? currentlySelectedCell.TerrainType.Color;

                // 2. Tell ALL clients to revert the old cell's color.
                UpdateCellVisualsClientRpc(currentlySelectedCell.CellPosition, colorToRevert);

                // 3. Remove the old selection state from the server's tracking.
                playerSelections.Remove(clientId);
                cellOriginalColors.Remove(currentlySelectedCell);
            }

            // --- STEP 2: SELECTION LOGIC (Highlight the new selection) ---

            // Select the new cell only if it's different from the current selection.
            if (currentlySelectedCell != newCell)
            {
                // 1. Store the new cell's ORIGINAL color before changing it.
                Color originalColorToStore = newCell.CellColor ?? newCell.TerrainType.Color;

                // 2. Update the selection state on the server.
                playerSelections[clientId] = newCell;
                cellOriginalColors[newCell] = originalColorToStore;

                // 3. Tell ALL clients to update the new cell visually with the player's color.
                UpdateCellVisualsClientRpc(newCell.CellPosition, playerColor);
            }
        }

        public HexCell GetSelectedCell()
        {
            ulong ownClientId = NetworkManager.Singleton.LocalClientId;
            return playerSelections[ownClientId];
        }

        public void OnBuilding(Object eventArgs)
        {
            if (eventArgs is not FeatureType featureType)
            {
                return;
            }

            HexCell cell = GetSelectedCell();
            if (cell == null)
            {
                Debug.LogWarning("Tried to build without a cell selected.");
                return;
            }

            cell.BuildFeature(featureType);
        }

        public void OnDestroyingFeature(Object eventArgs)
        {
            HexCell cell = GetSelectedCell();
            if (cell == null)
            {
                Debug.LogWarning("Tried to destroy without a cell selected.");
                return;
            }

            cell.DestroyFeature();
        }
    }
}
