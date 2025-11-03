using System;
using System.Collections.Generic;
using System.IO;
using TTT.DataClasses.HexData;
using TTT.Features;
using TTT.GameEvents;
using TTT.Terrain;
using Unity.Netcode;
using UnityEngine;

namespace TTT.Hex
{
    public class HexGrid
    {
        // Key: Player's Network Client ID Value: The HexCell the player has selected
        // private Dictionary<ulong, HexCell> playerSelections = new Dictionary<ulong, HexCell>();

        // Key: The HexCell object Value: The original Color of the cell (before ANY player selected it)
        // private Dictionary<HexCell, Color?> cellOriginalColors = new Dictionary<HexCell, Color?>();

        // This so can detect collision with ray casts just to this object
        // public static readonly int GRID_LAYER_MASK = 1 << 10;
        // public static LayerMask layerMask = GRID_LAYER_MASK;

        private static readonly CubeCoordinates[] neighbourDirections =
        {
            new CubeCoordinates(1, 0, -1),
            new CubeCoordinates(-1, 0, 1),
            new CubeCoordinates(0, 1, -1),
            new CubeCoordinates(0, -1, 1),
            new CubeCoordinates(1, -1, 0),
            new CubeCoordinates(-1, 1, 0),
        };

        private static HexCell badCell;

        public static readonly float HexSize = 3.0f;

        public static readonly HexOrientation HexOrientation = HexOrientation.pointyTop;

        public int Padding { get; private set; }

        public MapData GameMapData { get; set; }

        private HexCell[,] _hexCells;

        public ref HexCell GetCellFromCubeCoordinates(
            HexCell[,] cells, CubeCoordinates hc, out bool success)
        {
            if (HexGrid.HexOrientation == HexOrientation.pointyTop)
            {
                if (
                    (hc.q + Padding) < 0
                    || (hc.r) < 0
                    || (hc.q + Padding) >= (GameMapData.Width + Padding)
                    || (hc.r) >= (GameMapData.Height)
                )
                {
                    success = false;
                    return ref HexGrid.badCell;
                }
                success = true;
                return ref cells[hc.r, hc.q + Padding];
            }
            else
            {
                if (
                    (hc.r + Padding) < 0
                    || (hc.q) < 0
                    || (hc.r + Padding) >= (GameMapData.Height + Padding)
                    || (hc.q) >= (GameMapData.Width)
                )
                {
                    success = false;
                    return ref HexGrid.badCell;
                }
                success = true;
                return ref cells[hc.r + Padding, hc.q];
            }
        }

        /// <summary>
        /// Gets a HexCell from the HexCells array (takes care of adding paddings to indices).
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public ref HexCell GetCellFromPosition(
            HexCell[,] cells, Vector3 position, out bool success)
        {
            CubeCoordinatesF hcf = HexMath.PositionToCubeF(HexSize, position, HexOrientation);
            CubeCoordinates hc = HexMath.RoundCube(hcf);

            return ref GetCellFromCubeCoordinates(cells, hc, out success);
        }

        /// <summary>
        /// Returns the neighbours of a HexCell.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public List<HexCell> GetCellNeighbours(
            HexCell[,] cells, HexCell c)
        {
            List<HexCell> neighbours = new List<HexCell>();

            foreach (CubeCoordinates dir in neighbourDirections)
            {
                bool success;
                CubeCoordinates neighborPos = c.CellCubeCoordinates + dir;
                HexCell n = GetCellFromCubeCoordinates(cells, neighborPos, out success);

                if (success)
                    neighbours.Add(n);
            }

            return neighbours;
        }


        /// <summary>
        /// Given MapData deserialized data, construct a 2D grid indexable with cube coordinates.
        /// 
        /// This involves determining the padding of the rows or columns, dependent on orientation.
        /// </summary>
        /// <param name="md"></param>
        /// <param name="cells"></param>
        public void BuildMap(NetworkList<HexCell> cells, int width, int height)
        {
            // Padding =
            //     ((width & 1) == 0 ? width / 2 : (height + 1) / 2)
            //     - 1;

            // if (HexOrientation == HexOrientation.pointyTop)
            // {
            //     _hexCells = new HexCell[md.Height, md.Width + Padding];
            // }
            // else
            // {
            //     _hexCells = new HexCell[md.Height + Padding, md.Width];
            // }

            // // Add HexCell prefabs according to mapdata
            // foreach (MapTileData mapTileData in md.MapTilesData)
            // {
                

            //     // string terrainUid = mapTileData.TileType;
            //     // hexCell.TerrainType = AllowedTerrains.Get(terrainUid);

            //     if (HexOrientation == HexOrientation.pointyTop)
            //     {
            //         _hexCells[hexCubeCoordinates.r, hexCubeCoordinates.q + Padding] = hexCell;
            //     }
            //     else
            //     {
            //         _hexCells[hexCubeCoordinates.r + Padding, hexCubeCoordinates.q] = hexCell;
            //     }
            // }

            // This may prove useful at some point, I need to turn this pattern into something that applies
            // to a flattened 1D array instead of a 2D array.
            // I will think about this (WO)
        }

        // /// <summary>
        // /// Builds the map HexCells form JSON and initiates mesh triangulation.
        // /// </summary>
        // public void BuildMap()
        // {
        //     if (hexMesh == null)
        //     {
        //         throw new NullReferenceException("A hex mesh is required to create the hex grid!");
        //     }
        //     if (GameMapData == null)
        //     {
        //         throw new NullReferenceException(
        //             "No Game Map Data was loaded when the map attempted to be built."
        //         );
        //     }

        //     padding =
        //         ((GameMapData.Width & 1) == 0 ? GameMapData.Width / 2 : (GameMapData.Width + 1) / 2)
        //         - 1;

