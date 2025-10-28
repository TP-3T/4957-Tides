using UnityEngine;

namespace TTT.UI
{
    /// <summary>
    /// Holds the UI's state.
    /// Needs a Canvas to render the UI components in.
    /// When the UI state changes, the old state's UI components are disabled, and the new state's components are enabled.
    /// </summary>
    [CreateAssetMenu(fileName = "InspectState", menuName = "Scriptable Objects/UI Context")]
    public class UIContext
    {
        private readonly Canvas canvas;

        private UIState state;

        public UIContext(Canvas parentCanvas, UIState initialState)
        {
            canvas = parentCanvas;

            bool allowedChange = ChangeState(initialState);
            if (!allowedChange)
            {
                throw new IllegalStateChangeException();
            }
        }

        /// <summary>
        /// Changes the state of the UI.
        /// </summary>
        /// <param name="newState">The state to change to</param>
        /// <returns>
        /// True if the state is changed succesfully.
        /// False otherwise.
        /// </returns>
        public bool ChangeState(UIState newState)
        {
            if (state != null)
            {
                DisableComponents();
            }

            // add logic to block specific state transitions here if you want
            state = newState;
            const bool successful = true;

            // only instantiate if necessary
            int alreadyInstantiated = newState.ComponentsToEnable.Count;
            int numPrefabs = newState.PrefabsToInstantiate.Count;
            if (alreadyInstantiated != numPrefabs)
            {
                InstantiateComponents();
            }

            EnableComponents();

            return successful;
        }

        /// <summary>
        /// Instantiates prefabs for the current state.
        /// Needs to be called when switching to a state for the first time.
        /// </summary>
        private void InstantiateComponents()
        {
            foreach (GameObject prefab in state.PrefabsToInstantiate)
            {
                GameObject obj = Object.Instantiate(prefab);

                // put UI element in Canvas
                obj.transform.SetParent(canvas.gameObject.transform, false);

                state.ComponentsToEnable.Add(obj);
            }
        }

        /// <summary>
        /// Disable all of the current UI state's components with GameObject#SetActive.
        /// </summary>
        private void DisableComponents()
        {
            foreach (GameObject gameObject in state.ComponentsToEnable)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Enable all of the current UI state's components with GameObject#SetActive.
        /// </summary>
        private void EnableComponents()
        {
            foreach (GameObject gameObject in state.ComponentsToEnable)
            {
                gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Thrown when attempting a UI state transition that is not allowed.
    /// </summary>
    public class IllegalStateChangeException : System.Exception { }
}
