using System;
using UnityEngine;

namespace TTT.UI
{
    public class UIInputManager : MonoBehaviour
    {
        [Tooltip("The Canvas that this input manager is for.")]
        [SerializeField]
        private Canvas canvas;

        [Tooltip("The UI's initial/default state.")]
        [SerializeField]
        private UIState initialState;

        /// <summary>
        /// Holds and controls the UI's current state
        /// </summary>
        private UIContext context;

        private UIState CurrentState => context.State;

        private const int LEFT_MOUSE_BUTTON = 0;

        private const int RIGHT_MOUSE_BUTTON = 1;

        public void ChangeState(UnityEngine.Object eventArgs)
        {
            if (eventArgs is not UIState newState)
            {
                return;
            }

            UIState oldState = CurrentState;

            bool success = context.ChangeState(newState);
            if (!success)
            {
                Debug.LogWarning($"Unable to switch UI States (old: {oldState}, new: {newState})");
            }
        }

        private void Awake()
        {
            if (canvas == null)
            {
                throw new NullReferenceException("UIInputManager is missing its canvas.");
            }
            if (initialState == null)
            {
                throw new NullReferenceException("UIInputManager is missing its initial UI State");
            }
            context = new UIContext(canvas, initialState);
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(LEFT_MOUSE_BUTTON))
            {
                Vector3 mousePos = Input.mousePosition;
                CurrentState.OnLeftClick(mousePos);
            }
        }
    }
}
