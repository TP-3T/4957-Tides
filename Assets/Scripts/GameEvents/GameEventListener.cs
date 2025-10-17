using UnityEngine;
using UnityEngine.Events;

namespace GameEvents
{
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("The GameEvent to listen to.")]
        public GameEvent Event;

        [Tooltip("What to call when the GameEvent is raised.")]
        public UnityEvent Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
            Response.Invoke();
        }
    }
}
