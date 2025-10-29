using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TTT.Hex;
using NUnit.Framework.Interfaces;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using TTT.Helpers;

public class Sea : GenericNetworkSingleton<Sea>
{
    public float SeaLevel;
    //private float seaLevelOffset = 12.66f;
    public float RisingRate;
    public Queue<HexCell> ToFlood;
    public Queue<HexCell> FloodQueue;
    public Queue<HexCell> FloodQueue2;
    public List<HexCell> Flooded;

    private HexGrid hexGrid;
    private HexMesh hexMesh;
    private const int CellsPerFrame = 100;

    /// <summary>
    /// Unity build in method, gets once at the beginning.
    /// </summary>
    void Start()
    {
        this.RisingRate = 1.0f;
        this.SeaLevel = 0.0f;
        this.transform.position = new Vector3(0, this.SeaLevel, 0);
        this.ToFlood = new();
        this.FloodQueue = new();
        this.FloodQueue2 = new();
        this.Flooded = new();
        this.hexGrid = FindFirstObjectByType<HexGrid>();
        this.hexMesh = hexGrid.GetComponentInChildren<HexMesh>();

        // Start flooding from the first cell
        // FloodFill(hexGrid.GetCellFromCubeCoordinates(new CubeCoordinates(0, 0)));
        // StartRaiseSea();

        ToFlood.Enqueue(this.hexGrid.GetCellFromCubeCoordinates(new CubeCoordinates(0, 0)));
    }

    public void StartRaiseSea()
    {
        StopAllCoroutines();
        StartCoroutine(RaiseSea());
    }

    /// <summary>
    /// Simulate rising on a per turn basis, not per frame.
    /// </summary>
    public IEnumerator RaiseSea()
    {
        this.SeaLevel += this.RisingRate;

        while (true)
        {
            // --- 1. Flood queue is empty, go through neighbours that were not eligable for flooding and see if they will be ---
            if (ToFlood.Count == 0)
            {
                Debug.Log("Flood fill cycle complete");

                while (this.FloodQueue.Count > 0)
                {
                    HexCell test = this.FloodQueue.Dequeue();

                    if (test.CellPosition.y <= (this.SeaLevel + this.RisingRate))
                        ToFlood.Enqueue(test);
                    else
                        this.FloodQueue2.Enqueue(test);
                }

                while (this.FloodQueue2.Count > 0)
                {
                    this.FloodQueue.Enqueue(this.FloodQueue2.Dequeue());
                }

                yield break;
            }

            // --- 2. Process the flooding queue, use specific number of cells (idk 100) ---
            int i = 0;

            Flooded.Clear();

            while (ToFlood.Count > 0 && i < CellsPerFrame)
            {
                HexCell cell = ToFlood.Dequeue();

                //if water level is higher and cell is a border cell.
                cell.FloodCell();

                Flooded.Add(cell);

                // hexMesh.ReTriangulateCell(
                //    cell, hexGrid.HexSize, hexGrid.HexOrientation);

                foreach (HexCell neighbor in hexGrid.GetCellNeighbours(cell))
                {
                    if (neighbor.IsFlooded())
                        continue;
                    if (this.ToFlood.Contains(neighbor) || this.FloodQueue.Contains(neighbor))
                        continue;
                    if (neighbor.CellPosition.y <= this.SeaLevel)
                        ToFlood.Enqueue(neighbor);
                    else
                        this.FloodQueue.Enqueue(neighbor);
                }
                i++;
            }

            // --- 3. Retriangulate what has been flooded ---
            hexMesh.ReTriangulateCells(
                Flooded.ToArray(), hexGrid.HexSize, hexGrid.HexOrientation);

            yield return null;
        }
    }

    [ClientRpc]
    public void HandleNextTurnClickedClientRpc()
    {
        StartRaiseSea();
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleNextTurnClickedServerRpc()
    {
        HandleNextTurnClickedClientRpc();
    }
}
