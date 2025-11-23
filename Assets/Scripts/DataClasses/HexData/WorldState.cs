using System;
using UnityEngine;

namespace TTT.DataClasses.HexData
{
    [Serializable]
    public struct WorldState
    {
        public int Pollution;
        public int SeaLevel;
        public int Temp;
        public int Year;
    }
}
