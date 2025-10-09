using System.Collections.Generic;
using PlayerResources;
using Terrain;
using UnityEngine;

namespace Features
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

        [Tooltip("Radius of this feature's zone of control (1 = adjacent only).")]
        [SerializeField]
        private int zoneOfControlRadius;

        [Tooltip("The amount of polution emited per turn.")]
        [SerializeField]
        private int polutionEmission;

        [Tooltip("List of resource producers for this feature.")]
        [SerializeField]
        private List<BaseProducer> resourceProducers;

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
        /// Defines the max distance that you can place another feature relative to this one.
        /// A radius of 1 means you can only place other features adjacent to it.
        /// </summary>
        public int ZoneOfControlRadius => zoneOfControlRadius;

        /// <summary>
        /// The amount of polution emited per turn.
        /// </summary>
        public int PolutionEmission => polutionEmission;

        /// <summary>
        /// List of resource producers for this feature.
        /// </summary>
        public List<BaseProducer> ResourceProducers => resourceProducers;

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
