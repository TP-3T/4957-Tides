using UnityEngine;

namespace TTT.GameEvents
{
    public class EndTurnEventArgs : ScriptableObject
    {
        public int Year { get; set; }
        public string Season { get; set; }

    }
}
