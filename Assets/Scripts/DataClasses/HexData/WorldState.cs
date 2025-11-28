using System;
using UnityEngine;

namespace TTT.DataClasses.HexData
{
    [Serializable]
    public struct WorldState
    {
        public float Pollution { get; set; }
        public float SeaLevel { get; set; }
        public float Temp { get; set; }
        public int Year { get; set; }
    }
}
