using TTT.DataClasses.HexData;
using TTT.DataClasses.ModularData;
using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    [CreateAssetMenu(
        fileName = "Reject All",
        menuName = "Scriptable Objects/TileFeatures/Build Validators/Reject All"
    )]
    public class RejectAllValidator : BuildValidator
    {
        public override bool CanBuild(
            HexCell buildLocation,
            HexCell[] nearbyTiles,
            FeatureRuntimeSet spawnedFeatures,
            BuildConstraints constraints
        )
        {
            return false;
        }
    }
}
