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
        Vector3[] directions = new Vector3[]
        {
            new Vector3(1, 0, -1),    // East
            new Vector3(-1, 0, 1),   // West  
            new Vector3(0, 1, -1),    // Northeast
            new Vector3(0, -1, 1),   // Southwest
            new Vector3(1, -1, 0),   // Southeast
            new Vector3(-1, 1, 0)    // Northwest
        };

        foreach (Vector3 dir in directions)
        {
            CubeCoordinates neighborPos = new CubeCoordinates(
                this.CellCubeCoordinates.q + (int)dir.x,
                this.CellCubeCoordinates.r + (int)dir.y,
                this.CellCubeCoordinates.s + (int)dir.z
            );
            HexCell n = GameObject.FindFirstObjectByType<HexGrid>().GetCellFromCubeCoordinates(neighborPos);
            if (n != null)
                neighbors.Add(n);
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
