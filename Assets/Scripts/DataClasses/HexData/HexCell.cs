using TTT.DataClasses.HexData;
using TTT.Features;
using TTT.Terrain;
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

            Vector3 cellPos = CellPosition;
            Vector3 featurePos = new(cellPos.x, cellPos.y, cellPos.z);

            FeatureType = featureType;
            GameObject feature = Instantiate(featureType.Prefab);
            InstantiatedFeature = feature;

            featurePos.y += 0.5f * feature.transform.localScale.y;
            feature.transform.position = featurePos;
        }

        /// <summary>
        /// Destroys the feature on this cell, if one exists.
        /// </summary>
        public void DestroyFeature()
        {
            if (FeatureType == null)
            {
                // then there's nothing on this cell
                return;
            }

            FeatureType = null;
            Destroy(InstantiatedFeature);
            InstantiatedFeature = null;
        }
    }
}
