namespace TTT.DataClasses.HexData
{
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
