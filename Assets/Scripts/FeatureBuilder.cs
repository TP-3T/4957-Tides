using System.Collections.Generic;
using System.Linq;
using TTT.DataClasses.HexData;
using TTT.DataClasses.PlayerResources;
using TTT.DataClasses.TileFeatures;
using TTT.Hex;
using TTT.Managers;
using UnityEngine;

public class FeatureBuilder : MonoBehaviour
{
    /// <summary>
    /// Runtime set of features owned by this client.
    /// </summary>
    public FeatureRuntimeSet PlayerFeatures;

    /// <summary>
    /// Runtime set of all features spawned in the map.
    /// </summary>
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

        TryToBuild(
            FixLocation(bfArgs.Location),
            bfArgs.FeatureType,
            bfArgs.OwnedByClient
        );
    }

    public void OnDestroyingFeature(Object eventArgs)
    {
        if (eventArgs is not BuildingFeatureArgs bfArgs)
        {
            return;
        }

        DestroyAt(FixLocation(bfArgs.Location));
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

    private void TryToBuild(
        Vector3 location,
        FeatureType featureType,
        bool ownedByClient
    )
    {
        if (!CheckIfCanBuild(location, featureType))
        {
            //po: emit event to say build fail??
            Debug.Log($"Tried to build but failed due to constraints: {featureType.name}");
            return;
        }

        Feature feature = BuildAt(location, featureType);

        if (feature == null)
        {
            Debug.LogError("No renderers found in this prefab.");
            return;
        }

        SpawnedFeatures.Add(feature);
        if (ownedByClient)
        {
            PlayerFeatures.Add(feature);
        }
    }

    private bool CheckIfCanBuild(Vector3 location, FeatureType featureType)
    {
        Feature[] allFeatures = SpawnedFeatures.GetItems();

        if (allFeatures.Any(feat => feat.CellPosition.Equals(location)))
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

    private Feature BuildAt(Vector3 location, FeatureType featureType)
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
                Destroy(modelInstance);
                return null;
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

        // Mark as static for occlusion culling (if not in Editor)
#if UNITY_EDITOR
        UnityEditor.GameObjectUtility.SetStaticEditorFlags(
            parent,
            UnityEditor.StaticEditorFlags.OccludeeStatic
                | UnityEditor.StaticEditorFlags.OccluderStatic
        );
#endif

        // encapsulate in feature object
        Feature feature = new(location, featureType, parent);
        return feature;
    }

    private void DestroyAt(Vector3 location)
    {
        Feature feature = SpawnedFeatures.GetByLocation(location);

        if (feature == null)
        {
            return;
        }

        Destroy(feature.PrefabInstance);

        SpawnedFeatures.Remove(feature);
        PlayerFeatures.Remove(feature); // returns quietly if not player owned
    }

    private static float Hypotenuse(float x, float y)
    {
        double zSquared = System.Math.Pow(x, 2) + System.Math.Pow(y, 2);
        double z = System.Math.Sqrt(zSquared);
        return (float)z;
    }
}
