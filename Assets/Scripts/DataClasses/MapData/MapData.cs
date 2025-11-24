using System;
using System.Collections.Generic;
using UnityEngine;

namespace TTT.DataClasses.MapData
{
    [Serializable]
    public struct MapData
    {
        public int MapID;
        public string SteamID;
        public string MapName;
        public WorldState WorldState;
        public Dictionary<string, Dictionary<string, TileData>> MapTile;
    }
}
