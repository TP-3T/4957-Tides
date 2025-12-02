using UnityEngine;

namespace TTT.GameEvents
{
    public class FeatureDestroyArgs : Object
    {
        public Vector3 Location { get; set; }
        public ulong DestroyerId { get; set; }
    }
}
