using System;
using UnityEngine;

namespace TTT.DataClasses.PlayerResources
{
    /// <summary>
    /// Data class encapsulating a resource type and an arbitrary amount.
    /// </summary>
    [Serializable]
    public class ResourceAmount
    {
        [Tooltip("The resource being described.")]
        [SerializeField]
        private PlayerResource resource;

        [Tooltip("The amount of the resource.")]
        [SerializeField]
        private int amount;

        /// <summary>
        /// The resource being described.
        /// </summary>
        public PlayerResource Resource => resource;

        /// <summary>
        /// The amount of the resource (arbitary).
        /// Do not confuse this with the total amount owned by the player,
        /// which is stored in the PlayerResource Scriptable Object.
        /// </summary>
        public int Amount => amount;
    }
}
