using System;
using Unity.Netcode;

namespace TTT.DataClasses.HexData
{
    [Serializable]
    public struct OffsetCoordinates : INetworkSerializable, IEquatable<OffsetCoordinates>
    {
        public int x;
        public int z;

        public OffsetCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public bool Equals(OffsetCoordinates other)
        {
            return this.x == other.x
                && this.z == other.z;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref x);
            serializer.SerializeValue(ref z);
        }

        public override string ToString()
        {
            return $"({x}, {z})";
        }
    }
}
