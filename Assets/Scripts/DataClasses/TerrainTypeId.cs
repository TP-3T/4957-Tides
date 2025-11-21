using UnityEngine;

namespace TTT.DataClasses
{
    /// <summary>
    /// The unique IDs of all terrain types
    /// </summary>
    public enum TerrainTypeId
    {
        PLAINS,
        DESERT,
        COASTAL,
    }

    /// <summary>
    /// Terrain features
    /// </summary>
    public enum TerrainFeatureId
    {
        LBML,           // INDUSTRY
        FACT,
        PORT,
        MINE,

        TREE,           // NATURE


        CITY,
        RURL,
        NUKE,
        COAL,
        HYDR,
        AIRP,

    }
}
