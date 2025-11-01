namespace TTT.PlayerResources
{
    /// <summary>
    /// Interface that defines methods for resource producers.
    /// The method names indicate what game events might cause those methods to be called.
    /// Should also support producing negative resources (spending resources).
    /// </summary>
    public interface IResourceProducer
    {
        PlayerResource Resource { get; }

        int ResourceAmount { get; }

        /// <summary>
        /// Adds to the resource's total count.
        /// </summary>
        /// <param name="amount">The amount to add.</param>
        void ProduceResource(int amount);

        /// <summary>
        /// Called when the resource producer is first created in the game map.
        /// For example, when a player places a building.
        /// </summary>
        void OnCreated();

        /// <summary>
        /// Called at the end of every turn after the producer is created.
        /// </summary>
        void OnTurnEnding();

        /// <summary>
        /// Called when the producer is destroyed in a disaster or similar.
        /// For example, when a building is removed due to flooding.
        /// </summary>
        void OnDestroyed();

        /// <summary>
        /// Called when the producer is removed willingly by the player.
        /// For example, when a building is sold.
        /// </summary>
        void OnSold();
    }
}
