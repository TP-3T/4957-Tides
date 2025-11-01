using UnityEngine;

namespace TTT.PlayerResources
{
    /// <summary>
    /// Base Resource Producer class to get boilerplate code out of the way.
    /// </summary>
    public abstract class ProducerBase : ScriptableObject, IResourceProducer
    {
        [Tooltip("The resource being produced.")]
        [SerializeField]
        private PlayerResource resource;

        [Tooltip("The amount to be produced")]
        [SerializeField]
        private int resourceAmount;

        public PlayerResource Resource => resource;

        public int ResourceAmount => resourceAmount;

        public void ProduceResource(int amount)
        {
            resource.ApplyChange(amount);
        }

        public virtual void OnCreated()
        {
            // override if you want to produce when a building is created
        }

        public virtual void OnDestroyed()
        {
            // override if you want to produce when a building is destroyed
        }

        public virtual void OnSold()
        {
            // override if you want to produce when a building is sold by the player
        }

        public virtual void OnTurnEnding()
        {
            // override if you want to produce resources every turn
        }
    }
}
