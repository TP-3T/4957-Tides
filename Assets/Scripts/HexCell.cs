using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] public CubeCoordinates CellCubeCoordinates;
    [SerializeField] public Vector3 CellPosition;
    public MapTileData MapTileData;
    [SerializeField]public bool isFlooded = false;

    public void FloodCell()
    {
        this.isFlooded = true;
    }

    public bool getIsFlooded() {
        return this.isFlooded;
    }
}
