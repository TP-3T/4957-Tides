using System;

namespace TTT.HexData
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
}
