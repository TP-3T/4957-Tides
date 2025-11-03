using UnityEngine;

namespace TTT.GameEvents
{
    public class StartNetworkEventArgs : Object
    {
        public bool IsHost { get; set; }
        public bool IsServer { get; set; }
    }
}
