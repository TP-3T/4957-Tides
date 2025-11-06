using System.Linq;
using TTT.DataClasses;
using TTT.DataClasses.HexData;
using TTT.DataClasses.TileFeatures;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Adjacency Validation",
    menuName = "Scriptable Objects/TileFeatures/Build Validators/Near Water"
)]
public class NearWaterValidator : DefaultValidator
{
    private static bool TileHasWater(HexCell tile)
    {
        // we don't have a water/ocean/river/lake terrain type yet
        return tile.IsFlooded();
    }

    public override bool CanBuild(
        HexCell buildLocation,
        HexCell[] nearbyTiles,
        BuildConstraints constraints
    )
    {
        // default validation
        if (base.CanBuild(buildLocation, nearbyTiles, constraints))
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
