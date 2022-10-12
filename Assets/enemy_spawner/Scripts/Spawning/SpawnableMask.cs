using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// A spawnable mask is used to determine which items in the <see cref="Spawning.SpawnableItems"/> can be used.  
    /// </summary>
    [Serializable]
    public sealed class SpawnableMask
    {
        // Private 
        private SpawnableItems items = null;
        private SpawnableMask parent = null;

        [SerializeField]
        private bool noMask = true;

        [SerializeField]
        private List<int> maskedItems = new List<int>(); // Masked items cannot be spawned

        // Properties
        /// <summary>
        /// The <see cref="Spawning.SpawnableItems"/> that this mask uses to identify the possible spawnable items.  
        /// </summary>
        public SpawnableItems SpawnableItems
        {
            get { return items; }
            set
            {
                items = value;

                // Check if we should clear the mask
                if(value == null)
                    ClearMask();
            }
        }

        /// <summary>
        /// Get the parents <see cref="SpawnableMask"/>. 
        /// </summary>
        public SpawnableMask Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        /// <summary>
        /// Returns true if none of the <see cref="SpawnableItem"/> are masked. 
        /// </summary>
        public bool NoMask
        {
            get { return noMask; }
            set { noMask = value; }
        }

        // Methods
        /// <summary>
        /// Add an item with the specified if to this mask.
        /// </summary>
        /// <param name="spawnableID">The item id to unmask</param>
        public void MaskItem(int spawnableID)
        {
            // Mask item if it is not already
            if(maskedItems.Contains(spawnableID) == false)
                maskedItems.Add(spawnableID);
        }

        /// <summary>
        /// Remove an item with the specified id from this mask.
        /// </summary>
        /// <param name="spawnableID">The item id to unmask</param>
        public void UnmaskItem(int spawnableID)
        {
            // Unmask item if it is masked
            if (maskedItems.Contains(spawnableID) == true)
                maskedItems.Remove(spawnableID);
        }

        /// <summary>
        /// Clears all masked items.
        /// </summary>
        public void ClearMask()
        {
            noMask = true;
            maskedItems.Clear();
        }

        /// <summary>
        /// Checks whether the specified spawnable id is masked.
        /// If the item is considered masked then it will not be used by the owning spawner.
        /// </summary>
        /// <param name="spawnableID">The spawnable id to check</param>
        /// <returns>True if the item is masked or false if it is not</returns>
        public bool IsMasked(int spawnableID)
        {
            // CHeck for masked id
            return maskedItems.Contains(spawnableID);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Checks whether all of the spawnable items are masked.
        /// If all items are masked then no items will be available for spawning.
        /// This method is only available at edit time.
        /// </summary>
        /// <returns></returns>
        public bool IsAllMasked()
        {
            if (items != null)
            {
                // Make sure there are no un-masked items
                foreach (SpawnableItem item in items.items)
                    if(item.provider != null && item.provider.IsAssigned == true)
                        if (IsMasked(item.SpawnableID) == false)
                            return false;

                // We have not returned so all items must be masked
                return true;
            }
            // Default error value is false
            return false;            
        }
#endif

        /// <summary>
        /// Combines all masked items with the masked items in the specified mask.
        /// </summary>
        /// <param name="other">The other <see cref="SpawnableMask"/> to combine</param>
        /// <returns>A new <see cref="SpawnableMask"/> representing both combined masks</returns>
        public SpawnableMask Combine(SpawnableMask other)
        {
            SpawnableMask result = new SpawnableMask();

            foreach (int id in maskedItems)
                result.MaskItem(id);

            foreach (int id in other.maskedItems)
                result.MaskItem(id);

            return result;
        }
    }
}
