using System;
using System.Collections.Generic;
using UnityEngine;

namespace TTT.DataClasses.HexData
{
    [Serializable]
    public struct MapData
    {
        public int MapID;
        public string SteamID;
        public string MapName;
        public WorldState WorldState;
        public Dictionary<string, Dictionary<string, TileData>> MapTile;
        public string Name;
        public int Width;
        public int Height;
        public List<MapTileData> MapTilesData;
    }
}
