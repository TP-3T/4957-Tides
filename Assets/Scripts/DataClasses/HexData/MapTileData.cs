using System;
using Unity.Netcode;

namespace TTT.DataClasses.HexData
{
    [Serializable]
    public struct MapTileData : INetworkSerializable
    {
        /// <summary>
        /// The UID of the tile's terrain type.
        /// </summary>
        public TerrainTypeId TileType;
        public int Height;
        public OffsetCoordinates OffsetCoordinates;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            OffsetCoordinates.NetworkSerialize(serializer);

            serializer.SerializeValue(ref TileType);
            serializer.SerializeValue(ref Height);
        }

        public void SetHeight(int newHeight)
        {
            Height = newHeight;
        }
    }
}
