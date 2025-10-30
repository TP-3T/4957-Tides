using TTT.DataClasses.States;
using UnityEngine;

namespace TTT.GameEvents
{
    public class StateSystemChangeEventArgs : Object
    {
        public SystemState NewState { get; set; }
    }
}
