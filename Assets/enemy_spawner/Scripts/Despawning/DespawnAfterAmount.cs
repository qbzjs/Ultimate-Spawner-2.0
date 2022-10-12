using System;
using UltimateSpawner.Spawning;

namespace UltimateSpawner.Despawning
{
    public class DespawnAfterAmount : Despawner
    {
        // Public
        public int maxAllowedCount = 10;

        // Methods
        public void Update()
        {
            if(spawnItemIdentity != null)
            {
                // Check for maximum exceeded
                if(spawnItemIdentity.SpawnMaster.SpawnedItemPool.SpawnedCount > maxAllowedCount)
                {
                    // Try to get the first instance
                    SpawnableIdentity firstSpawned = spawnItemIdentity.SpawnMaster.SpawnedItemPool.FirstSpawnableItemInstance;

                    // Check if this object was created the longest time ago and handle destruction. 
                    // Note that other DespawnAfterComponents could be checking the same pool so we need to check 'this==' so that despawning only occurs once
                    if(firstSpawned != null && firstSpawned == spawnItemIdentity)
                    {
                        // Set despawn condition
                        MarkDespawnConditionAsMet();

                        // Despawn this object
                        Despawn();
                    }
                }
            }
        }

        public override void CloneFrom(Despawner cloneFrom)
        {
            DespawnAfterAmount despawner = cloneFrom as DespawnAfterAmount;

            if (despawner != null)
            {              
                maxAllowedCount = despawner.maxAllowedCount;
            }
        }
    }
}
