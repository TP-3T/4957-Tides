using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    public class Feature
    {
        /// <summary>
        /// The position of the cell this feature belongs to.
        /// </summary>
        public Vector3 CellPosition { get; private set; }

        /// <summary>
        /// The type of feature.
        /// </summary>
        public FeatureType Type { get; private set; }

        /// <summary>
        /// The currently instantiated GameObject from this feature's prefab.
        /// </summary>
        public GameObject PrefabInstance { get; private set; }

        public Feature(Vector3 location, FeatureType type, GameObject instance)
        {
            CellPosition = location;
            Type = type;
            PrefabInstance = instance;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Feature f)
            {
                return false;
            }
            return CellPosition.Equals(f.CellPosition);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
