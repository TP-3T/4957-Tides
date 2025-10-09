using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public CubeCoordinates CellCubeCoordinates;
    public Vector3 CellPosition;
    public Color? CellColor = null;
    public MapTileData MapTileData;
    public TerrainType TerrainType;
    [SerializeField] public bool flooded = false;
    public int CenterVertexIndex;

    /// <summary>
    /// Flood this cell
    /// </summary>
    public void FloodCell()
    {
        this.flooded = true;
        this.CellColor = Color.blue;
    }

    /// <summary>
    /// Get the flooded state of the cell.
    /// </summary>
    public bool IsFlooded()
    {
        return this.flooded;
    }

    /// <summary>
    /// Get the [upto] 6 neighbors of a cell.
    /// <param name="HexCells">The full hex cell array to search within.</param>
    /// </summary>
    public List<HexCell> GetNeighbors(HexCell[,] HexCells)
    {
        List<HexCell> neighbors = new List<HexCell>();
        Vector3[] directions;
        if (GameObject.FindFirstObjectByType<HexGrid>().HexOrientation == HexOrientation.pointyTop)
        {
            directions = new Vector3[]
            {
                new Vector3(1, 0, -1),    // East
                new Vector3(-1, 0, 1),   // West  
                new Vector3(0, 1, -1),    // Northeast
                new Vector3(0, -1, 1),   // Southwest
                new Vector3(1, -1, 0),   // Southeast
                new Vector3(-1, 1, 0)    // Northwest
            };
        }
        else
        {
            directions = new Vector3[]
            {
                new Vector3(0, -1, 1),    // North
                new Vector3(0, 1, -1),   // South
                new Vector3(1, -1, 0),    // Northeast  
                new Vector3(-1, 1, 0),   // Southwest
                new Vector3(1, 0, -1),   // Southeast
                new Vector3(-1, 0, 1)    // Northwest
            };
        }
        foreach (Vector3 dir in directions)
        {
            Vector3 neighborPos = new Vector3(
                this.CellCubeCoordinates.q + dir.x,
                this.CellCubeCoordinates.r + dir.y,
                this.CellCubeCoordinates.s + dir.z
            );
            foreach (HexCell cell in HexCells)
            {
                if (cell == null) continue;
                if (cell.CellCubeCoordinates.q == neighborPos.x &&
                    cell.CellCubeCoordinates.r == neighborPos.y &&
                    cell.CellCubeCoordinates.s == neighborPos.z)
                {
                    neighbors.Add(cell);
                    break;
                }
            }
        }
        return neighbors;
    }

        /// <summary>
    /// Mainly for debugging.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{{ cellPosition: {CellPosition}, cellCubeCoordinates: {CellCubeCoordinates}, cellColor: {CellColor} }}";
    }
}
