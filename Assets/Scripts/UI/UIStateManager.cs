using UnityEngine;

namespace UI
{
    /// <summary>
    /// Manages the UI's state, meaning it controls what UI components are enabled.
    /// </summary>
    public class UIStateManager : MonoBehaviour
    {
        [Tooltip("The Canvas where the UI elements will be rendered.")]
        [SerializeField]
        private Canvas canvas;

        [Tooltip("The UI's default disabled state")]
        [SerializeField]
        private UIState disabledState;

        [Tooltip("The UI's initial state")]
        [SerializeField]
        private UIState initialState;

        /// <summary>
        /// Holds the UI's current state and handles state changes
        /// </summary>
        private UIContext context;

        private void Awake()
        {
            context = new UIContext(canvas, disabledState);
        }

        public void Start()
        {
            //context.ChangeState(initialState);
        }

        public void ChangeToInitialState()
        {
            context.ChangeState(initialState);
        }
    }
}
