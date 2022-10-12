using System;
using UltimateSpawner.Despawning;
using UnityEngine;
using UnityEngine.AI;

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// A <see cref="SpawnableIdentity"/> is attached to all spawned items and informs <see cref="SpawnableItems"/> when it has been destroyed. 
    /// </summary>
    public sealed class SpawnableIdentity : MonoBehaviour, ISpawnEventReceiver, IDespawnEventReceiver
    {
        // Private
        private SpawnableItems spawnPool = null;
        private SpawnableItem spawnMaster = null;
        private bool destroyFlag = false;

        // Properties
        /// <summary>
        /// Get the <see cref="SpawnableItems"/> that spawned this item.
        /// This value will return null if this item has outlived the <see cref="SpawnableItems"/> object that created it.  
        /// </summary>
        public SpawnableItems SpawnPool
        {
            get { return spawnPool; }
        }

        public SpawnableItem SpawnMaster
        {
            get { return spawnMaster; }
        }

        public SpawnableItemProvider SpawnProvider
        {
            get { return spawnMaster.provider; }
        }

        public int SpawnItemID
        {
            get { return spawnMaster.SpawnableID; }
        }

        /// <summary>
        /// Returns true is this spawnable item has been marked as destroyed during this frame.
        /// This value will only be true for a single frame after which the objetc will be removed from the scene.
        /// </summary>
        public bool IsDestroyed
        {
            get { return destroyFlag; }
        }

        // Methods
        /// <summary>
        /// Called when this spawnable item has been spawned.
        /// </summary>
        public void OnSpawned(SpawnLocation location)
        {
            // Check for ai component
            NavMeshAgent agent = GetComponent<NavMeshAgent>();

            if(agent != null)
            {
                // Make sure the nav mesh agent is at the correct location
                agent.Warp(location.SpawnPosition);
            }

            destroyFlag = false;
        }

        /// <summary>
        /// Called when this item is about to be despawned
        /// </summary>
        public void OnDespawned()
        {
            if(destroyFlag == false)
            {
                // Remove from pool
                spawnMaster.SpawnedItemPool.RemoveSpawnedItem(this);

                // Inform that this object is about to be destroyed
                SpawnableItems.InformSpawnableDestroyed(transform);
                destroyFlag = true;
            }
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void OnDestroy()
        {            
            if (destroyFlag == false)
            {
                // Remove from pool
                spawnMaster.SpawnedItemPool.RemoveSpawnedItem(this);

                // Inform that this object is about to be destroyed
                SpawnableItems.InformSpawnableDestroyed(transform);
                destroyFlag = true;
            }
        }

        /// <summary>
        /// Attempts to assign a new <see cref="SpawnableIdentity"/> to the specified game object.
        /// If the specified object already has a valid identity then this method will simply return that attached identity.
        /// </summary>
        /// <param name="go">The object to give an identity to</param>
        /// <param name="owner">The <see cref="SpawnableItems"/> that created the specified object</param>
        /// <returns>A valid <see cref="SpawnableIdentity"/></returns>
        public static SpawnableIdentity AddObjectIdentity(GameObject go, SpawnableItems owner, SpawnableItem item)
        {
            // Check for null object
            if (go == null)
                throw new ArgumentNullException("go");

            // Check if the object already has a valid identity
            SpawnableIdentity result = go.GetComponent<SpawnableIdentity>();

            // Check for a valid script
            if (result != null)
            {
                // Be sure to update the owning items 
                if (result.spawnPool == null)
                    result.spawnPool = owner;

                result.spawnMaster = item;

                return result;
            }

            // Create a script
            result = go.AddComponent<SpawnableIdentity>();
            result.spawnPool = owner;
            result.spawnMaster = item;

            return result;
        }
    }
}
