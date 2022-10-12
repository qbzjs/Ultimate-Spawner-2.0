using System.Collections;
using UltimateSpawner.Spawning;

namespace UltimateSpawner.Despawning
{
    public class DespawnAfterTime : Despawner
    {
        // Private
        private bool waitForSpawn = true;

        // Public
        public float despawnAfterTime = 5f;

        // Methods
        public IEnumerator Start()
        {
            // Spawners cannot run despawners on the same object only on spawned objects
            if (ShouldAllowDespawn == false)
                yield break;

            // Check for spawnable item
            if (GetComponent<SpawnableIdentity>() == null)
                waitForSpawn = false;

            // Wait for spawn
            while (waitForSpawn == true)
                yield return null;

            // Wait for time to pass
            yield return new WaitForSecondsNonAlloc(despawnAfterTime);

            // Set despawn condition
            MarkDespawnConditionAsMet();

            // Despawn this object
            Despawn();
        }

        public override void OnSpawned(SpawnLocation location)
        {
            base.OnSpawned(location);

            // We have now spawned
            waitForSpawn = false;
        }

        public override void CloneFrom(Despawner cloneFrom)
        {
            DespawnAfterTime despawner = cloneFrom as DespawnAfterTime;

            if(despawner != null)
            {
                // Copy fields
                despawnAfterTime = despawner.despawnAfterTime;
            }
        }
    }
}
