using System;
using UnityEngine;

namespace UltimateSpawner.Spawning
{
    [Serializable]
    public class PrefabSpawnableItemProvider : SpawnableItemProvider
    {
        // Public
        [SerializeField]
        public GameObject prefab;

        // Properties
        public override bool IsAssigned
        {
            get { return prefab != null; }
        }

        public override string ItemName
        {
            get
            {
                if (prefab == null)
                    return "None";

                return prefab.name;
            }
        }

        public override string ItemTag
        {
            get
            {
                if (prefab == null)
                    return base.ItemTag;

                // Get the object tag
                return prefab.tag;
            }
        }

        // Methods
        public override UnityEngine.Object CreateSpawnableInstance(SpawnLocation spawnLocation, SpawnRotationApplyMode applyRotation)
        {
            // Check for prefab
            if (prefab == null)
                return null;

            // Try to spawn the item
            return UltimateSpawning.UltimateSpawnerInstantiate(prefab, spawnLocation.SpawnPosition, spawnLocation.GetSpawnRotation(applyRotation));
        }

        public override void DestroySpawnableInstance(UnityEngine.Object spawnableInstance)
        {
            // Try to destroy the item
            UltimateSpawning.UltimateSpawnerDestroy(spawnableInstance);
        }
    }
}
