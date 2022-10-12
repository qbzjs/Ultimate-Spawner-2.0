
namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// Event interface that can be implemented by script components attached to a spawnable game object.
    /// When the parent object is spawned its event method will be triggered.
    /// </summary>
    public interface ISpawnEventReceiver
    {
        // Methods
        /// <summary>
        /// Called when the parent object has been spawned into the scene via Ultimate Spawner.
        /// </summary>
        void OnSpawned(SpawnLocation location);
    }
}
