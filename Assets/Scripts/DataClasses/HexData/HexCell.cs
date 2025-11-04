using TTT.DataClasses.Terrain;
using TTT.DataClasses.TileFeatures;
using UnityEngine;

namespace TTT.DataClasses.HexData
{
    public class HexCell : MonoBehaviour
    {
        public CubeCoordinates CellCubeCoordinates;
        public Vector3 CellPosition;
        public Color? CellColor = null;
        public MapTileData MapTileData;
        public TerrainType TerrainType;

        [SerializeField]
        public bool flooded = false;
        public int CenterVertexIndex;

        /// <summary>
        /// The type of feature currently instantiated on this cell.
        /// </summary>
        public FeatureType FeatureType { get; set; }

        /// <summary>
        /// The model of the feature currently instantiated on this cell.
        /// </summary>
        public GameObject InstantiatedFeature { get; set; }

        /// <summary>
        /// Flood this cell
        /// </summary>
        public void FloodCell()
        {
            this.flooded = true;
            this.CellColor = Color.blue;
        }

        /// <summary>
        /// Get the flooded state of the cell.
        /// </summary>
        public bool IsFlooded()
        {
            return this.flooded;
        }

        /// <summary>
        /// Mainly for debugging.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{{ cellPosition: {CellPosition}, cellCubeCoordinates: {CellCubeCoordinates}, cellColor: {CellColor} }}";
        }

        /// <summary>
        /// Builds a feature on this cell.
        /// </summary>
        /// <param name="featureType">The kind of feature to build.</param>
        public void BuildFeature(FeatureType featureType)
        {
            if (FeatureType != null)
            {
                // then there's already something on this cell
                return;
            }

            FeatureType = featureType;
            InstantiateFeaturePrefab(featureType);

            foreach (var producer in featureType.ResourceProducers)
            {
                producer.OnCreated();
            }
        }

        /// <summary>
        /// Destroys the feature on this cell, if one exists.
        /// </summary>
        /// <param name="wasSold">If this feature is being destroyed due to being sold.</param>
        public void DestroyFeature(bool wasSold)
        {
            if (FeatureType == null)
            {
                // then there's nothing on this cell
                return;
            }

            FeatureType = null;
            RemoveFeaturePrefab();

            if (wasSold)
            {
                foreach (var producer in FeatureType.ResourceProducers)
                {
                    producer.OnSold();
                }
            }
            else
            {
                foreach (var producer in FeatureType.ResourceProducers)
                {
                    producer.OnSold();
                }
            }
        }

        /// <summary>
        /// Spawns the model prefab of the given feature type on this cell.
        /// </summary>
        /// <param name="featureType">The type of feature to instantiate from.</param>
        private void InstantiateFeaturePrefab(FeatureType featureType)
        {
            Vector3 cellPos = CellPosition;
            Vector3 featurePos = new(cellPos.x, cellPos.y, cellPos.z);

            GameObject feature = Instantiate(featureType.Prefab);
            InstantiatedFeature = feature;

            featurePos.y += 0.5f * feature.transform.localScale.y;
            feature.transform.position = featurePos;
        }

        /// <summary>
        /// Removes the current model prefab from this cell.
        /// </summary>
        private void RemoveFeaturePrefab()
        {
            Destroy(InstantiatedFeature);
            InstantiatedFeature = null;
        }
    }
}
