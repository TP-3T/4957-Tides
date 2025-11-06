using TTT.DataClasses.HexData;
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
            BuildConstraints constraints
        );
    }
}
