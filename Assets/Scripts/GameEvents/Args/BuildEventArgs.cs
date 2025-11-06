using TTT.DataClasses.HexData;
using TTT.DataClasses.TileFeatures;
using UnityEngine;

namespace TTT.GameEvents
{
    public class BuildEventArgs
    {
        public BuildingType Building { get; set; }
        public HexCell HexCell { get; set; }
    }
}
