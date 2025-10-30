using System;
using System.Collections.Generic;
using TTT.DataClasses.HexData;

namespace TTT.Hex
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