        //     if (HexOrientation == HexOrientation.pointyTop)
        //     {
        //         HexCells = new HexCell[GameMapData.Height, GameMapData.Width + padding];
        //     }
        //     else
        //     {
        //         HexCells = new HexCell[GameMapData.Height + padding, GameMapData.Width];
        //     }

        //     // Add HexCell prefabs according to mapdata
        //     foreach (var mapTileData in GameMapData.MapTilesData)
        //     {
        //         if (mapTileData.Height < 0)
        //             mapTileData.Height = 0;

        //         Vector3 hexCenter = HexMath.GetHexCenter(
        //             HexSize,
        //             mapTileData.Height + 1,
        //             mapTileData.OffsetCoordinates,
        //             HexOrientation
        //         );

        //         CubeCoordinates hexCubeCoordinates = HexMath.OddOffsetToCube(
        //             mapTileData.OffsetCoordinates,
        //             HexOrientation
        //         );

        //         // HexCell hexCell = Instantiate(
        //         //     HexCell,
        //         //     hexCenter,
        //         //     Quaternion.identity,
        //         //     this.transform
        //         // );

        //         // hexCell.CellPosition = hexCenter;
        //         // hexCell.CellCubeCoordinates = hexCubeCoordinates;
        //         // hexCell.MapTileData = mapTileData;

        //         // string terrainUid = mapTileData.TileType;
        //         // hexCell.TerrainType = AllowedTerrains.Get(terrainUid);

        //         // if (HexOrientation == HexOrientation.pointyTop)
        //         // {
        //         //     HexCells[hexCubeCoordinates.r, hexCubeCoordinates.q + padding] = hexCell;
        //         // }
        //         // else
        //         // {
        //         //     HexCells[hexCubeCoordinates.r + padding, hexCubeCoordinates.q] = hexCell;
        //         // }
        //     }

        //     hexMesh.Triangulate(HexCells, HexSize, HexOrientation);
        // }

        // --- HexGrid.cs: Replace existing ApplyColorToMeshClientRpc with this ---

        // [ClientRpc]
        // private void UpdateCellVisualsClientRpc(Vector3 cellPosition, Color colorToApply)
        // {
        //     HexCell hc = GetCellFromPosition(cellPosition);

        //     // Apply the color dictated by the server.
        //     hc.CellColor = colorToApply;

        //     // Force a re-render of this specific cell's mesh on the client.
        //     if (hexMesh != null)
        //     {
        //         hexMesh.ReTriangulateCell(hc, HexSize, HexOrientation);
        //     }
        // }

        // [ServerRpc(RequireOwnership = false)]
        // public void HandlePlayerClickServerRpc(
        //     Vector3 playerClickPoint,
        //     Color playerColor,
        //     float desiredCellHeight,
        //     // This allows the server to automatically get the player's unique ID
        //     ServerRpcParams rpcParams = default
        // )
        // {
        //     ulong clientId = rpcParams.Receive.SenderClientId;
        //     HexCell newCell = GetCellFromPosition(playerClickPoint);

        //     // Get the currently selected cell for THIS player.
        //     playerSelections.TryGetValue(clientId, out HexCell currentlySelectedCell);

        //     // --- STEP 1: DESELECTION LOGIC (Revert the old selection) ---
        //     if (currentlySelectedCell != null && currentlySelectedCell != newCell)
        //     {
        //         // 1. Get the original color to revert to.
        //         cellOriginalColors.TryGetValue(currentlySelectedCell, out Color? originalColor);
        //         Color colorToRevert = originalColor ?? currentlySelectedCell.TerrainType.Color;

        //         // 2. Tell ALL clients to revert the old cell's color.
        //         UpdateCellVisualsClientRpc(currentlySelectedCell.CellPosition, colorToRevert);

        //         // 3. Remove the old selection state from the server's tracking.
        //         playerSelections.Remove(clientId);
        //         cellOriginalColors.Remove(currentlySelectedCell);
        //     }

        //     // --- STEP 2: SELECTION LOGIC (Highlight the new selection) ---

        //     // Select the new cell only if it's different from the current selection.
        //     if (currentlySelectedCell != newCell)
        //     {
        //         // 1. Store the new cell's ORIGINAL color before changing it.
        //         Color originalColorToStore = newCell.CellColor ?? newCell.TerrainType.Color;

        //         // 2. Update the selection state on the server.
        //         playerSelections[clientId] = newCell;
        //         cellOriginalColors[newCell] = originalColorToStore;

        //         // 3. Tell ALL clients to update the new cell visually with the player's color.
        //         UpdateCellVisualsClientRpc(newCell.CellPosition, playerColor);
        //     }
        // }

        // public HexCell GetSelectedCell()
        // {
        //     ulong ownClientId = NetworkManager.Singleton.LocalClientId;
        //     return playerSelections[ownClientId];
        // }

        // public void OnBuilding(UnityEngine.Object eventArgs)
        // {
        //     if (eventArgs is not FeatureType featureType)
        //     {
        //         return;
        //     }

        //     HexCell cell = GetSelectedCell();
        //     if (cell == null)
        //     {
        //         Debug.LogWarning("Tried to build without a cell selected.");
        //         return;
        //     }

        //     cell.BuildFeature(featureType);
        // }

        // public void OnDestroyingFeature(UnityEngine.Object eventArgs)
        // {
        //     HexCell cell = GetSelectedCell();
        //     if (cell == null)
        //     {
        //         Debug.LogWarning("Tried to destroy without a cell selected.");
        //         return;
        //     }

        //     cell.DestroyFeature();
        // }
    }
}
