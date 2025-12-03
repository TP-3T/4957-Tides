using System.Linq;
using TTT.DataClasses;
using TTT.DataClasses.HexData;
using TTT.DataClasses.Terrain;
using TTT.DataClasses.TileFeatures;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Near Water",
    menuName = "Scriptable Objects/TileFeatures/Build Validators/Near Water"
)]
public class NearWaterValidator : DefaultValidator
{
    private readonly TerrainTypeId[] waterTerrains = new TerrainTypeId[]
    {
        TerrainTypeId.OCEAN,
        TerrainTypeId.ESTUARY,
        TerrainTypeId.FRESHWATER,
    };

    private bool TileHasWater(HexCell tile)
    {
        if (tile.Flooded)
        {
            return true;
        }

        TerrainTypeId terrainId = tile.TerrainTypeId;
        if (waterTerrains.Contains(terrainId))
        {
            return true;
        }

        return false;
    }

    public override bool CanBuild(
        HexCell buildLocation,
        HexCell[] nearbyTiles,
        FeatureRuntimeSet spawnedFeatures,
        BuildConstraints constraints
    )
    {
        // default validation
        if (
            !base.CanBuild(
                buildLocation,
                nearbyTiles,
                spawnedFeatures,
                constraints
            )
        )
        {
            return false;
        }

        // (checking if any of the adjacent tiles are water tiles)
        if (!nearbyTiles.Any(tile => TileHasWater(tile)))
        {
            // failed water adjacency check
            return false;
        }

        // all checks passed
        return true;
    }
}
