using UnityEngine;

namespace PlayerResources
{
    /// <summary>
    /// Produces a resource every turn.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AdditiveProducer",
        menuName = "Scriptable Objects/Player Resources/AdditiveProducer"
    )]
    public class AdditiveProducer : ProducerBase
    {
        public override void OnTurnEnding()
        {
            ProduceResource(ResourceAmount);
        }
    }
}
