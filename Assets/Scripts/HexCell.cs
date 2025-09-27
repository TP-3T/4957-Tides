using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] public CubeCoordinates CellCubeCoordinates;
    [SerializeField] public Vector3 CellPosition;
    public MapTileData MapTileData;

    /// <summary>
    /// Mainly for debugging.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{{ cellPosition: {CellPosition}, cellCubeCoordinates: {CellCubeCoordinates} }}";
    }
}
