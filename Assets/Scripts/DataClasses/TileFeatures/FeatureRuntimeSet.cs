using System.Linq;
using TTT.DataClasses.TileFeatures;
using UnityEngine;

namespace TTT.ModularData
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
                    Items.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
