using System;
using System.Linq;
using TTT.DataClasses;
using TTT.DataClasses.TileFeatures;
using TTT.Helpers;
using UnityEngine;

namespace TTT.Managers
{
    public class ResourceProductionManager
        : GenericSingleton<ResourceProductionManager>
    {
        [SerializeField]
        private FeatureRuntimeSet playerFeatures;

        private event Action turnEnding;

        private event Action seasonEnding;

        private event Action yearEnding;

        public void OnTurnEnding(UnityEngine.Object eventArgs)
        {
            // TODO: need to check if this is the player whose turn is ending

            turnEnding?.Invoke();
        }

        public void OnSeasonEnding(UnityEngine.Object _)
        {
            float newPollution = playerFeatures
                .GetItems()
                .Sum(item => item.Type.PollutionEmission);

            GameManager.Instance.CO2_Pollution.Value += newPollution;
            seasonEnding?.Invoke();
        }

        public void OnYearEnding(UnityEngine.Object _)
        {
            yearEnding?.Invoke();
        }

        private void RaiseOnCreated(Feature feature)
        {
            feature.Type.ResourceProducers.ForEach(rp => rp.OnCreated());
        }

        private void RaiseOnDestroyed(Feature feature)
        {
            feature.Type.ResourceProducers.ForEach(rp => rp.OnDestroyed());
        }

        private void RegisterStateChangeHandlers(Feature feature)
        {
            foreach (var rp in feature.Type.ResourceProducers)
            {
                var productionSchedule = rp.ProductionSchedule;

                if (productionSchedule.OnTurnEnding != 0)
                {
                    turnEnding += rp.OnTurnEnding;
                }

                if (productionSchedule.OnSeasonEnding != 0)
                {
                    seasonEnding += rp.OnSeasonEnding;
                }

                if (productionSchedule.OnYearEnding != 0)
                {
                    seasonEnding += rp.OnYearEnding;
                }
            }
        }

        private void UnregisterStateChangeHandlers(Feature feature)
        {
            foreach (var rp in feature.Type.ResourceProducers)
            {
                var productionSchedule = rp.ProductionSchedule;

                if (productionSchedule.OnTurnEnding != 0)
                {
                    turnEnding -= rp.OnTurnEnding;
                }

                if (productionSchedule.OnSeasonEnding != 0)
                {
                    seasonEnding -= rp.OnSeasonEnding;
                }

                if (productionSchedule.OnYearEnding != 0)
                {
                    seasonEnding -= rp.OnYearEnding;
                }
            }
        }

        public override void Awake()
        {
            base.Awake();

            playerFeatures.FeatureAdded += RegisterStateChangeHandlers;

            playerFeatures.FeatureAdded += RaiseOnCreated;

            playerFeatures.FeatureRemoved += RaiseOnDestroyed;

            playerFeatures.FeatureRemoved += UnregisterStateChangeHandlers;
        }

        void OnDestroy()
        {
            playerFeatures.FeatureAdded -= RegisterStateChangeHandlers;

            playerFeatures.FeatureAdded -= RaiseOnCreated;

            playerFeatures.FeatureRemoved -= RaiseOnDestroyed;

            playerFeatures.FeatureRemoved -= UnregisterStateChangeHandlers;
        }
    }
}
