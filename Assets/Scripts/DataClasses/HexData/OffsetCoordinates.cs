using System;

namespace TTT.DataClasses.HexData
{
    [Serializable]
    public struct OffsetCoordinates
    {
        public int x;
        public int z;

        public OffsetCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public override string ToString()
        {
            return $"({x}, {z})";
        }
    }
}
