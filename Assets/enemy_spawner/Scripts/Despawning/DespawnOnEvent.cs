
namespace UltimateSpawner.Despawning
{
    public class DespawnOnEvent : Despawner
    {
        // Methods
        /// <summary>
        /// Cause this spawnable item to be despawned.
        /// You can call this method from Unity events or animation events to trigger despawns at specific times.
        /// </summary>
        public void DespawnItem()
        {
            // Check if despawn is allowed
            if (ShouldAllowDespawn == false)
                return;

            // Set despawn condition
            MarkDespawnConditionAsMet();

            Despawn();
        }

        public override void CloneFrom(Despawner cloneFrom)
        {
            // Do nothing
        }
    }
}
