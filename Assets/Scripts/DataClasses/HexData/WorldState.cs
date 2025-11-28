using System;
using UnityEngine;

namespace TTT.DataClasses.HexData
{
    [Serializable]
    public struct WorldState
    {
        public float Pollution;
        public float SeaLevel;
        public float Temp;
        public int Year;
    }
}
