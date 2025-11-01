using UnityEngine;

namespace PlayerResources
{
    /// <summary>
    /// Produces a resource once, on creation.
    /// </summary>
    [CreateAssetMenu(
        fileName = "StaticProducer",
        menuName = "Scriptable Objects/Player Resources/StaticProducer"
    )]
    public class StaticProducer : ProducerBase
    {
        public override void OnCreated()
        {
            ProduceResource(ResourceAmount);
        }

        public override void OnDestroyed()
        {
            ProduceResource(-ResourceAmount);
        }

        public override void OnSold()
        {
            ProduceResource(-ResourceAmount);
        }
    }
}
