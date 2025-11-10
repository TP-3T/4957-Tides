using System;
using TTT.ModularData;
using UnityEngine;

namespace TTT.DataClasses.Terrain
{
    /// <summary>
    /// The terrain type of a tile. Represents climates and helps define regions.
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainType", menuName = "Scriptable Objects/TerrainType")]
    public class TerrainType : ScriptableObject
    {
        [Tooltip("The color that represents this terrain.")]
        [SerializeField]
        private ColorReference TileColor;

        [Tooltip("The unique ID of this terrain.")]
        [SerializeField]
        private TerrainTypeId uid;

        /// <summary>
        /// The color of this terrain type.
        /// </summary>
        public Color Color => TileColor.Value;

        /// <summary>
        /// The unique ID representing this terrain type.
        /// </summary>
        public TerrainTypeId UniqueID => uid;
    }
}
