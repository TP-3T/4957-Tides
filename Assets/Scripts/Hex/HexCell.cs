using TTT.Features;
using TTT.Terrain;
using UnityEngine;

namespace TTT.Hex
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
    }
}
