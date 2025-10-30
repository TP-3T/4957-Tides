using TTT.Features;
using TTT.Hex;
using UnityEngine;

namespace TTT.GameEvents
{
    [CreateAssetMenu(
        fileName = "Build Event Args",
        menuName = "Scriptable Objects/GameEvents/Build Event Args"
    )]
    public class BuildEventArgs : ScriptableObject
    {
        /// <summary>
        /// The hex cell to build the feature on.
        /// </summary>
        public HexCell HexCell;

        /// <summary>
        /// The feature type to build.
        /// </summary>
        public FeatureType FeatureType;
    }
}
