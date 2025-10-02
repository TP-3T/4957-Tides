using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] public CubeCoordinates CellCubeCoordinates;
    [SerializeField] public Vector3 CellPosition;
    public MapTileData MapTileData;
    [SerializeField] public bool flooded = false;

    public void FloodCell()
    {
        this.flooded = true;
    }

    public bool IsFlooded()
    {
        return this.flooded;
    }

    public List<HexCell> GetNeighbors()
    {
        List<HexCell> neighbors = new List<HexCell>();
        Vector3[] directions = new Vector3[]
        {
            new Vector3(1, 0, 0),   // Right
            new Vector3(-1, 0, 0),  // Left
            new Vector3(0, 0, 1),   // Up
            new Vector3(0, 0, -1),  // Down
            new Vector3(1, 0, 1),   // Up-Right
            new Vector3(-1, 0, -1)  // Down-Left
        };
        foreach (Vector3 dir in directions)
        {
            Vector3 neighborPos = this.transform.position + dir;
            //add this when push comes
            HexCell neighbor = HexMath.GetCellAtPosition(neighborPos);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }
}
