using UnityEngine;

namespace TTT.GameEvents
{
    public class StartNetworkEventArgs : ScriptableObject
    {
        public bool IsHost { get; set; }
    }
}
