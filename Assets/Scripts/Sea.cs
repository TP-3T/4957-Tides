using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Sea : NetworkBehaviour
{
    [SerializeField] public float SeaLevel = 0.0f;
    private float seaLevelOffset = 12.66f;
    [SerializeField] public float RisingRate;

    public HexGrid grid;

    void Start()
    {
        this.RisingRate = 0.0f;
        this.SeaLevel = this.seaLevelOffset;
        this.transform.position = new Vector3(0, this.SeaLevel, 0);
        grid = GameObject.FindFirstObjectByType<HexGrid>();
    }

    void Update()
    {
        this.SeaLevel += Time.deltaTime * this.RisingRate;
        this.transform.position = new Vector3(0, this.SeaLevel, 0);
        foreach (HexCell cell in grid.HexCells)
        {
            if (cell.transform.position.y < this.SeaLevel - this.seaLevelOffset
            && (cell.CellCubeCoordinates.q == 0 || cell.CellCubeCoordinates.q == 20 || cell.CellCubeCoordinates.s == 0 || cell.CellCubeCoordinates.s == 20))
            {
                if (cell.IsFlooded()) continue;
                cell.FloodCell();
                FloodFill(cell);
            }
        }
    }

    public void FloodFill(HexCell startCell)
    {
        Queue<HexCell> q = new Queue<HexCell>();
        q.Enqueue(startCell);
        while (q.Count > 0)
        {
            HexCell cell = q.Dequeue();
            //if water level is higher and cell is a border cell.
            if (cell.transform.position.y < this.SeaLevel - seaLevelOffset)
            {
                cell.FloodCell();
                cell.floodedWithFloodFill = true;
                foreach (HexCell neighbor in cell.GetNeighbors(grid))
                {
                    if (!neighbor.IsFlooded() && neighbor.transform.position.y < this.SeaLevel - seaLevelOffset)
                    {
                        q.Enqueue(neighbor);
                    }
                }
            }
        }
    }
}
