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
        [SerializeField]
        private ProductionSchedule productionSchedule;

        [Tooltip("The resources to produce.")]
        [SerializeField]
        private List<ResourceAmount> resourceAmounts;

        /// <summary>
        /// When and how the resources will be produced/spent.
        /// </summary>
        public ProductionSchedule ProductionSchedule => productionSchedule;

        /// <summary>
        /// The resources being produced and their amounts.
        /// </summary>
        public List<ResourceAmount> ResourceAmounts => resourceAmounts;

        private void Produce(int multiplier)
        {
            foreach (var data in ResourceAmounts)
            {
                data.Resource.ApplyChange(data.Amount * multiplier);
            }
        }

        /// <summary>
        /// This should only be called once per producer.
        /// </summary>
        public void OnCreated() => Produce(productionSchedule.OnCreated);

        public void OnTurnEnding() => Produce(productionSchedule.OnTurnEnding);

        public void OnSeasonEnding() => Produce(productionSchedule.OnSeasonEnding);

        public void OnYearEnding() => Produce(productionSchedule.OnYearEnding);

        /// <summary>
        /// This should only be called once per producer.
        /// </summary>
        public void OnSold() => Produce(productionSchedule.OnSold);

        /// <summary>
        /// This should only be called once per producer.
        /// </summary>
        public void OnDestroyed() => Produce(productionSchedule.OnDestroyed);
    }
}
