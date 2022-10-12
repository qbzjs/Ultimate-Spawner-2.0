using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// Represents a collection of <see cref="SpawnableItem"/> which can be assigned to a <see cref="Spawner"/>. 
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "SpawnableItems", menuName = "Ultimate Spawner/Spawnable Items")]
    public class SpawnableItems : ScriptableObject
    {
        // Events
        /// <summary>
        /// Called when a <see cref="SpawnableItem"/> created by this spawning pool has been destroyed. 
        /// </summary>
        public static event Action<Transform> OnSpawnedItemDestroyed;

        // Private
        private List<SpawnableItem> cachedMaskedItems = new List<SpawnableItem>();

        // Public
        /// <summary>
        /// An array of <see cref="SpawnableItem"/> that can be spawned. 
        /// </summary>
        public SpawnableItem[] items = new SpawnableItem[0];

        // Methods
        /// <summary>
        /// Returns a list of all <see cref="SpawnableItem"/> that are not masked by the specified <see cref="SpawnableMask"/>.  
        /// </summary>
        /// <param name="mask">The maks used to determine which items are spawnable</param>
        /// <returns>A list of all <see cref="SpawnableItem"/> that are not masked</returns>
        public List<SpawnableItem> FindMaskedSpawmableItems(SpawnableMask mask)
        {
            // Clear cached items
            cachedMaskedItems.Clear();
            
            // Check each item
            foreach(SpawnableItem item in items)
            {
                // If we dont have a mask then we always add the item
                if (mask == null)
                {
                    // Make sure the item can be spawned
                    if (item.IsSpawnable == true)
                    {
                        // Add to list
                        cachedMaskedItems.Add(item);
                    }
                }
                else
                {
                    // Check if the item is masked
                    if (mask.IsMasked(item.SpawnableID) == false)
                    {
                        // Make sure the item can be spawned
                        if (item.IsSpawnable == true)
                        {
                            // The item is not masked
                            cachedMaskedItems.Add(item);
                        }
                    }
                }
            }

            // Get the collection
            return cachedMaskedItems;
        }

        /// <summary>
        /// Attempt to find the <see cref="SpawnableItem"/> from the specified <see cref="SpawnableItemRef"/>.  
        /// </summary>
        /// <param name="itemRef">The item reference to resolve</param>
        /// <param name="mask">An optional <see cref="SpawnableMask"/> used to filter out unwanted spawnable items</param>
        /// <returns>A <see cref="SpawnableItem"/> that was resolved from the specified <see cref="SpawnableItemRef"/> or null if it could not be resolved</returns>
        public SpawnableItem FindSpawnableItem(SpawnableItemRef itemRef, SpawnableMask mask = null)
        {
            // Check for error
            if (itemRef == null)
                throw new ArgumentNullException("itemRef");

            // Find all masked items
            List<SpawnableItem> masked = FindMaskedSpawmableItems(mask);

            // Check for no spawnables
            if (masked.Count == 0)
                return null;

            // Try to match the id
            foreach (SpawnableItem item in masked)
            {
                // No asset assigned
                if (item.provider == null || item.provider.IsAssigned == false)
                    continue;

                // Check for item name
                if(itemRef.IsNamed == true)
                {
                    if(item.provider.ItemName == itemRef.Name)
                    {
                        return item;
                    }
                }

                // Check for item tag
                if(itemRef.IsTagged == true)
                {
                    if(string.Compare(item.provider.ItemTag, itemRef.Tag) == 0)
                    {
                        return item;
                    }
                }

                // Check for item id
                if(itemRef.IsID == true)
                {
                    if(item.SpawnableID == itemRef.ID)
                    {
                        return item;
                    }
                }
            }

            // No such item id
            return null;
        }

        /// <summary>
        /// Selects a random <see cref="SpawnableItem"/> using the specified spawn mask.
        /// Spawn chance will also be taken into consideration.
        /// </summary>
        /// <param name="mask">The <see cref="SpawnableMask"/> used to determine which items can be spawned. This value can be null</param>
        /// <returns>A randomly selected <see cref="SpawnableItem"/></returns>
        public SpawnableItem SelectSpawnableItem(SpawnableMask mask = null)
        {
            // Find all masked items
            List<SpawnableItem> masked = FindMaskedSpawmableItems(mask);

            // Check for no spawnable items
            if(masked.Count == 0)
                return null;

            // Calculate the spawn chance
            float accumulator = 0;

            // Add the spawn chance for each item
            foreach (SpawnableItem item in masked)
                accumulator += item.spawnChance;

            // Select a random value
            float value = Random.Range(0, accumulator);

            // reset the accumulator for reuse
            accumulator = 0;

            // Find the spawnable 
            foreach(SpawnableItem item in masked)
            {
                // Add to accumulator
                accumulator += item.spawnChance;

                // We have found the spawnable item based on the spawn chance
                if (value < accumulator)
                    return item;
            }

            // Go to default spawnable
            return masked[0];
        }

        /// <summary>
        /// Attempt to create a spawnable item from this item pool.
        /// </summary>
        /// <param name="itemRef">An optional <see cref="SpawnableItemRef"/> used to specify the spawnable item that should be created</param>
        /// <param name="mask">An optional <see cref="SpawnableMask"/> representing spawnable items that should be ignored</param>
        /// <returns>The <see cref="Transform"/> of the spawned item or null if an error occurred</returns>
        public Transform CreateSpawnableItem(SpawnableItemRef itemRef = null, SpawnableMask mask = null, SpawnLocation spawnLocation = default(SpawnLocation), SpawnRotationApplyMode applyRotation = SpawnRotationApplyMode.FullRotation)
        {
            SpawnableItem item = null;

            // Check for an item refernence
            if (itemRef != null)
            {
                item = FindSpawnableItem(itemRef, mask);

                if (item == null)
                    UnityEngine.Debug.LogWarning("Failed to spawn target spawnable - falling back to random spawning");
            }

            // Check if we found the item
            if (item == null)
                item = SelectSpawnableItem(mask);

            // Create an insatnce of the spawnable item
            return CreateSpawnableInstance(item, spawnLocation, applyRotation);
        }

        /// <summary>
        /// Informas all <see cref="SpawnableItems"/> that a spawned item has been destroyed and it should not be tracked anymore.
        /// </summary>
        /// <param name="destroyedTransform">The <see cref="Transform"/> of the spawned item that has been destroyed</param>
        public static void InformSpawnableDestroyed(Transform destroyedTransform)
        {
            // Trigger the event
            if (OnSpawnedItemDestroyed != null)
                OnSpawnedItemDestroyed(destroyedTransform);
        }

        private Transform CreateSpawnableInstance(SpawnableItem item, SpawnLocation spawnLocation = default(SpawnLocation), SpawnRotationApplyMode applyRotation = SpawnRotationApplyMode.FullRotation)
        {
            // Check for error
            if (item == null || item.provider == null)
                return null;

            // Call the provider to create the spawnable instance
            GameObject spawnable = item.provider.CreateSpawnableInstance(spawnLocation, applyRotation) as GameObject;

            // Give the item an identity so we can be informed when it is destroyed
            SpawnableIdentity itemIdentity = SpawnableIdentity.AddObjectIdentity(spawnable, this, item);

            // Register with the spawned item pool
            item.SpawnedItemPool.AddSpawnedItem(itemIdentity);

            return spawnable.transform;
        }
    }
}
