using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] public CubeCoordinates CellCoordinates;
    [SerializeField] public Vector3 CellPosition;
    public MapTileData MapTileData;
}
