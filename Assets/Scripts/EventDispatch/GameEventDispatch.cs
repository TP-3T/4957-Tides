using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


namespace TTT.Dispatch
{
    /// <summary>
    /// Dispatches Game events from UI to registered listeners.
    /// </summary>
    public class GameEventDispatch : MonoBehaviour
    {
        /// <summary>
        /// List of registered listeners.
        /// </summary>
        private static readonly Dictionary<Type, List<object>> listeners = new();


        /// <summary>
        /// Raises the Game event call to the registered listeners.
        /// </summary>
        /// <param name="nt">The Game Event</param>
        public static void Raise<T>(T evt)
        {
            if (!listeners.TryGetValue(typeof(T), out var list))
            {
                return;
            }

            foreach (var listener in list.Cast<IGameEventListener<T>>())
            {
                listener.OnEventRaised(evt);
            }
        }


        /// <summary>
        /// Registers a listener to receive Next Turn events.
        /// </summary>
        /// <param name="listener">The object that wants to listen, must implement INextTurnListener.</param>
        public static void RegisterListener<T>(IGameEventListener<T> listener)
        {
            var t = typeof(T);
            if (!listeners.TryGetValue(t, out var list))
            {
                listeners[t] = list = new List<object>();
            }

            if (!list.Contains(listener))
            {
                list.Add(listener);
            }

        }

        /// <summary>
        /// Unregisters a listener from receiving Next Turn events.
        /// </summary>
        /// <param name="listener">The listener to unregister, must implement INextTurnListener.</param>
        public static void UnregisterListener<T>(IGameEventListener<T> listener)
        {
            if (listeners.TryGetValue(typeof(T), out var list))
            {
                list.Remove(listener);
            }
        }
    }

    /// <summary>
    /// Interface for objects that want to listen for Game events.
    /// </summary>
    public interface IGameEventListener<T>
    {
        /// <summary>
        /// Method called when a Game event call occurs.
        /// </summary>
        /// <param name="nt">The Game Event</param>
        void OnEventRaised(T evt);
    }
}