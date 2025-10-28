using System.Collections.Generic;
using TTT.Helpers;
using TTT.Hex;
using Unity.Netcode;
using UnityEngine;

public class Sea : GenericNetworkSingleton<Sea>
{
    [SerializeField]
    public float SeaLevel;

    //private float seaLevelOffset = 12.66f;
    [SerializeField]
    public float RisingRate;

    public Queue<HexCell> Unflooded;
    public Queue<HexCell> Unflooded2;

    private HexGrid hexGrid;
    private HexMesh hexMesh;

    /// <summary>
    /// Unity build in method, gets once at the beginning.
    /// </summary>
    void Start()
    {
        this.RisingRate = 1.0f;
        this.SeaLevel = 1.0f;
        this.transform.position = new Vector3(0, this.SeaLevel, 0);
        this.Unflooded = new();
        this.Unflooded2 = new();
        //! CB: FirstFirstObjectByType is not a great way to get references.
        //! We should be loading assets or creating
        //! new gameobjects if we need them.
        this.hexGrid = FindFirstObjectByType<HexGrid>();
        this.hexMesh = hexGrid.GetComponentInChildren<HexMesh>();

        // Start flooding from the first cell
        FloodFill(hexGrid.GetCellFromCubeCoordinates(new CubeCoordinates(0, 0)));
    }

    /// <summary>
    /// Simulate rising on a per turn basis, not per frame.
    /// </summary>
    public void RaiseSea()
    {
        this.SeaLevel += this.RisingRate;

        while (this.Unflooded.Count > 0)
        {
            HexCell hc = this.Unflooded.Dequeue();
            if (hc.CellPosition.y <= this.SeaLevel)
            {
                FloodFill(hc);
            }
            else
            {
                Unflooded2.Enqueue(hc);
            }
        }

        while (this.Unflooded2.Count > 0)
        {
            Unflooded.Enqueue(Unflooded2.Dequeue());
        }
    }

    /// <summary>
    /// Flood-fill algorithm to flood all applicable cells.
    /// <param name="startCell">The cell to start the flood-fill from.</param>
    /// </summary>
    public void FloodFill(HexCell startCell)
    {
        Queue<HexCell> q = new();
        List<HexCell> flooded = new();

        q.Enqueue(startCell);

        while (q.Count > 0)
        {
            HexCell cell = q.Dequeue();

            //if water level is higher and cell is a border cell.
            cell.FloodCell();
            flooded.Add(cell);

            foreach (HexCell neighbor in hexGrid.GetCellNeighbours(cell))
            {
                if (neighbor.IsFlooded())
                    continue;

                if (q.Contains(neighbor) || this.Unflooded.Contains(neighbor))
                    continue;

                if (neighbor.CellPosition.y <= this.SeaLevel)
                    q.Enqueue(neighbor);
                else
                    this.Unflooded.Enqueue(neighbor);
            }
        }

        // Re-triangulate what has been flooded
        hexMesh.ReTriangulateCells(flooded.ToArray(), hexGrid.HexSize, hexGrid.HexOrientation);
    }

    [ClientRpc]
    public void HandleNextTurnClickedClientRpc()
    {
        RaiseSea();
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleNextTurnClickedServerRpc()
    {
        HandleNextTurnClickedClientRpc();
    }
}
