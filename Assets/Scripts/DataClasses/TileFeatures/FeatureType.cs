using System.Collections.Generic;
using TTT.DataClasses.PlayerResources;
using TTT.DataClasses.Terrain;
using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    /// <summary>
    /// Features are any structure that can be placed on top a hex cell.
    /// Each hex cell can hold only one feature.
    /// </summary>
    [CreateAssetMenu(fileName = "FeatureType", menuName = "Scriptable Objects/FeatureType")]
    public class FeatureType : ScriptableObject
    {
        [Tooltip("Identifier unique for all feature types.")]
        [SerializeField]
        private string Uid;

        [Tooltip("The amount of polution emited per turn.")]
        [SerializeField]
        private int pollutionEmission;

        [Tooltip("List of resource producers for this feature.")]
        [SerializeField]
        private List<ResourceProducer> resourceProducers;

        [Tooltip("Terrains that this feature can be placed on.")]
        [SerializeField]
        private List<TerrainType> allowedTerrains;

        [Tooltip("The prefab representing this feature")]
        [SerializeField]
        private GameObject prefab;

        /// <summary>
        /// The unique ID representing this feature type.
        /// </summary>
        public string UniqueID => Uid;

        /// <summary>
        /// The amount of pollution emitted per turn.
        /// </summary>
        public int PollutionEmission => pollutionEmission;

        /// <summary>
        /// List of resource producers for this feature.
        /// </summary>
        public List<ResourceProducer> ResourceProducers => resourceProducers;

        /// <summary>
        /// Terrains that this feature can be placed on.
        /// </summary>
        public List<TerrainType> AllowedTerrains => allowedTerrains;

        /// <summary>
        /// The prefab representing this feature.
        /// </summary>
        public GameObject Prefab => prefab;
    }
}
