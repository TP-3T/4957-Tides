using System;
using TTT.DataClasses.ModularData;
using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    /// <summary>
    /// It's possible to optimize this class by having it build a dictionary if we want.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Feature List",
        menuName = "Scriptable Objects/ModularData/Runtime Sets/Features"
    )]
    public class FeatureRuntimeSet : RuntimeSet<Feature>
    {
        public event Action<Feature> FeatureAdded;

        public event Action<Feature> FeatureRemoved;

        public override bool Add(Feature feature)
        {
            bool success = base.Add(feature);

            if (success)
            {
                FeatureAdded?.Invoke(feature);
            }

            return success;
        }

        public override bool Remove(Feature feature)
        {
            bool success = base.Remove(feature);

            if (success)
            {
                FeatureRemoved?.Invoke(feature);
            }

            return success;
        }

        public override bool RemoveAt(int index)
        {
            Feature removedFeature = Items[index];
            bool success = base.RemoveAt(index);

            if (success)
            {
                FeatureRemoved.Invoke(removedFeature);
            }

            return success;
        }

        public Feature GetByLocation(Vector3 cellPosition)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].CellPosition == cellPosition)
                {
                    return Items[i];
                }
            }
            return null;
        }

        public void RemoveByLocation(Vector3 cellPosition)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].CellPosition == cellPosition)
                {
                    RemoveAt(i);
                    return;
                }
            }
        }
    }
}
