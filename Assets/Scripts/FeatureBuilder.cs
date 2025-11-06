using System.Linq;
using TTT.DataClasses.HexData;
using TTT.DataClasses.TileFeatures;
using TTT.Managers;
using TTT.ModularData;
using UnityEngine;

public class FeatureBuilder : MonoBehaviour
{
    public FeatureRuntimeSet FeatureRuntimeSet;

    private void TryToBuild(Vector3 location, FeatureType featureType)
    {
        if (CheckConstraints(location, featureType))
        {
            BuildAt(location, featureType);
        }
        else
        {
            Debug.Log("Tried to build but failed due to constraints");
        }
    }

    private bool CheckConstraints(Vector3 location, FeatureType featureType)
    {
        if (FeatureRuntimeSet.Items.Any(feats => feats.CellPosition.Equals(location)))
        {
            return false;
        }

        // build location
        HexCell? selectedTile = MapManager.Instance.GetCellFromPosition(
            location,
            out bool foundCell
        );

        if (selectedTile == null || !foundCell)
        {
            Debug.Log("Tried to build but couldn't find a HexCell");
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
            constraints
        );
    }

    private void BuildAt(Vector3 location, FeatureType featureType)
    {
        GameObject gameInstance = Instantiate(featureType.Prefab);

        Vector3 displayLocation = new(location.x, location.y, location.z);
        displayLocation.y += 0.5f * gameInstance.transform.localScale.y;
        gameInstance.transform.position = displayLocation;

        Feature feature = new(location, featureType, gameInstance);
        FeatureRuntimeSet.Add(feature);

        feature.Type.ResourceProducers.ForEach(p => p.OnCreated());
    }

    private void DestroyAt(Vector3 location, bool wasSold)
    {
        Feature feature = FeatureRuntimeSet.GetByLocation(location);

        if (feature == null)
        {
            return;
        }

        Destroy(feature.PrefabInstance);
        FeatureRuntimeSet.Remove(feature);

        if (wasSold)
        {
            feature.Type.ResourceProducers.ForEach(p => p.OnSold());
        }
        else
        {
            feature.Type.ResourceProducers.ForEach(p => p.OnDestroyed());
        }
    }
}
