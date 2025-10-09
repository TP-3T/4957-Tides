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
    /// Mainly for debugging.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{{ cellPosition: {CellPosition}, cellCubeCoordinates: {CellCubeCoordinates}, cellColor: {CellColor} }}";
    }
}
