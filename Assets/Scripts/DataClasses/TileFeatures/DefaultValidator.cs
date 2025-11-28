using System.Linq;
using TTT.DataClasses;
using TTT.DataClasses.HexData;
using TTT.DataClasses.ModularData;
using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    [CreateAssetMenu(
        fileName = "Default Validation",
        menuName = "Scriptable Objects/TileFeatures/Build Validators/Default"
    )]
    public class DefaultValidator : BuildValidator
    {
        public override bool CanBuild(
            HexCell buildTile,
            HexCell[] nearbyTiles,
            FeatureRuntimeSet spawnedFeatures,
            BuildConstraints constraints
        )
        {
            if (buildTile.Flooded)
            {
                return false;
            }

            if (ExceedsMaxHeight(buildTile, constraints))
            {
                return false;
            }

            if (TerrainTypeAtLocationIsInvalid(buildTile, constraints))
            {
                return false;
            }

            Vector3[] nearbyTilePositions = nearbyTiles
                .Select(t => t.CellPosition)
                .ToArray();
            if (
                NotEnoughNearbyFeatures(
                    nearbyTilePositions,
                    spawnedFeatures,
                    constraints
                )
            )
            {
                return false;
            }

            // all checks passed
            return true;
        }
    }
}
