using TTT.DataClasses.TileFeatures;
using UnityEngine;

namespace TTT.GameEvents
{
    public class BuildingFeatureEventArgs : Object
    {
        public Vector3 Location { get; set; }
        public FeatureType FeatureType { get; set; }
    }
}
