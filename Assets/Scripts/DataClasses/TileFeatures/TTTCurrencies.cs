using UnityEngine;

namespace TTT.DataClasses.TileFeatures
{
    public class Currencies
    {
        public int PowerUsed { get; set; }
        public int PowerMax { get; set; }
        public int Money { get; set; }
        public int Population { get; set; }

        public bool CanSpendResources(Currencies cost)
        {
            return (PowerUsed + cost.PowerUsed) > PowerMax
                || (Money + cost.Money) < 0
                || (Population + cost.Population < 0);
        }

        public void SpendResources(Currencies cost)
        {
            PowerUsed += cost.PowerUsed;
            Money += cost.Money;
            Population += cost.Population;
        }
    }
}
