using System;
using System.Collections.Generic;
using UltimateSpawner.Spawning;

namespace UltimateSpawner.Spawning
{
    public sealed class SpawnedPool
    {
        // Private
        private SpawnableItem spawnItem = null;
        private List<SpawnableIdentity> spawnedItemInstances = new List<SpawnableIdentity>();

        // Properties
        public SpawnableIdentity FirstSpawnableItemInstance
        {
            get { return (spawnedItemInstances.Count > 0) ? spawnedItemInstances[0] : null; }
        }

        public IList<SpawnableIdentity> SpawnedItemInstances
        {
            get { return spawnedItemInstances; }
        }

        public int SpawnedCount
        {
            get { return spawnedItemInstances.Count; }
        }

        // Constructor
        public SpawnedPool(SpawnableItem spawnItem)
        {
            this.spawnItem = spawnItem;
        }

        // Methods
        public void AddSpawnedItem(SpawnableIdentity identity)
        {
            if (identity.SpawnMaster != spawnItem)
                throw new InvalidOperationException("Attempted to add a spawned instance which does not match the spawn item master type");

            spawnedItemInstances.Add(identity);
        }

        public void RemoveSpawnedItem(SpawnableIdentity identity)
        {
            if (spawnedItemInstances.Contains(identity) == true)
                spawnedItemInstances.Remove(identity);
        }
    }
}
