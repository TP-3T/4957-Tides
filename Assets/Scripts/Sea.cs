using System;
using System.Collections.Generic;
using Hex;
using Unity.Netcode;
using UnityEngine;

public class Sea : NetworkBehaviour
{
    [SerializeField]
    public float SeaLevel = 0.0f;

    //private float seaLevelOffset = 12.66f;
    [SerializeField]
    public float RisingRate;

    public HexCell[,] HexCells;

    /// <summary>
    /// Unity build in method, gets once at the beginning.
    /// </summary>
    void Start()
    {
        this.RisingRate = 0.0f;
        this.SeaLevel = 0.0f;
        this.transform.position = new Vector3(0, this.SeaLevel, 0);
        if (HexCells == null)
        {
            HexCells = GameObject.FindFirstObjectByType<HexGrid>().HexCells;
        }
        else
        {
            Debug.Log("HexCells already assigned to Sea script.");
        }
    }

    /// <summary>
    /// Unity build in method, gets called every frame.
    /// </summary>
    void Update()
    {
        this.SeaLevel += Time.deltaTime * this.RisingRate;
        //this.transform.position = new Vector3(0, this.SeaLevel, 0);
        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                if (HexCells[x, z] == null)
                {
                    Debug.LogWarning($"HexCells[{x}, {z}] is null.");
                    continue;
                }
                if (HexCells[x, z].IsFlooded())
                    continue;
                if (
                    HexCells[x, z].CellCubeCoordinates.q == 0
                    || HexCells[x, z].CellCubeCoordinates.q == 19
                )
                {
                    //HexCells[x, z].FloodCell();
                    FloodFill(HexCells[x, z]);
                }
            }
        }
    }

    /// <summary>
    /// Flood-fill algorithm to flood all applicable cells.
    /// <param name="startCell">The cell to start the flood-fill from.</param>
    /// </summary>
    public void FloodFill(HexCell startCell)
    {
        if (startCell.CellPosition.y < this.SeaLevel)
        {
            Queue<HexCell> q = new Queue<HexCell>();
            q.Enqueue(startCell);
            while (q.Count > 0)
            {
                HexCell cell = q.Dequeue();
                //if water level is higher and cell is a border cell.
                cell.FloodCell();
                foreach (HexCell neighbor in cell.GetNeighbors(HexCells))
                {
                    if (!neighbor.IsFlooded() && neighbor.CellPosition.y < this.SeaLevel)
                    {
                        q.Enqueue(neighbor);
                    }
                }
            }
            GameObject
                .FindFirstObjectByType<HexMesh>()
                .Triangulate(
                    HexCells,
                    3,
                    GameObject.FindFirstObjectByType<HexGrid>().HexOrientation
                );
        }
    }
}
