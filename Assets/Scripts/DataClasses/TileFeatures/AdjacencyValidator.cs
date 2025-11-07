using System.Linq;
using TTT.DataClasses;
using TTT.DataClasses.HexData;
using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
[CreateAssetMenu(
    fileName = "Adjacency Validation",
    menuName = "Scriptable Objects/TileFeatures/Build Validators/Adjacency Validation"
)]
public class AdjacencyValidator : BuildValidator
{
    public override bool CanBuild(
        HexCell buildLocation,
        HexCell[] nearbyTiles,
        BuildConstraints constraints
    )
    {
        // height check
        // (comparing tileHeight and maxHeight)
        // var maxHeight = constraints.MaximumHeight;
        // var tileHeight = buildLocation.MapTileData.Height;
        // if (tileHeight > maxHeight)
        // {
        //     // failed height check
        //     return false;
        // }

        // terrain check
        // (checking if any of the adjacent tiles have one of the required terrains)
        var terrains = constraints.TerrainConstraints.List;
        var useWhitelist = constraints.TerrainConstraints.Mode is FilterListMode.WHITELIST;
        var terrainIds = terrains.Select(terrain => terrain.UniqueID);
        // if (
        //     nearbyTiles.Any(tile =>
        //     {
        //         return terrainIds.Contains(tile.TerrainType.UniqueID) == useWhitelist;
        //     })
        // )
        // {
        //     // failed terrain check
        //     return false;
        // }

        // nearby features check
        // (checking for at least X features nearby)
        var featureRequirements = constraints.FeatureConstraints;
        // foreach (var requirement in featureRequirements)
        // {
        //     var matchingCells = nearbyTiles.Where(cell =>
        //     {
        //         return cell.FeatureType.UniqueID == requirement.Thing.UniqueID;
        //     });
        //     if (matchingCells.Count() < requirement.Count)
        //     {
        //         // failed nearby features check
        //         return false;
        //     }
        // }

        // all checks passed
        return true;
    }
}
}
