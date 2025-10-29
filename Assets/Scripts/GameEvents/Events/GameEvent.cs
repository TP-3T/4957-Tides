using System.Collections.Generic;
using UnityEngine;

namespace TTT.GameEvents
{
    /// <summary>
    /// A GameEvent can have listeners subscribe to it so that they are called when it's raised.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Game Event",
        menuName = "Scriptable Objects/GameEvents/Game Event"
    )]
    public class GameEvent : ScriptableObject
    {
        /// <summary>
        /// The listeners that this event will notify if it is raised.
        /// </summary>
        private readonly List<GameEventListener> eventListeners = new();

        [Tooltip("The arguments to pass when raising from the inspector")]
        [SerializeField]
        private Object defaultEventArgs;

        /// <summary>
        /// Calls all registered listeners with the event arguments defined in the inspector.
        /// </summary>
        public void Raise() => NotifyListeners(defaultEventArgs);

        /// <summary>
        /// Calls all registered listeners with the provided event arguments.
        /// </summary>
        /// <param name="eventArgs">A UnityEngine object containing data for the listeners to use.</param>
        public void Raise(Object eventArgs) => NotifyListeners(eventArgs);

        private void NotifyListeners(Object eventArgs)
        {
            // looping backwards lets listeners unregister themselves
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventRaised(eventArgs);
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void UnregisterListener(GameEventListener listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
    }
}
