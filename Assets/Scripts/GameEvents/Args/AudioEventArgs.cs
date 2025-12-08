using TTT.DataClasses;
using TTT.DataClasses.States;
using UnityEngine;

namespace TTT.GameEvents
{
    public class AudioEventArgs : Object
    {
        public AudioTypes Type { get; set; }
        public string ToPlay { get; set; }
    }
}
