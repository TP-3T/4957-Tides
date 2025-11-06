using System;
// using TTT.Features;
// using TTT.Terrain;
using Unity.Netcode;
using UnityEngine;

namespace TTT.DataClasses.HexData
{
    public struct HexCell : INetworkSerializable, IEquatable<HexCell>
    {
        public CubeCoordinates CellCubeCoordinates;
        public Vector3 CellPosition;
        public Color CellColor;
        public int CenterVertexIndex;
        public bool Flooded;

        public HexCell(
            CubeCoordinates cellCubeCoordinates,
            Vector3 cellPosition,
            Color cellColor)
        {
            CellCubeCoordinates = cellCubeCoordinates;
            CellPosition        = cellPosition;
            CellColor           = cellColor;

            CenterVertexIndex = -1;     // To let everyone know that this is not set
            Flooded = false;            // default flooded state of the cell
        }

        /// <summary>
        /// The type of feature currently instantiated on this cell.
        /// </summary>
        // public FeatureType FeatureType { get; set; }

        /// <summary>
        /// The model of the feature currently instantiated on this cell.
        /// </summary>
        // public GameObject InstantiatedFeature { get; set; }

        /// <summary>
        /// Mainly for debugging.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{{ cellPosition: {CellPosition}, cellCubeCoordinates: {CellCubeCoordinates}, cellColor: {CellColor}, flooded: {Flooded} }}";
        }

        public bool Equals(HexCell other)
        {
            return CellCubeCoordinates == other.CellCubeCoordinates
                && CellPosition == other.CellPosition
                && CellColor == other.CellColor
                && CenterVertexIndex == other.CenterVertexIndex
                && Flooded == other.Flooded;
        }

        public override bool Equals(object other)
        {
            return CellCubeCoordinates == ((HexCell)other).CellCubeCoordinates
                && CellColor == ((HexCell)other).CellColor
                && CellPosition == ((HexCell)other).CellPosition
                && CenterVertexIndex == ((HexCell)other).CenterVertexIndex
                && Flooded == ((HexCell)other).Flooded;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            CellCubeCoordinates.NetworkSerialize(serializer);

            serializer.SerializeValue(ref CellPosition);
            serializer.SerializeValue(ref CellColor);
            serializer.SerializeValue(ref Flooded);
            serializer.SerializeValue(ref CenterVertexIndex);
        }

        // /// <summary>
        // /// Builds a feature on this cell.
        // /// </summary>
        // /// <param name="featureType">The kind of feature to build.</param>
        // public void BuildFeature(FeatureType featureType)
        // {
        //     if (FeatureType != null)
        //     {
        //         // then there's already something on this cell
        //         return;
        //     }

        //     Vector3 cellPos = CellPosition;
        //     Vector3 featurePos = new(cellPos.x, cellPos.y, cellPos.z);

        //     FeatureType = featureType;
        //     GameObject feature = Instantiate(featureType.Prefab);
        //     InstantiatedFeature = feature;

        //     featurePos.y += 0.5f * feature.transform.localScale.y;
        //     feature.transform.position = featurePos;
        // }

        // /// <summary>
        // /// Destroys the feature on this cell, if one exists.
        // /// </summary>
        // public void DestroyFeature()
        // {
        //     if (FeatureType == null)
        //     {
        //         // then there's nothing on this cell
        //         return;
        //     }

        //     FeatureType = null;
        //     Destroy(InstantiatedFeature);
        //     InstantiatedFeature = null;
        // }
    }
}
