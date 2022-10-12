using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UltimateSpawner.Despawning;
using UnityEngine;

[assembly : InternalsVisibleTo("Assembly-CSharp-Editor")]

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// The base class for all spawning components.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class Spawner : MonoBehaviour, IEnumerable<Spawner>
    {
        // Class
        /// <summary>
        /// Attribute used to mark certain <see cref="Spawner"/> fields or properties to be displayed in the inspector info section. 
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public sealed class SpawnerInfoAttribute : Attribute
        {
            // Public
            /// <summary>
            /// The inspector name used when displaying the information
            /// </summary>
            public string displayName = null;

            // Constructor
            /// <summary>
            /// Create a new instance.
            /// </summary>
            /// <param name="displayName">The inspector name used when displaying the information or null if the field or property name should be used</param>
            public SpawnerInfoAttribute(string displayName = null)
            {
                this.displayName = displayName;
            }
        }


        // Internal
        internal const string errorNoItems = "Spawner ({0}): Failed to create spawnable item - No items assigned";
        internal const string errorNoItemsDefined = "Spawner ({0}): Failed to created spawnable item - No spawnabled items defined. Are all items masked?";

#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        internal bool despawnersExpanded = false;
#endif

        // Private
        private Spawner parent = null;
        private List<Despawner> despawners = new List<Despawner>();

        // Public
        /// <summary>
        /// The <see cref="Spawning.SpawnableItems"/> assigned to this spawner which will be used to create an instance of a <see cref="SpawnableItem"/>.
        /// This value can be null if the items should be inherited from a parent <see cref="Spawner"/>.
        /// </summary>
        [HideInInspector]
        [Tooltip("A spawnable items asset which contains all spawnables that could be spawned by this spawner")]
        public SpawnableItems spawnItems = null;
        
        /// <summary>
        /// The <see cref="Spawning.SpawnableMask"/> assigned to this spawner which is used to determine which items can be spawned by this <see cref="Spawner"/>. 
        /// If this <see cref="Spawner"/> has a parent then the parents mask will be combined so that this <see cref="Spawner"/> cannot spawn items that its parent cant.  
        /// </summary>
        [HideInInspector]
        [Tooltip("A spawnable mask is used to specify which items in the spawn items can be spawned at this spawner. Using this mask it is possible to create spawners that only spawn a single item even if there are multiple items in the collection")]
        public SpawnableMask spawnMask = null;

        // Properties
        /// <summary>
        /// The parent <see cref="Spawner"/> of this <see cref="Spawner"/> or null if this is the root.  
        /// </summary>
        public virtual Spawner Parent
        {
            get
            {
                // Always update the parent in editor as there can be hierarchy changes at any time
                if (Application.isPlaying == false)
                    UpdateParent();

                return parent;
            }
        }

        /// <summary>
        /// Get the <see cref="Spawning.SpawnableItems"/> assigned to this <see cref="Spawner"/>. 
        /// </summary>
        public virtual SpawnableItems SpawnableItems
        {
            get
            {
                // Make sure we have a parent
                if (Parent != null)
                {
                    // Get the spawnable items for the parent
                    return Parent.SpawnableItems;
                }

                // Get the assigned items
                return spawnItems;
            }
        }

        /// <summary>
        /// Get the <see cref="Spawning.SpawnableMask"/> assigned to this <see cref="Spawner"/>.  
        /// </summary>
        public virtual SpawnableMask SpawnableMask
        {
            get
            {
                SpawnableMask mask = spawnMask;

                // Check for invalid mask
                if (mask == null)
                    mask = new SpawnableMask();

                // Combine mask with parent
                if (Parent != null)
                    mask = mask.Combine(Parent.SpawnableMask);

                return mask;
            }
        }

        public IList<Despawner> Despawners
        {
            get
            {
                // Reset list
                despawners.Clear();

                // Find components
                GetComponents(despawners);

                return despawners;
            }
        }

        /// <summary>
        /// Returns true if the <see cref="Spawner"/> is able to spawn an object or false if there are no available spawn locations. 
        /// </summary>
        [SpawnerInfo("Available")]
        public abstract bool IsAvailable { get; }

        /// <summary>
        /// Returns the number of items that can potentially be spawned at this <see cref="Spawner"/>. 
        /// Note that this property does not take into account whether or not all locations are available. 
        /// </summary>
        [SpawnerInfo("Max Capacity")]
        public abstract int SpawnableItemCapacity { get; }
                
        /// <summary>
        /// Returns the number of items that can be spawned at this <see cref="Spawner"/> on the current frame.
        /// Note that this property takes into account the availablity of the <see cref="Spawner"/>. 
        /// </summary>
        public abstract int AvailableSpawnableItemCapacity { get; }

#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity when the spawner is created or cloned.
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void OnValidate()
        {
            // Get the parent spawn mask for hierarchy masking
            if(Parent != null)
                spawnMask.Parent = Parent.spawnMask;

            // Set the spawnable items
            spawnMask.SpawnableItems = SpawnableItems;
        }
#endif

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void Awake()
        {
            // Find the parent of this spawner if there is one
            UpdateParent();
        }

        /// <summary>
        /// Attempts to create a <see cref="SpawnableItem"/> object at this <see cref="Spawner"/> location.  
        /// </summary>
        /// <returns>The <see cref="Transform"/> component of the newly created <see cref="SpawnableItem"/></returns>
        public Transform CreateSpawnableItem(SpawnableItemRef itemRef = null, SpawnLocation spawnLocation = default(SpawnLocation), SpawnRotationApplyMode applyRotation = SpawnRotationApplyMode.FullRotation)
        {
            // Get the spawnable items
            SpawnableItems items = SpawnableItems;

            // Check for error
            if(items == null)
            {
                Debug.LogWarning(string.Format(errorNoItems, name));
                return null;
            }

            // Create the item using the mask
            Transform spawned = UltimateSpawning.Spawn(items, itemRef, SpawnableMask, spawnLocation, applyRotation);

            // Check for error
            if(spawned == null)
            {
                Debug.LogWarning(string.Format(errorNoItemsDefined, name));
                return null;
            }

            return spawned;
        }

        public void ApplyDespawners(Transform applyTo)
        {
            foreach(Despawner despawner in Despawners)
            {
                if(despawner.ApplyDespawnerToSpawnedItems == true)
                {
                    // Create the despawner clone on the target object
                    Despawner despawnerInstance = applyTo.gameObject.AddComponent(despawner.GetType()) as Despawner;

                    // Copy despawner values
                    despawnerInstance.CloneFrom(despawner);
                }
            }
        }

        /// <summary>
        /// Attempts to create a new spawnable item and spawn it using this <see cref="Spawner"/> settings. 
        /// </summary>
        /// <param name="itemRef">An item reference of the spawnable item that should be spawned if possible. This item can only be spawned if it is not masked and can be resolved. Use null to select a spawnable item randomly using the spawn chance value</param>
        /// <returns>The transofmr of the spawned item or null if the spawn failed</returns>
        public abstract Transform Spawn(SpawnableItemRef itemRef = null);

        /// <summary>
        /// Attempts to spawn an object.
        /// </summary>
        /// <param name="toSpawn">The transform of the object to spawn</param>
        /// <returns>True if the spawn is sucessful or false if there are no available spawn locations</returns>
        public abstract bool Spawn(Transform toSpawn);

        /// <summary>
        /// Get a <see cref="SpawnLocation"/> for this spawner. 
        /// If the spawner is not setup or does not have a <see cref="SpawnLocation"/> available then <see cref="SpawnLocation.invalid"/> will be returned.  
        /// </summary>
        /// <returns>An instance of <see cref="SpawnLocation"/> representing the world location of the spawner</returns>
        public abstract SpawnLocation GetLocation();

        /// <summary>
        /// Enumerate all child spawners of this <see cref="Spawner"/>. 
        /// </summary>
        /// <returns>An enumerator for all child spawners</returns>
        public abstract IEnumerator<Spawner> GetEnumerator();

        /// <summary>
        /// Updates the parent spawner if any changes have been made to the spawner hierarchy.
        /// If the <see cref="Spawner"/> no longer has a parent then the <see cref="Parent"/> property will become null.
        /// </summary>
        public void UpdateParent()
        {
            // Check for parent
            if (transform.parent != null)
            {
                // Get the parent spawner component
                parent = transform.parent.GetComponent<Spawner>();
            }
            else
            {
                // Allow unassign - for editor
                parent = null;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Attempt to find a <see cref="Spawner"/> in the current spawn hierarchy with the specified name. 
        /// </summary>
        /// <param name="name">The name that the <see cref="Spawner"/> should have</param>
        /// <returns>The first <see cref="Spawner"/> found with the matching name or null if no spawner could be found</returns>
        public Spawner FindSpawnerWithName(string name)
        {
            // Check for trivial case
            if (this.name == name)
                return this;

            // Create a state stack
            Stack<Spawner> state = new Stack<Spawner>();
            state.Push(this);

            while(state.Count > 0)
            {
                // Get the next spawner
                Spawner current = state.Pop();

                // Check for end condition
                if (current.name == name)
                    return current;

                // Push child spawners
                foreach (Spawner child in current)
                    state.Push(child);
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Attempt to find a <see cref="Spawner"/> in the current spawn hierarchy with the specified tag. 
        /// </summary>
        /// <param name="tag">The tag that the <see cref="Spawner"/> should have</param>
        /// <returns>The first <see cref="Spawner"/> found with the matching tag or null if no spawner could be found</returns>
        public Spawner FindSpawnerWithTag(string tag)
        {
            // Check for trivial case
            if (this.CompareTag(tag) == true)
                return this;

            // Create a state stack
            Stack<Spawner> state = new Stack<Spawner>();
            state.Push(this);

            while (state.Count > 0)
            {
                // Get the next spawner
                Spawner current = state.Pop();

                // Check for end condition
                if (current.CompareTag(tag) == true)
                    return current;

                // Push child spawners
                foreach (Spawner child in current)
                    state.Push(child);
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Attempt to find a <see cref="Spawner"/> in the current spawn hierarchy with the specified id. 
        /// </summary>
        /// <param name="id">The id that the <see cref="Spawner"/> should have</param>
        /// <returns>The first <see cref="Spawner"/> found with the matching id or null if no spawner could be found</returns>
        public Spawner FindSpawnerWithID(int id)
        {
            // Check for trivial case
            if (id == 0)
                return this;

            // Create a state stack
            Stack<Spawner> state = new Stack<Spawner>();
            state.Push(this);

            int index = 0;

            while (state.Count > 0)
            {
                // Get the next spawner
                Spawner current = state.Pop();

                // Check for end condition
                if (index == id)
                    return current;

                // Push child spawners
                foreach (Spawner child in current)
                    state.Push(child);

                // Increment index
                index++;
            }

            // Not found
            return null;
        }
    }
}
