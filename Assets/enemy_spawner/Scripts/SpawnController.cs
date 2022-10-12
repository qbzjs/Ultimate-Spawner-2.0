using UnityEngine;
using UltimateSpawner.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UltimateSpawner
{
    /// <summary>
    /// Represents an item that has been spawned by a specific <see cref="Spawner"/>. 
    /// </summary>
    public struct SpawnedItem
    {
        // Public
        /// <summary>
        /// The <see cref="Transform"/> of the spawned game object. 
        /// </summary>
        public Transform spawnedTransform;

        /// <summary>
        /// The <see cref="Spawner"/> that created the spawned item. 
        /// </summary>
        public Spawner spawnedOwner;

        // Methods
        /// <summary>
        /// Returns a value indicating whether this spawned item is alive in the scene.
        /// </summary>
        /// <returns>True if the object is alive or false if not</returns>
        public bool IsAlive()
        {
            // Check for destroyed transform
            if (spawnedTransform == null)
                return false;

            // Check for id
            SpawnableIdentity id = spawnedTransform.GetComponent<SpawnableIdentity>();

            // Chekc for destroyed id
            if (id == null)
                return false;

            // Check for destroyed flag
            return id.IsDestroyed == false;
        }
    }

    /// <summary>
    /// A <see cref="SpawnController"/> is responsible for issuing spawn requests to its assigned <see cref="Spawner"/>.
    /// The frequency of these requests will be determined by the controller implementation.
    /// </summary>
    public abstract class SpawnController : MonoBehaviour
    {
        // Class
        /// <summary>
        /// A Unity event that accepts a <see cref="Transform"/> component as the argument. 
        /// </summary>
        [Serializable]
        public class UnityTransformEvent : UnityEvent<Transform> { }

        /// <summary>
        /// A skeleton class that is used solely for running coroutines and being able to pause them.
        /// </summary>
        [Serializable]
        public class SpawnControllerRoutine : MonoBehaviour
        {
            // Properties
            /// <summary>
            /// Is the routine currently paused.
            /// </summary>
            public bool IsPaused
            {
                get { return gameObject.activeSelf == false; }
                set { gameObject.SetActive(!value); }
            }
        }

        // Editor
#if UNITY_EDITOR
        /// <summary>
        /// Should the events foldout be expanded.
        /// </summary>
        [HideInInspector]
        public bool editorEventsExpanded = false;
#endif

        // Private
        private SpawnControllerRoutine controllerRoutine = null;
        private List<SpawnedItem> spawnedItems = new List<SpawnedItem>();
        private IEnumerator active = null;
        private bool isTryingToSpawn = false;
        private bool isSpawning = false;

        private int totalItemsSpawned = 0;
        private int totalItemsDestroyed = 0;

        // Public
        /// <summary>
        /// The <see cref="Spawner"/> that this controller should use for spawning items. 
        /// </summary>
        [Tooltip("The spawner responsible for handling spawn requests")]
        public Spawner spawner;

        /// <summary>
        /// Should the controller automatically begin spawning when the scene is loaded.
        /// </summary>
        [Tooltip("Should the controller automatically begin spawning when the scene is loaded")]
        public bool playOnStart = true;

        /// <summary>
        /// The maximum number of items that can exist in the scene at any time.
        /// </summary>
        [Tooltip("The maximum number of concurrent items that can exist in the scene at any time. [No spawn limit = -1]")]
        public int maximumSpawnCount = 12;
        
        // Events
        /// <summary>
        /// Invoked when this <see cref="SpawnController"/>spawns an item. 
        /// </summary>
        [HideInInspector]
        public UnityTransformEvent OnItemSpawned;
        /// <summary>
        /// Invoked when an item spawned by this <see cref="SpawnController"/> has been destroyed. 
        /// </summary>
        [HideInInspector]
        public UnityTransformEvent OnItemDespawned;
        /// <summary>
        /// Invoked when this <see cref="SpawnController"/> has started its spawning routine.
        /// </summary>
        [HideInInspector]
        public UnityEvent OnStart;
        /// <summary>
        /// Called when this <see cref="SpawnController"/> has been paused. 
        /// </summary>
        [HideInInspector]
        public UnityEvent OnPaused;
        /// <summary>
        /// Called when this <see cref="SpawnController"/> has been resumed.
        /// </summary>
        [HideInInspector]
        public UnityEvent OnResumed;
        /// <summary>
        /// Called when this <see cref="SpawnController"/> has ended its spawning routine. 
        /// </summary>
        [HideInInspector]
        public UnityEvent OnEnd;

        // Properties
        private SpawnControllerRoutine ControllerRoutine
        {
            get
            {
                // Create if necessary
                if (controllerRoutine == null)
                    controllerRoutine = CreateControllerRoutineObjectIfRequired();

                return controllerRoutine;
            }
        }

        /// <summary>
        /// Returns true if this controller has been started.
        /// </summary>
        public bool IsSpawning
        {
            get { return isSpawning; }
        }

        /// <summary>
        /// Returns true when this controller is attempting to spawn an item.
        /// This may be true for a number of frames until a suitable <see cref="Spawner"/> becomes available for spawning. 
        /// </summary>
        public bool IsTryingToSpawn
        {
            get { return isTryingToSpawn; }
        }

        /// <summary>
        /// Is the controller currently paused.
        /// </summary>
        public bool IsPaused
        {
            get { return ControllerRoutine.IsPaused; }
            set
            {
                ControllerRoutine.IsPaused = value;

                if (value == true)
                    OnPaused.Invoke();
                else
                    OnResumed.Invoke();
            }
        }

        /// <summary>
        /// Returns the number of alive <see cref="SpawnableIdentity"/> instances that were created by this <see cref="SpawnableItems"/>.  
        /// </summary>
        public int SpawnedItemCount
        {
            get { return spawnedItems.Count; }
        }

        /// <summary>
        /// Get the total number of items that have been spawned.
        /// </summary>
        public int TotalSpawnedItemCount
        {
            get { return totalItemsSpawned; }
        }

        /// <summary>
        /// Get the total number of items that have been destroyed.
        /// </summary>
        public int TotalDestroyedItemCount
        {
            get { return totalItemsDestroyed; }
        }

        // Methods
        /// <summary>
        /// The main spawn coroutine that must be implemented by inheriting controllers.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator SpawnRoutine();

        /// <summary>
        /// A couroutine that attempts to periodically spawn an item unitl the target <see cref="Spawner"/> becomes available. 
        /// </summary>
        /// <param name="targetSpawner">The target <see cref="Spawner"/> that should be used to spawn the item or null if the controller should default to the root <see cref="Spawner"/></param>
        /// <param name="targetItemRef">The target spawnable item that should be spawned or null if the controller should auto select the item</param>
        /// <param name="spawnCollection">An optional collection where all spawned items will be added</param>
        /// <returns></returns>
        public IEnumerator ItemSpawnRoutine(Spawner targetSpawner = null, SpawnableItemRef targetItemRef = null, ICollection<SpawnedItem> spawnCollection = null)
        {
            // Set the spawning flag
            isTryingToSpawn = true;

            // use default spawner if none is specified
            if (targetSpawner == null)
                targetSpawner = spawner;

            // Loop forever or until we stop spawning
            while (isSpawning == true)
            {
                // Check if we are over the spawn limit
                if (maximumSpawnCount != -1)
                {
                    // Wait until we are under the limit
                    while (spawnedItems.Count >= maximumSpawnCount)
                        yield return null;
                }

                // We can only spawn when the spawner is available
                if (targetSpawner.IsAvailable == true)
                {
                    // Call the spawn method
                    Transform spawnedItem = targetSpawner.Spawn(targetItemRef);

                    // Register the spawned item
                    spawnedItems.Add(new SpawnedItem
                    {
                        spawnedTransform = spawnedItem,
                        spawnedOwner = spawner,
                    });

                    // Register the spawned item
                    if (spawnCollection != null)
                        spawnCollection.Add(spawnedItems[spawnedItems.Count - 1]);

                    // Increment counter
                    totalItemsSpawned++;

                    // Call child method
                    OnControllerSpawnedItem(spawnedItem);

                    // Stop trying to spawn
                    isTryingToSpawn = false;

                    // Trigger the event
                    OnItemSpawned.Invoke(spawnedItem);

                    // Finish spawning
                    break;
                }

                // Wait for next frame
                yield return null;
            }

            // Unset the flag
            isTryingToSpawn = false;
        }

        /// <summary>
        /// Resets the <see cref="SpawnController"/> to its initial state. 
        /// </summary>
        public virtual void ResetState()
        {
            totalItemsSpawned = 0;
            totalItemsDestroyed = 0;
        }

        /// <summary>
        /// Enable spawning for this controller.
        /// </summary>
        public virtual void StartSpawning()
        {
            // CHeck for already running
            if (isSpawning == true)
                return;

            isSpawning = true;
            active = SpawnRoutine();

            // Start the main routine
            ControllerRoutine.StartCoroutine(active);

            // Trigger event
            OnStart.Invoke();
        }

        /// <summary>
        /// Disable spawning for this controller.
        /// </summary>
        public virtual void StopSpawning()
        {
            // Make sure we are spawning
            if (isSpawning == false)
                return;

            // Stop routine
            if (active != null)
                ControllerRoutine.StopCoroutine(active);

            // Reset the controller
            ResetState();

            active = null;
            isSpawning = false;
            isTryingToSpawn = false;

            // Trigger event
            OnEnd.Invoke();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void Awake()
        {
            // Check if there is no spawner assigned
            if(spawner == null)
            {
                // Disable the controller
                enabled = false;
                throw new MissingReferenceException(string.Format("Spawn controller '{0}' requires a 'Spawner' to be assigned", GetType().Name));
            }

            // Listen for destroyed item events
            SpawnableItems.OnSpawnedItemDestroyed += OnSpawnedItemDestroyed;
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void Start()
        {
            // Begin spawning
            if (playOnStart == true)
                StartSpawning();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void OnDestroy()
        {
            // Remove listener for destroyed items
            SpawnableItems.OnSpawnedItemDestroyed -= OnSpawnedItemDestroyed;
        }

        /// <summary>
        /// Retruns a value indicating whether this spawn controller will have spawning abilities for the specified spawnable item.
        /// </summary>
        /// <param name="itemRef">The spawnable item reference</param>
        /// <returns>True if the item is spawnable by this controller or false if not</returns>
        public bool HasSpawnableItem(SpawnableItemRef itemRef)
        {
            // Check for no spawner
            if (spawner == null)
                return false;

            // Try to get items
            SpawnableItems items = spawner.SpawnableItems;

            // Check for no items
            if (items == null)
               return false;

            // Check if the item can be found
            return items.FindSpawnableItem(itemRef) != null;
        }

        /// <summary>
        /// Called when this controller has spawned an item.
        /// </summary>
        /// <param name="item">The <see cref="Transform"/> of the item that was spawned</param>
        protected virtual void OnControllerSpawnedItem(Transform item) { }

        /// <summary>
        /// Called when one of this controllers spawned items has been destroyed.
        /// </summary>
        /// <param name="item">The <see cref="Transform"/> of the item that was destroyed</param>
        protected virtual void OnControllerDestroyedItem(Transform item) { }

        private void OnSpawnedItemDestroyed(Transform destroyedItem)
        {
            // Check all spawned items
            for(int i = 0; i < spawnedItems.Count; i++)
            {
                // Check if the item was spawned by this controller
                if(spawnedItems[i].spawnedTransform == destroyedItem)
                {
                    // Increment destroy counter
                    totalItemsDestroyed++;

                    // Trigger child method
                    OnControllerDestroyedItem(destroyedItem);

                    // Trigger event
                    OnItemDespawned.Invoke(destroyedItem);

                    // Remove from spawned list
                    spawnedItems.RemoveAt(i);
                    break;
                }
            }
        }

        private SpawnControllerRoutine CreateControllerRoutineObjectIfRequired()
        {
            // Create a sub object
            GameObject go = new GameObject("ControllerRoutine");
            go.transform.SetParent(transform);

            // Add the component
            return go.AddComponent<SpawnControllerRoutine>();
        }
    }
}
