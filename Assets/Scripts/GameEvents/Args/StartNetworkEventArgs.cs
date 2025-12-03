using UnityEngine;

namespace TTT.GameEvents
{
    public class StartNetworkEventArgs : ScriptableObject
    {
        public bool IsHost { get; set; }
        public string Ip { get; set; }
        public ushort Port { get; set; }
    }
}
