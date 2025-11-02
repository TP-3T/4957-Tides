using System;
using System.Collections.Generic;
using TTT.DataClasses.Terrain;
using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    /// <summary>
    /// Defines a feature's build constraints, such as where it can be placed, what features must be next to it, etc.
    /// </summary>
    [Serializable]
    public class BuildConstraints
    {
        /// <summary>
        /// The maximum height where building is still allowed.
        /// </summary>
        [field: Tooltip("Max height where you're allowed to build.")]
        [field: SerializeField]
        public int MaximumHeight { get; private set; }

        /// <summary>
        /// List of terrains to either allow or disallow.
        /// </summary>
        [field: Tooltip("Terrains to either allow or disallow.")]
        [field: SerializeField]
        public FilterList<TerrainType> TerrainConstraints { get; private set; }

        /// <summary>
        /// List of features to either require or ban having nearby.
        /// </summary>
        [field: Tooltip("Features to either require or ban.")]
        [field: SerializeField]
        public List<FeatureAmount> FeatureConstraints { get; private set; }
    }
}
