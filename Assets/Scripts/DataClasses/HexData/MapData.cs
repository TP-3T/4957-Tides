using System;
using System.Collections.Generic;

namespace TTT.HexData
{
    [Serializable]
    public class MapData
    {
        public string Name;
        public int Width;
        public int Height;
        public List<MapTileData> MapTilesData;
    }
}
