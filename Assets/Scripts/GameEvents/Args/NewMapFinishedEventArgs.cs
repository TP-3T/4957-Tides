using UnityEngine;

namespace TTT.GameEvents
{
    public class NewMapFinishedEventArgs : Object
    {
        public bool WasSuccessful { get; set; }

        public float MaxMapHeight { get; set; } = 0;

        public float SeaLevel { get; set; } = 0;
    }
}
