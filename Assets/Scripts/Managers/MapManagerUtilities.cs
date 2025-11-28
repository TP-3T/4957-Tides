using System;
using System.Collections;
using System.Collections.Generic;
using TTT.DataClasses.HexData;
using TTT.DataClasses.TileFeatures;
using TTT.GameEvents;
using TTT.Hex;
using UnityEngine;

namespace TTT.Managers
{
    /// <summary>
    /// Logic-y stuff for the grid shall live here.
    /// </summary>
    public partial class MapManager
    {
        [SerializeField]
        private GameEvent DestroyingFeatureEvent;

        private void FloodCell(ref HexCell hc)
        {
            int index = GetCellIndexFromCubeCoordinates(hc.CellCubeCoordinates);
            hc.Flooded = true;
            // hc.CellColor = Color.blue;
            HexCells[index] = hc;

            // Remove any building on this flooded cell
            RaiseDestroyingFeatureEvent(hc.CellPosition);
        }

        /// <summary>
        /// Removes a building from a cell when it gets flooded.
        /// </summary>
        private void RaiseDestroyingFeatureEvent(Vector3 cellPosition)
        {
            BuildingFeatureArgs bfArgs =
                ScriptableObject.CreateInstance<BuildingFeatureArgs>();
            if (bfArgs.FeatureType != null)
            {
                bfArgs.Location = cellPosition;

                DestroyingFeatureEvent.Raise(bfArgs);
            }
        }

        private void SetCellCenterVertex(HexCell hc, int cv)
        {
            int index = GetCellIndexFromCubeCoordinates(hc.CellCubeCoordinates);
            hc.CenterVertexIndex = cv;
            HexCells[index] = hc;
        }

        private CubeCoordinates GetCubeCoordinatesFromPosition(Vector3 position)
        {
            CubeCoordinatesF cf = HexMath.PositionToCubeF(
                MapManager.HexSize,
                position,
                MapManager.HexOrientation
            );
            CubeCoordinates cc = HexMath.RoundCube(cf);
            return cc;
        }

        private int GetCellIndexFromCubeCoordinates(CubeCoordinates hc)
        {
            if (MapManager.HexOrientation == HexOrientation.pointyTop)
            {
                return (
                    (Mathf.RoundToInt(hc.r / 2) + hc.q)
                    + (hc.r * _hexGridWidth.Value)
                );
            }
            else
            {
                throw new Exception(
                    "This math has lazily not been implemented yet, get on it you git!" // po: TODO
                );
            }
        }

        private int GetCellIndexFromPosition(Vector3 position)
        {
            CubeCoordinates cc = GetCubeCoordinatesFromPosition(position);
            int ci = GetCellIndexFromCubeCoordinates(cc);
            return ci;
        }

        private HexCell? GetCellFromCubeCoordinates(
            CubeCoordinates hc,
            out bool success
        )
        {
            if (MapManager.HexOrientation == HexOrientation.pointyTop)
            {
                int cubeCoordinateIndex = GetCellIndexFromCubeCoordinates(hc);
                // Debug.Log(cubeCoordinateIndex);
                if (
                    cubeCoordinateIndex < HexCells.Count
                    && cubeCoordinateIndex >= 0
                    && ((Mathf.RoundToInt(hc.r / 2) + hc.q) >= 0)
                ) // Prevent row wrap-around, enforce row constraint (r component / 2 + q component zeros out if this is a valid cell)
                {
                    success = true;
                    return HexCells[cubeCoordinateIndex];
                }
                else
                {
                    success = false;
                    return null;
                }
            }
            else
            {
                success = false;
                throw new Exception("The math for this is not implemented");
            }
        }

        public HexCell? GetCellFromPosition(Vector3 position, out bool success)
        {
            CubeCoordinates hc = GetCubeCoordinatesFromPosition(position);
            return GetCellFromCubeCoordinates(hc, out success);
        }

        public List<HexCell> GetCellNeighbours(HexCell c)
        {
            List<HexCell> neighbours = new List<HexCell>();

            foreach (CubeCoordinates dir in MapManager.NeighbourDirections)
            {
                bool success;
                CubeCoordinates neighborPos = c.CellCubeCoordinates + dir;
                HexCell? n = GetCellFromCubeCoordinates(
                    neighborPos,
                    out success
                );
                // Debug.Log($"{success}, {neighborPos}, {dir}");

                if (success)
                    neighbours.Add((HexCell)n);
            }

            return neighbours;
        }

        /// <summary>
        /// Simulate rising on a per turn basis, not per frame.
        /// </summary>
        public IEnumerator RaiseSea()
        {
            SeaLevel.Value += RisingRate.Value;   // Function for this perchance

            while (true)
            {
                if (ToFlood.Count == 0)
                {
                    while (FloodQueue.Count > 0)
                    {
                        HexCell test = FloodQueue.Dequeue();

                        if (
                            test.CellPosition.y
                            <= (SeaLevel.Value + RisingRate.Value)
                        )
                            ToFlood.Enqueue(test);
                        else
                            AboveSeaLevelQueue.Enqueue(test);
                    }

                    while (AboveSeaLevelQueue.Count > 0)
                        FloodQueue.Enqueue(AboveSeaLevelQueue.Dequeue());

                    Debug.Log("Flood fill cycle complete");

                    break;
                }

                List<HexCell> flooded = new();
                int cellCount = 0;
                while (ToFlood.Count > 0 && cellCount < CellsPerFrame)
                {
                    HexCell cell = ToFlood.Dequeue();
                    FloodCell(ref cell);
                    flooded.Add(cell);

                    List<HexCell> neighbours = GetCellNeighbours(cell);
                    foreach (HexCell neighbor in neighbours)
                    {
                        if (neighbor.Flooded)
                            continue;
                        if (
                            ToFlood.Contains(neighbor)
                            || FloodQueue.Contains(neighbor)
                        )
                            continue;
                        if (neighbor.CellPosition.y <= SeaLevel.Value)
                            ToFlood.Enqueue(neighbor);
                        else
                            FloodQueue.Enqueue(neighbor);
                    }

                    cellCount++;
                }

                TriangulateSeaMeshClientRpc(flooded.ToArray());

                yield return null;
            }

            onFloodEnded.Raise();
            //says unreachable but it is
        }
    }
}
