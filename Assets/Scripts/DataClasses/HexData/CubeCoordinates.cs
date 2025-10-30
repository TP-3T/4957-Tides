namespace TTT.DataClasses.HexData
{
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
}
