using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TTT.UI
{
    /// <summary>
    /// A UI state contains a list of what UI components are enabled for that state.
    /// </summary>
    public abstract class UIState : ScriptableObject
    {
        [Tooltip("GameObjects that should be enabled during this UI state.")]
        [SerializeField]
        protected List<GameObject> prefabsToInstantiate;

        /// <summary>
        /// UI GameObjects that need to be instantiated for this state.
        /// </summary>
        public List<GameObject> PrefabsToInstantiate => prefabsToInstantiate;

        protected List<GameObject> componentsToEnable = new();

        /// <summary>
        /// Instantiated UI GameObjects to enable for this state.
        /// </summary>
        public List<GameObject> ComponentsToEnable
        {
            get
            {
                // for some reason Unity keeps adding 1 null object to this list
                if (componentsToEnable.Any((gameObj) => gameObj == null))
                {
                    componentsToEnable.Clear();
                }
                return componentsToEnable;
            }
            set => componentsToEnable = value;
        }

        public abstract void OnClick();
    }
}
