using System;

namespace TTT.DataTypes
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

    public struct AxialCoordinates
    {
        public int q { get; private set; }
        public int r { get; private set; }

        public AxialCoordinates(int q, int r)
        {
            this.q = q;
            this.r = r;
        }
    }

    public struct CubeCoordinates
    {
        public int q { get; private set; }
        public int r { get; private set; }
        public int s { get; private set; }

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

        public static CubeCoordinates operator +(CubeCoordinates one, CubeCoordinates two)
        {
            return new CubeCoordinates(one.q + two.q, one.r + two.r, one.s + two.s);
        }
    }

    public struct CubeCoordinatesF
    {
        public float q { get; set; }
        public float r { get; set; }
        public float s { get; set; }

        public CubeCoordinatesF(float q, float r, float s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
        }

        public CubeCoordinatesF(float q, float r)
        {
            this.q = q;
            this.r = r;
            this.s = -q - r;
        }

        public override string ToString()
        {
            return $"({q}, {r}, {s})";
        }
    }
}
