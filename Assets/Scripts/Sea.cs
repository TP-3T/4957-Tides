using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

public class Sea : NetworkBehaviour
{
    [SerializeField] public float SeaLevel = 5.0f;
    //private float seaLevelOffset = 12.66f;
    [SerializeField] public float RisingRate;

    public HexCell[,] HexCells;
    public List<HexCell> CheckMe;

    /// <summary>
    /// Unity build in method, gets once at the beginning.
    /// </summary>
    void Start()
    {
        this.RisingRate = 0.1f;
        this.SeaLevel = 1.0f;
        this.transform.position = new Vector3(0, this.SeaLevel, 0);
        this.CheckMe = new List<HexCell>();
        if (HexCells == null)
        {
            HexCells = GameObject.FindFirstObjectByType<HexGrid>().HexCells;
        }
        else
        {
            Debug.Log("HexCells already assigned to Sea script.");
        }

        FloodFill(GameObject.FindFirstObjectByType<HexGrid>().GetCellFromCubeCoordinates(new CubeCoordinates(0,0)));

        // foreach (HexCell a in HexCells)
        // {
        //     if (a == null) { continue; }

        //     if (a.IsFlooded()) { continue; }

        //     // Debug.Log(a.CellPosition.y);
        //     // Debug.Log(this.SeaLevel);
        //     // Debug.Log(a.CellPosition.y <= this.SeaLevel);

        //     if (a.CellCubeCoordinates.q == 0
        //         && a.CellCubeCoordinates.r == 0
        //         && a.CellPosition.y <= this.SeaLevel) // Just start w/ first
        //     {
        //         Debug.Log("Time to flood");
        //         FloodFill(a);
        //     }
        //     // Debug.Log("'la");
        // }
    }

    /// <summary>
    /// Unity build in method, gets called every frame.
    /// </summary>
    void Update()
    {
        this.SeaLevel += Time.deltaTime * this.RisingRate;
        // Debug.Log(this.SeaLevel);

        Queue<HexCell> floodQueue = new Queue<HexCell>();

        foreach (HexCell c in CheckMe)
        {
            if (c.CellPosition.y <= this.SeaLevel)
            {
                floodQueue.Enqueue(c);
            }
        }

        foreach (HexCell c in floodQueue)
        {
            CheckMe.Remove(c);
        }

        while (floodQueue.Count > 0)
        {
            FloodFill(floodQueue.Dequeue());
        }
    }

        // for (int x = 0; x < 20; x++)
        //     {
        //         for (int z = 0; z < 20; z++)
        //         {
        //             if (HexCells[x, z] == null)
        //             {
        //                 // Debug.LogWarning($"HexCells[{x}, {z}] is null.");
        //                 continue;
        //             }
        //             if (HexCells[x, z].IsFlooded()) continue;
        //             if (HexCells[x, z].CellCubeCoordinates.q == 0
        //             || HexCells[x, z].CellCubeCoordinates.q == 19)
        //             {
        //                 //HexCells[x, z].FloodCell();
        //                 FloodFill(HexCells[x, z]);
        //             }
        //         }
        //     }

    /// <summary>
    /// Flood-fill algorithm to flood all applicable cells.
    /// <param name="startCell">The cell to start the flood-fill from.</param>
    /// </summary>
    public void FloodFill(HexCell startCell)
    {
        Queue<HexCell> q = new Queue<HexCell>();
        q.Enqueue(startCell);
        while (q.Count > 0)
        {
            HexCell cell = q.Dequeue();

            //if water level is higher and cell is a border cell.
            cell.FloodCell();
            GameObject.FindFirstObjectByType<HexMesh>()
                .TriangulateCell(cell, 3,
                GameObject.FindFirstObjectByType<HexGrid>().HexOrientation);

            List<HexCell> test = cell.GetNeighbors(HexCells);

            foreach (HexCell neighbor in test)
            {
                if (neighbor.IsFlooded())
                    continue;

                if (neighbor.CellPosition.y <= this.SeaLevel)
                    q.Enqueue(neighbor);
                else
                    CheckMe.Add(neighbor);
                    
            }
        }
    }
}
