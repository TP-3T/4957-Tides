using System.Collections;
using System.Collections.Generic;
using TTT.DataClasses.HexData;
using TTT.GameEvents;
using TTT.Helpers;
using TTT.Hex;
using Unity.Netcode;
using UnityEngine;

// WO: Probably can delete this class
// We will write a seperate class (perchance) to manage the mesh for the sea

public class Sea : NetworkBehaviour
{

    //private float seaLevelOffset = 12.66f;


    //? CB: Should the Sea be modifying the Map directly?
    [SerializeField]
    private HexGrid hexGrid;

    [SerializeField]
    private HexMesh hexMesh;


    void Awake()
    {

    }

    /// <summary>
    /// Unity build in method, gets once at the beginning.
    /// </summary>
    // void Start()
    // {
    //     ToFlood.Enqueue(this.hexGrid.GetCellFromCubeCoordinates(new CubeCoordinates(0, 0)));
    // }

    // public void OnNewMapFinish(Object eventArgs)
    // {
    //     NewMapFinishedEventArgs args = eventArgs as NewMapFinishedEventArgs;
    //     if (args.WasSuccessful)
    //     {
    //     }
    // }

    public void SeedSea(HexCell[,] cells, HexGrid hg)
    {
        // ToFlood.Enqueue(
        //     this.hexGrid.GetCellFromCubeCoordinates(new CubeCoordinates(0, 0)));
    }
}
