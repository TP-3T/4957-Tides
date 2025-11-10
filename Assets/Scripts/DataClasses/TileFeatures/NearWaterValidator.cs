using System.Linq;
using TTT.DataClasses;
using TTT.DataClasses.HexData;
using TTT.DataClasses.TileFeatures;
using TTT.ModularData;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Near Water",
    menuName = "Scriptable Objects/TileFeatures/Build Validators/Near Water"
)]
public class NearWaterValidator : DefaultValidator
{
    private static bool TileHasWater(HexCell tile)
    {
        // we don't have a water/ocean/river/lake terrain type yet
        return tile.Flooded;
    }

    public override bool CanBuild(
        HexCell buildLocation,
        HexCell[] nearbyTiles,
        FeatureRuntimeSet spawnedFeatures,
        BuildConstraints constraints
    )
    {
        // default validation
        if (!base.CanBuild(buildLocation, nearbyTiles, spawnedFeatures, constraints))
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
