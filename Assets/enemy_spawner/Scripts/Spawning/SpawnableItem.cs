using System;
using UnityEngine;

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// Represents a reference to a spawnable item.
    /// </summary>
    public sealed class SpawnableItemRef
    {
        // Private
        private string name = null;
        private string tag = null;
        private int id = -1;

        // Properties
        /// <summary>
        /// Get the name of the spawnable item that should be referenced.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Get the tag of the spawnable item that should be referenced.
        /// </summary>
        public string Tag
        {
            get { return tag; }
        }

        /// <summary>
        /// Get the id of the spawnable item that should be referenced.
        /// </summary>
        public int ID
        {
            get { return id; }
        }

        /// <summary>
        /// Check if this reference has a name.
        /// </summary>
        public bool IsNamed
        {
            get { return name != null; }
        }

        /// <summary>
        /// Check if this reference has a tag.
        /// </summary>
        public bool IsTagged
        {
            get { return tag != null; }
        }

        /// <summary>
        /// Check if this reference has an id.
        /// </summary>
        public bool IsID
        {
            get { return id != -1; }
        }

        /// <summary>
        /// Check if this reference is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (IsNamed == true)
                    return string.IsNullOrEmpty(name) == false;
                else if (IsTagged == true)
                    return string.IsNullOrEmpty(tag) == false;
                else
                    return id != -1;
            }
        }

        // Constructor
        private SpawnableItemRef() { }

        // Methods
        /// <summary>
        /// Create a <see cref="SpawnableItemRef"/> by referening a spawnable item by name. 
        /// </summary>
        /// <param name="spawnableName">The name of the spawnable item</param>
        /// <returns>A reference to a spawnable item with the specified name</returns>
        public static SpawnableItemRef WithName(string spawnableName)
        {
            return new SpawnableItemRef
            {
                name = spawnableName,
            };
        }

        /// <summary>
        /// Create a <see cref="SpawnableItemRef"/> by referencing a spawnable item by tag. 
        /// </summary>
        /// <param name="spawnableTag">The tag of the spawnable item</param>
        /// <returns>A reference to a spawnable item with the specified tag</returns>
        public static SpawnableItemRef WithTag(string spawnableTag)
        {
            return new SpawnableItemRef
            {
                tag = spawnableTag,
            };
        }

        /// <summary>
        /// Create a <see cref="SpawnableItemRef"/> by referencing a spawnable item by id. 
        /// </summary>
        /// <param name="spawnableID">The id of the spawnable item</param>
        /// <returns>A reference to a spawnable item with the specified id</returns>
        public static SpawnableItemRef WithID(int spawnableID)
        {
            return new SpawnableItemRef
            {
                id = spawnableID,
            };
        }
    }

    /// <summary>
    /// Represents an object that can be spawned at a <see cref="SpawnLocation"/>. 
    /// <seealso cref="SpawnableItems"/>.
    /// </summary>
    [Serializable]
    public class SpawnableItem
    {
        // Private
        private SpawnedPool spawnedPool = null;

        [HideInInspector]
        [SerializeField]
        private int spawnableID = -1;

        // Public
        /// <summary>
        /// The prefab associated with this <see cref="SpawnableItem"/>. 
        /// </summary>
        //[Tooltip("The prefab associated with this item")]
        //public GameObject prefab;

        public SpawnableItemProvider provider;
        
        /// <summary>
        /// How likley this <see cref="SpawnableItem"/> is to spawn.
        /// The higher the spawn chance, the more likley.
        /// </summary>
        [Tooltip("How likley the item is to spawn (Higher values are more likley)")]
        public float spawnChance = 0.5f;

        // Properties
        /// <summary>
        /// Get the unique spawnable id for this <see cref="SpawnableItem"/>.
        /// This is assigned automatically at edit time and is guarenteed to be unique.
        /// </summary>
        public int SpawnableID
        {
            get { return spawnableID; }
        }

        /// <summary>
        /// Returns true if the item is valid and can be spawned.
        /// In order to be true, a prefab must be assigned and the spawn chance must be greater than 0.
        /// </summary>
        public bool IsSpawnable
        {
            get
            {
                // Make sure we have a parefab and spawn chance
                if (provider == null ||
                    provider.IsAssigned == false ||
                    spawnChance == 0)
                    return false;

                // The item can be spawned
                return true;
            }
        }

        public SpawnedPool SpawnedItemPool
        {
            get
            {
                // Create pool if required
                if(spawnedPool == null)
                    spawnedPool = new SpawnedPool(this);

                return spawnedPool;
            }
        }

        // Constructor
        /// <summary>
        /// Create a new <see cref="SpawnableItem"/> with the specifid id. 
        /// </summary>
        /// <param name="id">The unique id that the new item should use</param>
        public SpawnableItem(int id)
        {
            // Store the id
            this.spawnableID = id;
        }

        // Methods
        /// <summary>
        /// Check if a spawnable item exists with the specified id.
        /// </summary>
        /// <param name="id">The id to check for</param>
        /// <returns>True if a spawnable item with the specified id was found or false if not</returns>
        public bool HasID(int id)
        {
            return spawnableID == id;
        }

        /// <summary>
        /// Check if a spawnable item exists with the specified name.
        /// </summary>
        /// <param name="name">The name to check for</param>
        /// <returns>True if a spawnable item with the specified name was found or false if not</returns>
        public bool HasName(string name)
        {
            // Check for valid prefab
            if (provider == null || provider.IsAssigned == false)
                return false;

            // CHeck prefab name
            return string.Compare(provider.ItemName, name) == 0;
        }

        /// <summary>
        /// Check if a spawnable item exists with the specified tag.
        /// </summary>
        /// <param name="tag">The tag to check for</param>
        /// <returns>True if a spawnable item with the specified tag was found or false if not</returns>
        public bool HasTag(string tag)
        {
            // Check for valid prefab
            if (provider == null || provider.IsAssigned == false)
                return false;

            // CHeck prefab name
            return string.Compare(provider.ItemTag, tag) == 0;
        }

        /// <summary>
        /// Override method.
        /// </summary>
        /// <returns>String representation of this item</returns>
        public override string ToString()
        {
            // Get the prefab name
            string name = (provider == null || provider.IsAssigned == false) ? "null" : provider.ItemName;

            // Format the string
            return string.Format("SpawnableItem({0}, {1}%)", name, spawnChance);
        }
    }
}
