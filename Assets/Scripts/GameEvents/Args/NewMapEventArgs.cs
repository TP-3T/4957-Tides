using UnityEngine;

namespace TTT.GameEvents
{
    public class NewMapEventArgs : ScriptableObject
    {
        public TextAsset DataFile { get; set; }
    }
}
