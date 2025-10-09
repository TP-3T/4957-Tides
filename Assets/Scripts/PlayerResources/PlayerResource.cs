using UnityEngine;

namespace PlayerResources
{
    /// <summary>
    /// Represents a player resource, such as a material or population count.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerResource", menuName = "Scriptable Objects/PlayerResource")]
    public class PlayerResource : ScriptableObject
    {
        [Tooltip("Name of the resource")]
        [SerializeField]
        private string resourceName;

        [Tooltip("How much of this resource the player has")]
        [SerializeField]
        private int amount;

        /// <summary>
        /// The name of this resource.
        /// </summary>
        public string Name => resourceName;

        /// <summary>
        /// How much of this resource the player has.
        /// </summary>
        public int Amount => amount;

        /// <summary>
        /// Set the amount of this resource to <i>value</i>.
        /// </summary>
        /// <param name="value"></param>
        public void Set(int value)
        {
            amount = value;
        }

        /// <summary>
        /// Increase the amount of this resource by <i>value</i>.
        /// </summary>
        /// <param name="value"></param>
        public void ApplyChange(int value)
        {
            amount += value;
        }
    }
}
