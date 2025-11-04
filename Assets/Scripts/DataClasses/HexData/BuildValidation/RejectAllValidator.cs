using TTT.DataClasses.HexData;
using TTT.DataClasses.TileFeatures;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Reject All",
    menuName = "Scriptable Objects/TileFeatures/Build Validators/Reject All"
)]
public class RejectAllValidator : BuildValidator
{
    public override bool CanBuild(
        HexCell buildLocation,
        HexCell[] nearbyTiles,
        BuildConstraints constraints
    )
    {
        return false;
    }
}
