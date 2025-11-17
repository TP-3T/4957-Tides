using System.Linq;
using TTT.DataClasses.HexData;
using TTT.DataClasses.PlayerResources;
using TTT.DataClasses.TileFeatures;
using TTT.Managers;
using TTT.ModularData;
using UnityEngine;

public class FeatureBuilder : MonoBehaviour
{
    public FeatureRuntimeSet SpawnedFeatures;

    public void OnBuildingFeature(Object eventArgs)
    {
        if (eventArgs is not BuildingFeatureArgs bfArgs)
        {
            return;
        }

        TryToBuild(FixLocation(bfArgs.Location), bfArgs.FeatureType);
    }

    public void OnDestroyingFeature(Object eventArgs)
    {
        if (eventArgs is not BuildingFeatureArgs bfArgs)
        {
            return;
        }

        DestroyAt(FixLocation(bfArgs.Location), wasSold: false);
    }

    public void OnSellingFeature(Object eventArgs)
    {
        if (eventArgs is not BuildingFeatureArgs bfArgs)
        {
            return;
        }

        DestroyAt(bfArgs.Location, wasSold: true);
    }

    private static Vector3 FixLocation(Vector3 location)
    {
        HexCell? exactCell = MapManager.Instance.GetCellFromPosition(location, out _);

        if (exactCell == null)
        {
            Debug.LogWarning($"Could not find cell at location {location}");
            return new Vector3(0, 0, 0);
        }

        return ((HexCell)exactCell).CellPosition;
    }

    private void TryToBuild(Vector3 location, FeatureType featureType)
    {
        if (CheckIfCanBuild(location, featureType))
        {
            BuildAt(location, featureType);
        }
        else
        {
            Debug.Log("Tried to build but failed due to constraints");
        }
    }

    private bool CheckIfCanBuild(Vector3 location, FeatureType featureType)
    {
        if (SpawnedFeatures.GetItems().Any(feats => feats.CellPosition.Equals(location)))
        {
            // then there's already something at this location
            return false;
        }

        if (!CheckCost(featureType))
        {
            // then the player is too poor
            return false;
        }

        // build location
        HexCell? selectedTile = MapManager.Instance.GetCellFromPosition(
            location,
            out bool foundCell
        );

        if (selectedTile == null || !foundCell)
        {
            Debug.LogWarning("Tried to build but couldn't find a HexCell");
            return false;
        }

        // build area
        HexCell[] adjacentTiles = MapManager
            .Instance.GetCellNeighbours((HexCell)selectedTile)
            .ToArray();

        // build constraints
        BuildConstraints constraints = featureType.Constraints;

        return featureType.BuildValidator.CanBuild(
            (HexCell)selectedTile,
            adjacentTiles,
            SpawnedFeatures,
            constraints
        );
    }

    private bool CheckCost(FeatureType featureType)
    {
        foreach (var resourceCost in featureType.Cost)
        {
            PlayerResource resource = resourceCost.Thing;
            if (resource.AmountOwned < resourceCost.Count)
            {
                return false;
            }
        }
        return true;
    }

    private void BuildAt(Vector3 location, FeatureType featureType)
    {
        GameObject gameInstance = Instantiate(featureType.Prefab);

        Vector3 displayLocation = new(location.x, location.y, location.z);
        gameInstance.transform.position = displayLocation;

        Vector3 displayScale = new(2, 2, 2);
        gameInstance.transform.localScale = displayScale;

        Feature feature = new(location, featureType, gameInstance);
        SpawnedFeatures.Add(feature);
    }

    private void DestroyAt(Vector3 location, bool wasSold)
    {
        Feature feature = SpawnedFeatures.GetByLocation(location);

        if (feature == null)
        {
            return;
        }

        Destroy(feature.PrefabInstance);
        SpawnedFeatures.Remove(feature);
    }
}
