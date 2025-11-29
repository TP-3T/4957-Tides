using TTT.DataClasses.HexData;
using TTT.DataClasses.Terrain;
using UnityEngine;

namespace TTT.Player
{
    public class HexCellReference : ScriptableObject
    {
        [SerializeField]
        private bool hasValue;

        [SerializeField]
        private CubeCoordinates cellCubeCoordinates;

        [SerializeField]
        private Vector3 cellPosition;

        [SerializeField]
        private Color cellColor;

        [SerializeField]
        private int centerVertexIndex;

        [SerializeField]
        private bool flooded;

        [SerializeField]
        private TerrainTypeId terrainTypeId;

        public HexCell? Cell
        {
            get
            {
                if (!hasValue)
                    return null;
                return new HexCell
                {
                    CellCubeCoordinates = cellCubeCoordinates,
                    CellPosition = cellPosition,
                    CellColor = cellColor,
                    CenterVertexIndex = centerVertexIndex,
                    Flooded = flooded,
                    TerrainTypeId = terrainTypeId,
                };
            }
            set
            {
                if (value.HasValue)
                {
                    hasValue = true;
                    var cell = value.Value;
                    cellCubeCoordinates = cell.CellCubeCoordinates;
                    cellPosition = cell.CellPosition;
                    cellColor = cell.CellColor;
                    centerVertexIndex = cell.CenterVertexIndex;
                    flooded = cell.Flooded;
                    terrainTypeId = cell.TerrainTypeId;
                }
                else
                {
                    hasValue = false;
                }
            }
        }

        public bool HasValue => hasValue;

        public void Clear()
        {
            hasValue = false;
        }
    }
}
