using UnityEngine;

namespace TTT.Features
{
    [CreateAssetMenu(fileName = "BuildingType", menuName = "Scriptable Objects/BuildingType")]
    public class BuildingType : FeatureType
    {
        [Tooltip("Radius of this feature's zone of control (1 = adjacent only).")]
        [SerializeField]
        private int zoneOfControlRadius;

        /// <summary>
        /// Defines the max distance that you can place another feature relative to this one.
        /// A radius of 1 means you can only place other features adjacent to it.
        /// </summary>
        public int ZoneOfControlRadius => zoneOfControlRadius;

        //? CB: Should we have Gains and Costs be separate classes?
        //?     It almost feels like we could have a CostToBuild dictionary
        //?     that we use to track this, and this Currencies could just be
        //?     benefits. IDK.
        public Currencies currencies;
    }
}
