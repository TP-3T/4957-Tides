using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    [CreateAssetMenu(
        fileName = "Args (Building Feature)",
        menuName = "TTT/EventArgs/BuildingFeatureArgs"
    )]
    public class BuildingFeatureArgs : ScriptableObject
    {
        /// <summary>
        /// Vector3 position of the HexCell where the building is happening.
        /// </summary>
        public Vector3 Location;

        /// <summary>
        /// The type of feature being built.
        /// </summary>
        public FeatureType FeatureType;
    }
}
