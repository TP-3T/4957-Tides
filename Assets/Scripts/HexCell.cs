using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] public CubeCoordinates CellCubeCoordinates;
    [SerializeField] public Vector3 CellPosition;
    public MapTileData MapTileData;
}
