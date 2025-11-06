using System;
using NUnit.Framework;
using Unity.Netcode;

namespace TTT.DataClasses.HexData
{
    public struct CubeCoordinates : INetworkSerializable, IEquatable<CubeCoordinates>
    {
        public int q;
        public int r;
        public int s;

        public CubeCoordinates(int q, int r, int s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
        }

        public CubeCoordinates(int q, int r)
        {
            this.q = q;
            this.r = r;
            this.s = (-q - r);
        }

        public override string ToString()
        {
            return $"({q}, {r}, {s})";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref q);
            serializer.SerializeValue(ref r);
            serializer.SerializeValue(ref s);
        }

        public bool Equals(CubeCoordinates other)
        {
            return this.q == other.q
                && this.r == other.r
                && this.s == other.s;
        }

        public static bool operator ==(CubeCoordinates one, CubeCoordinates two)
        {
            return one.q == two.q
                && one.r == two.r
                && one.s == two.s;
        }
        
        public static bool operator !=(CubeCoordinates one, CubeCoordinates two)
        {
            return one.q != two.q
                || one.r != two.r
                || one.s != two.s;
        }

        public static CubeCoordinates operator +(CubeCoordinates one, CubeCoordinates two)
        {
            return new CubeCoordinates(one.q + two.q, one.r + two.r, one.s + two.s);
        }
    }
}
