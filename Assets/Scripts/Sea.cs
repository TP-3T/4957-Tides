using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Sea : NetworkBehaviour
{
    [SerializeField] public float SeaLevel = 0.0f;
    private float seaLevelOffset = 12.66f;
    [SerializeField] public float RisingRate = 0f;

    public HexGrid grid;

    void Start()
    {
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
            if (cell.transform.position.y < this.SeaLevel - this.seaLevelOffset)
            {
                if (cell.getIsFlooded()) continue;
                cell.FloodCell();
            }
        }
    }

    private void floodFill(HexCell startCell)
    {
        //this will be implemented properly after the sea level is bound to the edges of the map
        Queue<HexCell> q = new Queue<HexCell>();
        q.Enqueue(startCell);
        while (q.Count > 0)
        {
            HexCell cell = q.Dequeue();
            if (cell.transform.position.y < this.SeaLevel - seaLevelOffset)
            {
                cell.FloodCell();
                // foreach (HexCell neighbor in cell.getNeighbors(cell))
                // {
                //     if (!neighbor.getIsFlooded())
                //     {
                //         q.Enqueue(neighbor);
                //     }
                // }
            }
        }
    }
}
