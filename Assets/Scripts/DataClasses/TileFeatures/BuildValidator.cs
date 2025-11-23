using System.Collections.Generic;
using System.Linq;
using TTT.DataClasses.HexData;
using TTT.ModularData;
using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    /// <summary>
    /// Allows features to customize logic for when and where they can be placed.
    /// </summary>
    public abstract class BuildValidator : ScriptableObject
    {
        /// <summary>
        /// Returns true if a feature can be built considering the given build location, adjacent cells, and the constraints for that feature.
        /// </summary>
        /// <param name="buildLocation">The cell where the feature should be built.</param>
        /// <param name="nearbyTiles">Any adjacent/nearby cells that may also be relevant.</param>
        /// <param name="constraints">The constraints of the feature being built.</param>
        /// <returns>True if the feature can be built on the given location.</returns>
        public abstract bool CanBuild(
            HexCell buildLocation,
            HexCell[] nearbyTiles,
            FeatureRuntimeSet spawnedFeatures,
            BuildConstraints constraints
        );

        public static bool ExceedsMaxHeight(
            HexCell buildLocation,
            BuildConstraints constraints
        )
        {
            var maxHeight = constraints.MaximumHeight;
            var tileHeight = buildLocation.CellPosition.y;
            return tileHeight > maxHeight;
        }

        public static bool TerrainTypeAtLocationIsInvalid(
            HexCell buildLocation,
            BuildConstraints constraints
        )
        {
            var terrainConstraints = constraints.TerrainConstraints.List;
            var terrainConstraintsUIDs = terrainConstraints.Select(terrain =>
                terrain.UniqueID
            );

            var isBlacklist =
                constraints.TerrainConstraints.Mode is FilterListMode.BLACKLIST;

            var buildLocationTerrain = buildLocation.TerrainTypeId;

            return terrainConstraintsUIDs.Contains(buildLocationTerrain)
                == isBlacklist;
        }

        public static bool NotEnoughNearbyFeatures(
            Vector3[] nearbyTilePositions,
            FeatureRuntimeSet spawnedFeatures,
            BuildConstraints constraints
        )
        {
            var featureConstraints = constraints.FeatureConstraints;
            var nearbyFeatures = new List<FeatureType>();

            // find nearby features
            foreach (Vector3 pos in nearbyTilePositions)
            {
                Feature feature = spawnedFeatures.GetByLocation(pos);
                if (feature != null)
                {
                    nearbyFeatures.Add(feature.Type);
                }
            }

            foreach (var featureConstraint in featureConstraints)
            {
                var matchingFeatures = nearbyFeatures.Where(near =>
                    near.UniqueID == featureConstraint.Thing.UniqueID
                );

                if (matchingFeatures.Count() < featureConstraint.Count)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
