using TTT.DataClasses.HexData;
using UnityEngine;


namespace TTT.DataClasses.TileFeatures
{
    [CreateAssetMenu(
        fileName = "Accept All",
        menuName = "Scriptable Objects/TileFeatures/Build Validators/Accept All"
    )]
    public class AcceptAllValidator : BuildValidator
    {
        public override bool CanBuild(
            HexCell buildLocation,
            HexCell[] nearbyTiles,
            BuildConstraints constraints
        )
        {
            return true;
        }
    }
}
