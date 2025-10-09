using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    /// <summary>
    /// A lookup table for finding terrain types from their UIDs.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TerrainDictionary",
        menuName = "Scriptable Objects/TerrainDictionary"
    )]
    public class TerrainDictionary : ScriptableObject
    {
        [Tooltip("Terrain types to be indexed by this dictionary")]
        public List<TerrainType> Values;

        private Dictionary<string, TerrainType> _uidToTerrain;
        public Dictionary<string, TerrainType> UidToTerrain
        {
            get
            {
                _uidToTerrain ??= MakeDictionary();
                return _uidToTerrain;
            }
        }

        /// <summary>
        /// Gets an indexed terrain type from its UID
        /// </summary>
        public TerrainType Get(string uid)
        {
            TerrainType terrain;
            try
            {
                terrain = UidToTerrain[uid];
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(
                    $"The given terrain UID ({uid}) does not match any of the defined UIDs. Defined UIDs: {string.Join(", ", UidToTerrain.Keys)}"
                );
            }

            return terrain;
        }

        /// <summary>
        /// Makes a dictionary from the List of terrain types and their UIDs
        /// </summary>
        private Dictionary<string, TerrainType> MakeDictionary()
        {
            Dictionary<string, TerrainType> uidToTerrain = new();
            foreach (TerrainType terrain in Values)
            {
                uidToTerrain[terrain.UniqueID] = terrain;
            }
            return uidToTerrain;
        }

        // public void UpdateDictionary()
        // {
        //     _uidToTerrain = MakeDictionary();
        // }
    }
}
