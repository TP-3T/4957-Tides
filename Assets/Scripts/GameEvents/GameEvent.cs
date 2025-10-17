using System.Collections.Generic;
using UnityEngine;

namespace GameEvents
{
    /// <summary>
    /// A GameEvent can have listeners subscribe to it so that they are called when it's raised.
    /// </summary>
    [CreateAssetMenu(fileName = "GameEvent", menuName = "Scriptable Objects/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        /// <summary>
        /// The listeners that this event will notify if it is raised.
        /// </summary>
        private readonly List<GameEventListener> eventListeners = new();

        public void Raise()
        {
            // go through list backwards so that listeners can unregister themselves without causing an out of bounds error
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!eventListeners.Contains(listener))
            {
                eventListeners.Add(listener);
            }
        }

        public void UnregisterListener(GameEventListener listener)
        {
            if (eventListeners.Contains(listener))
            {
                eventListeners.Remove(listener);
            }
        }
    }
}
