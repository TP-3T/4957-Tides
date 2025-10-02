using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField] public CubeCoordinates CellCubeCoordinates;
    [SerializeField] public Vector3 CellPosition;
    public MapTileData MapTileData;
    public TerrainType TerrainType;
    [SerializeField] public bool flooded = false;
    [SerializeField] public bool floodedWithFloodFill = false;

    public void FloodCell()
    {
        this.flooded = true;
    }

    public bool IsFlooded()
    {
        return this.flooded;
    }

    public List<HexCell> GetNeighbors(HexGrid grid)
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
            Vector3 neighborPos = new Vector3(
                this.CellCubeCoordinates.q + dir.x,
                this.CellCubeCoordinates.r + dir.y,
                this.CellCubeCoordinates.s + dir.z
            );
            int index = (int)(neighborPos.x + neighborPos.z * 20);
            if (index <= 0 || index > grid.HexCells.Length) continue;
            HexCell neighbor = grid.HexCells[index];
            Debug.Log(index + " " + neighborPos + " " + neighbor);
            if (neighbor != null)
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }
}
