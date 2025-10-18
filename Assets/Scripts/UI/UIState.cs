using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace TTT.UI
{
    /// <summary>
    /// A UI state contains a list of what UI components are enabled for that state.
    /// </summary>
    [CreateAssetMenu(fileName = "UIState", menuName = "Scriptable Objects/UIState")]
    public class UIState : ScriptableObject
    {
        [Tooltip("GameObjects that should be enabled during this UI state.")]
        [SerializeField]
        private List<GameObject> prefabsToInstantiate;

        /// <summary>
        /// UI GameObjects that need to be instantiated for this state.
        /// </summary>
        public List<GameObject> PrefabsToInstantiate => prefabsToInstantiate;

        /// <summary>
        /// Instantiated UI GameObjects to enable for this state.
        /// May be null.
        /// </summary>
        public List<GameObject> ComponentsToEnable { get; set; } = new();
    }
}
