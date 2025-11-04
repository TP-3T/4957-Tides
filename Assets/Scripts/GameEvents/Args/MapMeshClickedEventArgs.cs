using UnityEngine;

namespace TTT.GameEvents
{
    public class MapMeshClickedEventArgs : Object
    {
        public Vector3 ClickedPoint { get; set; }
        public ulong PlayerId { get; set; }
    }
}
