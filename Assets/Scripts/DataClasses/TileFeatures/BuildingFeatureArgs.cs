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

        /// <summary>
        /// Whether the feature is owned by the client receiving this event.
        /// </summary>
        public bool OwnedByClient;

        /// <summary>
        /// Whether building this feature should subtract the player's resources
        /// </summary>
        public bool CheckForCost = true;
    }
}
