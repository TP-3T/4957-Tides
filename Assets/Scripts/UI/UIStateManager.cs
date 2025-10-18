using UnityEngine;

namespace TTT.UI
{
    /// <summary>
    /// Manages the UI's state, meaning it controls what UI components are enabled.
    /// </summary>
    public class UIStateManager : MonoBehaviour
    {
        [Tooltip("The Canvas where the UI elements will be rendered.")]
        [SerializeField]
        private Canvas canvas;

        [Tooltip("The UI's initial state")]
        [SerializeField]
        private UIState initialState;

        /// <summary>
        /// Holds the UI's current state and handles state changes.
        /// </summary>
        private UIContext context;

        private void Awake()
        {
            context = new UIContext(canvas, initialState);
        }

        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="eventArgs">The UI State to change to.</param>
        public void OnStateChanging(Object eventArgs)
        {
            if (eventArgs is UIState newState)
            {
                ChangeState(newState);
            }
        }

        private void ChangeState(UIState newState)
        {
            context.ChangeState(newState);
        }
    }
}
