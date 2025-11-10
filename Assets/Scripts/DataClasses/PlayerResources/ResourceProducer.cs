using System;
using System.Collections.Generic;
using TTT.Managers;
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
        private List<Amount<PlayerResource>> resourceAmounts;

        /// <summary>
        /// When and how the resources will be produced/spent.
        /// </summary>
        public ProductionSchedule ProductionSchedule => productionSchedule;

        /// <summary>
        /// The resources being produced and their amounts.
        /// </summary>
        public List<Amount<PlayerResource>> ResourceAmounts => resourceAmounts;

        private void Initialize()
        {
            ResourcesController.Instance.TurnEnding += OnTurnEnding;
            ResourcesController.Instance.SeasonEnding += OnSeasonEnding;
            ResourcesController.Instance.YearEnding += OnYearEnding;
        }

        private void Dispose()
        {
            ResourcesController.Instance.TurnEnding -= OnTurnEnding;
            ResourcesController.Instance.SeasonEnding -= OnSeasonEnding;
            ResourcesController.Instance.YearEnding -= OnYearEnding;
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
        public void OnCreated()
        {
            Initialize();
            Produce(productionSchedule.OnCreated);
        }

        /// <summary>
        /// Produces some resources and adds to the player's resource amount.
        /// Warning: this is called every time a player's turn ends, including players other than the client.
        /// </summary>
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
        public void OnDestroyed()
        {
            Produce(productionSchedule.OnDestroyed);
            Dispose();
        }
    }
}
