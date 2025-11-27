using System;
using TTT.DataClasses.Terrain;
using UnityEngine;

namespace TTT.DataClasses.MapData
{
    [Serializable]
    public class TileData
    {
        public object Feature;
        public TerrainTypeId TileType;
        public int Owner;
        public int Elevation;
        public string Label;
    }
}
