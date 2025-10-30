using System;
using System.Collections.Generic;

namespace TTT.DataTypes
{
    [Serializable]
    public class MapTileData
    {
        /// <summary>
        /// The UID of the tile's terrain type.
        /// </summary>
        public string TileType;
        public int Height;
        public OffsetCoordinates OffsetCoordinates;
    }

    [Serializable]
    public class MapData
    {
        public string Name;
        public int Width;
        public int Height;
        public List<MapTileData> MapTilesData;
    }
}
