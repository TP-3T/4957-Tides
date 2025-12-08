using UnityEngine;

namespace TTT.DataClasses.PlayerResources
{
    /// <summary>
    /// Defines a producer's resource generation multipliers for different game events.
    /// </summary>
    /// <remarks>
    /// onXEvent = 0 --> Resources are not generated for this event
    /// <br/>
    /// onXEvent = 1 --> Resources are generated with a 1x multiplier
    /// <br/>
    /// onXEvent = 3 --> Resources are generated with a 3x multiplier
    /// <br/>
    /// onXEvent = -1 --> Resources are spent with a 1x multiplier
    /// <br/>
    /// </remarks>
    [CreateAssetMenu(
        fileName = "Production Schedule",
        menuName = "Scriptable Objects/PlayerResources/Production Schedule"
    )]
    public class ProductionSchedule : ScriptableObject
    {
        [Tooltip("(i.e. when a building is placed)")]
        [field: SerializeField]
        /// <summary>
        /// Resource multiplier for OnCreated.
        /// (i.e. when a building is placed)
        /// </summary>
        public int OnCreated { get; private set; }

        [Tooltip("(most resources generate per turn)")]
        [field: SerializeField]
        /// <summary>
        /// Resource multiplier for when a turn ends.
        /// (this applies to most producers)
        /// </summary>
        public int OnTurnEnding { get; private set; }

        [Tooltip("(some resources might only generate per season)")]
        [field: SerializeField]
        /// <summary>
        /// Resource multiplier for when a season ends.
        /// </summary>
        /// <remarks>
        /// Setting this and OnTurnEnding will cause duplicated generation at the end of every season.
        /// </remarks>
        public int OnSeasonEnding { get; private set; }

        [Tooltip("(some resources might only generate per year)")]
        [field: SerializeField]
        /// <summary>
        /// Resource multiplier for when a year ends.
        /// </summary>
        /// <remarks>
        /// Setting this and OnTurnEnding will cause duplicated generation at the end of every year.
        /// </remarks>
        public int OnYearEnding { get; private set; }

        [Tooltip("(i.e. when a building is sold)")]
        [field: SerializeField]
        /// summary>
        /// Resource multiplier for OnSold.
        /// (i.e. when a building is sold by the player)
        /// </summary>
        public int OnSold { get; private set; }

        [Tooltip("(i.e. when a building is destroyed by a flood)")]
        [field: SerializeField]
        /// <summary>
        /// Resource multiplier for OnDestroyed.
        /// (i.e. when a building is destroyed by a flood)
        /// </summary>
        public int OnDestroyed { get; private set; }
    }
}
