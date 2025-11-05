using System;
using System.Collections;
using System.Collections.Generic;
using TTT.DataClasses.HexData;
using TTT.Helpers;
using TTT.Hex;
using UnityEngine;

namespace TTT.Managers
{
    /// <summary>
    /// Logicy stuff for the grid shall live here.
    /// </summary>
    public partial class MapManager
    {
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
    }
}
