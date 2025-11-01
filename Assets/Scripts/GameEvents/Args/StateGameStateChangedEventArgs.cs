using TTT.DataClasses.States;
using UnityEngine;

namespace TTT.GameEvents
{
    public class StateGameStateChangedEventArgs : ScriptableObject
    {
        public GameState NewState { get; set; }
    }
}
