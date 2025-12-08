using UnityEngine;

namespace TTT.DataClasses.PlayerResources
{
    /// <summary>
    /// Represents a player resource, such as a material or population count.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Player Resource",
        menuName = "Scriptable Objects/PlayerResources/Resource"
    )]
    public class PlayerResource : ScriptableObject
    {
        [Tooltip("Name of the resource")]
        [field: SerializeField]
        public string Name { get; private set; }

        [Tooltip("How much of this resource the player has")]
        [field: SerializeField]
        /// <summary>
        /// How much of this resource the player has.
        /// </summary>
        public float AmountOwned { get; private set; }

        /// <summary>
        /// Set the amount of this resource to <i>value</i>.
        /// </summary>
        /// <param name="value"></param>
        public void Set(float value)
        {
            AmountOwned = value;
        }

        /// <summary>
        /// Increase the amount of this resource by <i>value</i>.
        /// </summary>
        /// <param name="value"></param>
        public void ApplyChange(float value)
        {
            AmountOwned += value;
        }
    }
}
