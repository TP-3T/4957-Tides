using UnityEngine;

public class HexCell : MonoBehaviour
{
    public CubeCoordinates CellCubeCoordinates;
    public Vector3 CellPosition;
    public Color? CellColor = null;
    public MapTileData MapTileData;
    public TerrainType TerrainType;
    public int CenterVertexIndex;

    /// <summary>
    /// Mainly for debugging.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{{ cellPosition: {CellPosition}, cellCubeCoordinates: {CellCubeCoordinates}, cellColor: {CellColor} }}";
    }
}
