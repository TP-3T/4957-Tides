using UnityEngine;

namespace PlayerResources
{
    [CreateAssetMenu(
        fileName = "ResourceProducer",
        menuName = "Scriptable Objects/Resources/ResourceProducer"
    )]
    public class ResourceProducer : ScriptableObject
    {
        [Tooltip("The resource to be produced.")]
        [SerializeField]
        private PlayerResource resource;

        [Tooltip("The amount to be produced.")]
        [SerializeField]
        private int amount;

        /// <summary>
        /// Produces some resources and adds to the player's resource count.
        /// </summary>
        /// <param name="multiplier">The amount produced is first multiplied by this number</param>
        public void Produce(float multiplier = 1.0f)
        {
            resource.ApplyChange((int)(amount * multiplier));
        }
    }
}
