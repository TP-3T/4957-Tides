using System.Collections.Generic;
using TTT.DataClasses.PlayerResources;
using TTT.DataClasses.HexData;
using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    /// <summary>
    /// Features are any structure that can be placed on top a hex cell.
    /// Each hex cell can hold only one feature.
    /// </summary>
    [CreateAssetMenu(
        fileName = "FeatureType",
        menuName = "Scriptable Objects/TileFeatures/Feature Type"
    )]
    public class FeatureType : ScriptableObject
    {
        /// <summary>
        /// The unique ID representing this feature type.
        /// </summary>
        [field: Tooltip("Identifier unique for all feature types.")]
        [field: SerializeField]
        public string UniqueID { get; private set; }

        /// <summary>
        /// The amount of pollution emitted per turn.
        /// </summary>
        [field: Tooltip("The amount of polution emited per turn.")]
        [field: SerializeField]
        public int PollutionEmission { get; private set; }

        /// <summary>
        /// The constraints for building this feature.
        /// </summary>
        [field: Tooltip("The constraints for building this feature.")]
        [field: SerializeField]
        public BuildConstraints Constraints { get; private set; }

        [field: SerializeField]
        public BuildValidator BuildValidator { get; private set; }

        /// <summary>
        /// List of resource producers for this feature.
        /// </summary>
        [field: Tooltip("List of resource producers for this feature.")]
        [field: SerializeField]
        public List<ResourceProducer> ResourceProducers { get; private set; }

        /// <summary>
        /// The cost of building this feature, as a list of resource amounts.
        /// </summary>
        [field: Tooltip("The cost of building this.")]
        [field: SerializeField]
        public List<Amount<PlayerResource>> Cost { get; private set; }

        /// <summary>
        /// The prefab representing this feature.
        /// </summary>
        [field: Tooltip("The prefab representing this feature.")]
        [field: SerializeField]
        public GameObject Prefab { get; private set; }
    }
}
