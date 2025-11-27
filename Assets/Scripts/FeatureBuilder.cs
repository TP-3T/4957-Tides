using System.Linq;
using TTT.DataClasses.HexData;
using TTT.DataClasses.PlayerResources;
using TTT.DataClasses.TileFeatures;
using TTT.Hex;
using TTT.Managers;
using TTT.ModularData;
using UnityEngine;

public class FeatureBuilder : MonoBehaviour
{
    public FeatureRuntimeSet SpawnedFeatures;

    private const float hexCellPadding = 0.05f;

    private readonly float hexCellSize =
        (1 - hexCellPadding) * HexMath.InnerRadius(MapManager.HexSize);

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
        HexCell? exactCell = MapManager.Instance.GetCellFromPosition(
            location,
            out _
        );

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
        if (
            SpawnedFeatures
                .GetItems()
                .Any(feats => feats.CellPosition.Equals(location))
        )
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
        GameObject modelInstance = Instantiate(featureType.Prefab);

        Bounds modelBounds;
        Vector3 center;
        try
        {
            // prefab with one renderer at the top level
            modelBounds = modelInstance.GetComponent<Renderer>().bounds;
            center = modelBounds.center;
        }
        catch (MissingComponentException)
        {
            // prefab with many child renderers
            MeshRenderer[] renderers =
                modelInstance.GetComponentsInChildren<MeshRenderer>();

            if (renderers.Length == 0)
            {
                Debug.LogError("No renderers found in this prefab.");
                return;
            }

            modelBounds = renderers[0].bounds;
            center = modelBounds.center;
            for (int i = 1; i < renderers.Length; i++)
            {
                modelBounds.Encapsulate(renderers[i].bounds);
                center.x += renderers[i].bounds.center.x;
                center.z += renderers[i].bounds.center.z;
            }
            center.x /= renderers.Length; // average center
            center.z /= renderers.Length; // average center
            // modelBounds.center = center;
        }

        Vector3 modelSize = modelBounds.size;
        float modelLength = Hypotenuse(modelSize.x, modelSize.y);
        float scaleFactor = 2 * hexCellSize / modelLength;

        // move
        Vector3 displayLocation = new(
            location.x - center.x,
            location.y,
            location.z - center.z
        );
        modelInstance.transform.position = displayLocation;

        // set a parent
        GameObject parent = new($"{modelInstance.name} (Parent)");
        parent.transform.position = location;
        modelInstance.transform.SetParent(parent.transform);

        // scale (the parent, not the model)
        Vector3 displayScale = new(scaleFactor, scaleFactor, scaleFactor);
        parent.transform.localScale = displayScale;

        // register
        Feature feature = new(location, featureType, modelInstance);
        SpawnedFeatures.Add(feature);
    }
    public void OnLoadingMapFeature(Object eventArgs)
    {
        if (eventArgs is not BuildingFeatureArgs bfArgs)
        {
            return;
        }

        BuildAt(bfArgs.Location, bfArgs.FeatureType);
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

    private static float Hypotenuse(float x, float y)
    {
        double zSquared = System.Math.Pow(x, 2) + System.Math.Pow(y, 2);
        double z = System.Math.Sqrt(zSquared);
        return (float)z;
    }
}
