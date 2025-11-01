using TTT.DataClasses.TileFeatures;
using UnityEngine;

namespace TTT.GameEvents
{
    [CreateAssetMenu(
        fileName = "FloodEventArgs",
        menuName = "TTT/Events/Build/BuildCancelEventArgs"
    )]
    public class BuildCancelEventArgs : ScriptableObject
    {
        public BuildingType building;
    }
}
