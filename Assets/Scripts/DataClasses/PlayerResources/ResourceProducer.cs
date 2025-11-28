using System;
using System.Collections.Generic;
using UnityEngine;

namespace TTT.DataClasses.PlayerResources
{
    /// <summary>
    /// Encapsulates a list of resource amounts, and a production schedule that defines how those resources will be produced.
    /// </summary>
    [Serializable]
    public class ResourceProducer
    {
        [Tooltip("When and how the resources will be produced/spent.")]
        [field: SerializeField]
        public ProductionSchedule ProductionSchedule { get; private set; }

        [Tooltip("The resources to produce.")]
        [field: SerializeField]
        public List<Amount<PlayerResource>> ResourceAmounts
        {
            get;
            private set;
        }

        private void Produce(int multiplier)
        {
            foreach (var resource in ResourceAmounts)
            {
                resource.Thing.ApplyChange(resource.Count * multiplier);
            }
        }

        /// <summary>
        /// This should only be called once per producer.
        /// </summary>
        public void OnCreated() => Produce(ProductionSchedule.OnCreated);

        /// <summary>
        /// Produces some resources and adds to the player's resource amount.
        /// Warning: this is called every time a player's turn ends, including players other than the client.
        /// </summary>
        public void OnTurnEnding() => Produce(ProductionSchedule.OnTurnEnding);

        public void OnSeasonEnding() =>
            Produce(ProductionSchedule.OnSeasonEnding);

        public void OnYearEnding() => Produce(ProductionSchedule.OnYearEnding);

        /// <summary>
        /// This should only be called once per producer.
        /// </summary>
        public void OnSold() => Produce(ProductionSchedule.OnSold);

        /// <summary>
        /// This should only be called once per producer.
        /// </summary>
        public void OnDestroyed() => Produce(ProductionSchedule.OnDestroyed);
    }
}
