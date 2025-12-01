using System;
using UnityEngine;

namespace TTT.DataClasses.HexData
{
    [Serializable]
    public struct WorldState
    {
        // ppm
        public float Pollution;

        // Metres
        public float SeaLevel;

        // Deg C
        public float Temp;
        public int Year;
    }
}
