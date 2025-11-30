using UnityEngine;

namespace TTT.GameEvents
{
    public class ResourceChangedEventArgs : Object
    {
        public int Money { get; set; }

        public int Power { get; set; }

        public int Population { get; set; }
    }
}
