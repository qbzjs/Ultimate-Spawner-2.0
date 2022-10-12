using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// The method used to determine which child <see cref="Spawner"/> should be selected for spawning.
    /// </summary>
    public enum SpawnMode
    {
        /// <summary>
        /// Select a random spawner.
        /// </summary>
        Random,
        /// <summary>
        /// Spawn from all spawners in sequence.
        /// </summary>
        Sequential,
        /// <summary>
        /// Spawn from all spawners in reverse sequence.
        /// </summary>
        ReverseSequential,
        /// <summary>
        /// Spawn from the spawner that is at the specified index.
        /// </summary>
        AtIndex,
        /// <summary>
        /// Spawn from the spawner that is nearest to 'target'.
        /// </summary>
        NearestTarget,
        /// <summary>
        /// Spawn from the spawner that is nearest to 'target'
        /// </summary>
        NearestTargetWithTag,
        /// <summary>
        /// Spawn from the spawner that is farthest from 'target'.
        /// </summary>
        FarthestTarget,
        /// <summary>
        /// Spawn from the spawner that is farthest from 'target'
        /// </summary>
        FarthestTargetWithTag,   
    }

    /// <summary>
    /// A <see cref="SpawnerGroup"/> is a special type of spawner which acts as a managing object and can distribute spawn requests to child <see cref="Spawner"/>. 
    /// </summary>
    public class SpawnerGroup : Spawner
    {
        // Private
        private List<Spawner> cachedAvailableSpawners = new List<Spawner>();
        private int currentSpawner = 0;

        // Protected
        /// <summary>
        /// An array of all spawners that are a child of this spawner.
        /// The array will be automatically filled when the game starts.
        /// </summary>
        protected Spawner[] spawners = new Spawner[0];

        // Public
        /// <summary>
        /// The <see cref="SpawnMode"/> used to determine which <see cref="Spawner"/> should be selected to spawn the next item.  
        /// </summary>
        public SpawnMode spawnSelectionMode = SpawnMode.Random;

#if UNITY_EDITOR
        /// <summary>
        /// The target used to filter <see cref="SpawnerTarget"/> objects in the scene. 
        /// </summary>
        [DisplayConditionMethod("IsSelectionModeTagBased", DisplayType.Hidden, typeof(TagCollectionDrawer))]
#endif
        public string spawnTargetTag = null;

        /// <summary>
        /// An optional distance value used to prevent items spawning too close to a target object when spawner selection is set to Nearest.
        /// The value represents a minimum distance that the target object should be from the spawn in order for that spawner to be considered for the next spawn request.
        /// </summary>
        [Tooltip("An optional distance value which will cause spawners within this radius of the spawner target to be excluded from the spawn request")]
        [DisplayConditionMethod("IsSelectionModeNearestBased")]
        public float targetProximityDeadzone = 0f;
        /// <summary>
        /// The index used to select the child spawner to handle spawn requests.
        /// </summary>
        [DisplayCondition("spawnSelectionMode", SpawnMode.AtIndex)]
        public int spawnSelectionIndex = 0;

        // Properties
        /// <summary>
        /// Get all child spawners of this <see cref="SpawnerGroup"/>. 
        /// </summary>
        public Spawner[] Spawners
        {
            get { return spawners; }
        }

        /// <summary>
        /// Returns true is any child <see cref="Spawner"/> is available for spawning or false if there are none available. 
        /// </summary>
        public override bool IsAvailable
        {
            get
            {
                // Always update the spawners in editor as there can be hierarchy changes at anytime.
                if (Application.isPlaying == false)
                    UpdateSpawners();

                // Check for any available child spawners
                foreach (Spawner spawner in spawners)
                    if (spawner.IsAvailable == true)
                        return true;

                // No spawners available
                return false;
            }
        }

        // Properties
        /// <summary>
        /// Returns the number of items that can potentially be spawned at this <see cref="Spawner"/>. 
        /// Note that this property does not take into account whether or not all locations are available. 
        /// </summary>
        public override int SpawnableItemCapacity
        {
            get
            {
                // Always update the spawners in editor as there can be hierarchy changes at anytime.
                if (Application.isPlaying == false)
                    UpdateSpawners();

                int count = 0;

                // Get all child spawners
                foreach (Spawner spawner in spawners)
                {
                    // Account for all child spawners capacities.
                    count += spawner.SpawnableItemCapacity;
                }

                return count;
            }
        }

        /// <summary>
        /// Returns the number of items that can be spawned at this <see cref="Spawner"/> on the current frame.
        /// Note that this property takes into account the availablity of the <see cref="Spawner"/>. 
        /// </summary>
        [SpawnerInfo("Available Capacity")]
        public override int AvailableSpawnableItemCapacity
        {
            get
            {
                // Always update the spawners in editor as there can be hierarchy changes at anytime
                if (Application.isPlaying == false)
                    UpdateSpawners();

                int count = 0;

                // Get all child spawners
                foreach(Spawner spawner in spawners)
                {
                    // Account for all child spawners
                    count += spawner.AvailableSpawnableItemCapacity;
                }

                return count;
            }
        }

        /// <summary>
        /// Enumerate all child spawners that are end point spawners.
        /// An end point spawner is defined as a spawner that is capable of handling a spawn request directly without delegating to child spawners.
        /// </summary>
        protected IEnumerable<Spawner> EndPointSpawners
        {
            get
            {
                // Create a state
                Stack<Spawner> state = new Stack<Spawner>(spawners);
                
                while(state.Count > 0)
                {
                    // Get the spawner
                    Spawner spawner = state.Pop();

                    // Check for end point
                    if ((spawner is SpawnerGroup) == false)
                        yield return spawner;

                    // Push children
                    foreach (Spawner child in spawner)
                        state.Push(child);
                }
            }
        }

        // Methods
        /// <summary>
        /// Called by unity.
        /// </summary>
        public override void Awake()
        {
            // Be sure to call the base method
            base.Awake();

            // Get all spawners attached
            UpdateSpawners();            
        }

        /// <summary>
        /// Attempt to spawn a <see cref="SpawnableItem"/> as one of the child <see cref="Spawner"/>.
        /// This method will automatically create a <see cref="SpawnableItem"/> using the settings specified in the inspector. 
        /// </summary>
        /// <param name="itemRef">An item reference of the spawnable item that should be spawned if possible. This item can only be spawned if it is not masked and can be resolved. Use null to select a spawnable item randomly using the spawn chance value</param>
        /// <returns>The transform of the newly spawned item or null if the spawn failed</returns>
        public override Transform Spawn(SpawnableItemRef itemRef = null)
        {
            // Make sure the spawner is available
            if(IsAvailable == false)
            {
                Debug.LogWarning("Failed to spawn - No available spawn location");
                return null;
            }
            
            // Get a spawner based on settings
            Spawner spawner = FindNextSelectiveSpawner(itemRef);

            // Check for error
            if (spawner == null)
            {
                Debug.LogWarning("Failed to spawn - No available spawn locaition");
                return null;
            }

            // Call the spawn method on the spawner
            return spawner.Spawn(itemRef);
        }

        /// <summary>
        /// Attempt to spawn the specified object transform at one of this child <see cref="Spawner"/>. 
        /// </summary>
        /// <param name="toSpawn">The transform component of the object to spawn</param>
        /// <returns>True if the spawn was successful or false if the <see cref="Spawner"/> handling the request could not spawn the transform</returns>
        public override bool Spawn(Transform toSpawn)
        {
            // Get a spawner based on settings
            Spawner spawner = FindNextSelectiveSpawner();

            // Check for error
            if (spawner == null)
            {
                Debug.LogWarning("Failed to spawn - No available spawn locaition");
                return false;
            }

            // Spawn the item
            return spawner.Spawn(toSpawn);
        }

        /// <summary>
        /// Get a spawn location from one of the child <see cref="Spawner"/>. 
        /// </summary>
        /// <returns>The <see cref="SpawnLocation"/> representing the position and rotation of the selected <see cref="Spawner"/></returns>
        public override SpawnLocation GetLocation()
        {
            // Get a spawner based on settings
            Spawner spawner = FindNextSelectiveSpawner();

            // Check for error
            if (spawner == null)
                return SpawnLocation.invalid;

            // Spawn the item
            return spawner.GetLocation();
        }

        /// <summary>
        /// Get an enumerator for all child spawners of this <see cref="Spawner"/>.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Spawner> GetEnumerator()
        {
            // Get all child spawners
            foreach (Spawner spawner in spawners)
                yield return spawner;
        }

        /// <summary>
        /// Checkes whether this <see cref="SpawnerGroup"/> has a valid end point spawner.
        /// An end point spawner is defined as a spawner that is capable of handling spawn requests without delegating to a child spawner (Spawn Point and Spawn areas).
        /// </summary>
        /// <returns>True is this spawner has a vlid end point</returns>
        public bool HasValidEndPointSpawner()
        {
            // Create a state stack
            Stack<Spawner> state = new Stack<Spawner>();
            state.Push(this);

            while(state.Count > 0)
            {
                // Get the next spawner
                Spawner current = state.Pop();

                // Check for end condition
                if ((current is SpawnerGroup) == false)
                    return true;

                // Push child spawners
                foreach (Spawner child in current)
                    state.Push(child);
            }

            // No valid end point spawner
            return false;
        }

        /// <summary>
        /// Attempts to select the next <see cref="Spawner"/> based on the current <see cref="SpawnMode"/>.
        /// If none of the child <see cref="Spawner"/> are available then the return value will be null.
        /// </summary>
        /// <returns></returns>
        public Spawner FindNextSelectiveSpawner(SpawnableItemRef itemRef = null)
        {
            // Find all available spawners
            List<Spawner> available = FindAvailableSpawners(itemRef);

            // Check for error
            if (available.Count == 0)
                return null;

            Spawner result = null;

            // Select based on mode
            switch(spawnSelectionMode)
            {
                default:
                case SpawnMode.Sequential:
                    {
                        // Make sure the current spawner index is in bounds
                        if (currentSpawner >= available.Count)
                            currentSpawner = 0;

                        // Get the current spawner
                        result = available[currentSpawner];

                        // Advance index for next spawn
                        currentSpawner++;
                        break;
                    }

                case SpawnMode.ReverseSequential:
                    {
                        // Make sure the current spawner index is in bounds
                        if (currentSpawner < 0)
                            currentSpawner = (available.Count - 1);

                        // Get the current spawner
                        result = available[currentSpawner];

                        // Advance index for next spawn
                        currentSpawner--;
                        break;
                    }

                case SpawnMode.NearestTarget:
                case SpawnMode.NearestTargetWithTag:
                    {
                        // Select the required tag
                        string tag = (spawnSelectionMode == SpawnMode.NearestTargetWithTag) ? spawnTargetTag : null;

                        // Find a spawner target in the scene
                        SpawnerTarget target = SpawnerTarget.FindRandomSpawnerTarget(tag);

                        // Make sure we have a target
                        if (target == null)
                        {
                            Debug.LogWarning("Failed to spawn at nearest target - There are no SpawnerTargets in the scene. Random spawning will be used");

                            // Select a random index
                            int index = Random.Range(0, available.Count);

                            // Get the spawner
                            result = available[index];
                            break;
                        }

                        // Find the nearest spawner
                        float nearestDistance = float.MaxValue;

                        foreach (Spawner spawner in available)
                        {
                            // Find the distance
                            float distance = Vector3.Distance(target.transform.position, spawner.transform.position);

                            // Skip the spawner because it is inside the deadzone
                            if (targetProximityDeadzone != 0f && distance < targetProximityDeadzone)
                                continue;

                            // CHeck for closer
                            if (distance < nearestDistance)
                            {
                                // Update the nearest spawner
                                result = spawner;
                                nearestDistance = distance;
                            }
                        }
                        break;
                    }

                case SpawnMode.FarthestTarget:
                case SpawnMode.FarthestTargetWithTag:
                    {
                        // Select the required tag
                        string tag = (spawnSelectionMode == SpawnMode.NearestTargetWithTag) ? spawnTargetTag : null;

                        // Find a spawner target in the scene
                        SpawnerTarget target = SpawnerTarget.FindRandomSpawnerTarget(tag);
                        
                        // Make sure we have a target
                        if (target == null)
                        {
                            Debug.LogWarning("Failed to spawn at farthest target - There are no SpawnerTargets in the scene. Random spawning will be used");

                            // Select a random index
                            int index = Random.Range(0, available.Count);

                            // Get the spawner
                            result = available[index];
                            break;
                        }

                        // Find the nearest spawner
                        float farthestDistance = 0;

                        foreach (Spawner spawner in available)
                        {
                            // Find the distance
                            float distance = Vector3.Distance(target.transform.position, spawner.transform.position);

                            // CHeck for closer
                            if (distance > farthestDistance)
                            {
                                // Update the nearest spawner
                                result = spawner;
                                farthestDistance = distance;
                            }
                        }
                        break;
                    }

                case SpawnMode.AtIndex:
                    {
                        // Get the index
                        int index = spawnSelectionIndex;

                        // Check for out of bounds
                        if(index < 0 || index >= spawners.Length)
                        {
                            index = 0;
                            Debug.LogWarningFormat("Failed to select spawner via index on spawn group '{0}'. Falling back to first available spawner", this);
                        }

                        // Get the spawner - dont select from the available list - this should always return the spawner at the index regardless
                        result = spawners[index];
                        break;
                    }

                case SpawnMode.Random:
                    {
                        // Select a random index
                        int index = Random.Range(0, available.Count);

                        // Get the spawner
                        result = available[index];
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        /// Finds all child <see cref="Spawner"/> that are currently available.
        /// If there are no available spawners then the return value will be an empty list.
        /// </summary>
        /// <returns>A list of available <see cref="Spawner"/> or an empty list on failure</returns>
        public List<Spawner> FindAvailableSpawners(SpawnableItemRef itemRef = null)
        {
            // Clear old list
            cachedAvailableSpawners.Clear();

            // Check all child spawners
            foreach(Spawner spawner in spawners)
            {
                // check if we have a valid spawnable id
                if(itemRef != null)
                {
                    // Check if the spawner is able to spawn the specified item
                    if (spawner.SpawnableMask.IsMasked(itemRef.ID) == true)
                    {
                        UltimateSpawning.Log("Exclude spawner: " + spawner.name);
                        continue;
                    }
                }

                // Check whether the spawner is available
                if(spawner.IsAvailable == true)
                {
                    // The spawner is available so add it to the list
                    cachedAvailableSpawners.Add(spawner);
                }
            }

            // Get the cached list
            return cachedAvailableSpawners;
        }

        /// <summary>
        /// Repopulates the collection of child spawners that this <see cref="SpawnerGroup"/> delegates spawn requests to. 
        /// </summary>
        public void UpdateSpawners()
        {
            // List of child spawners
            List<Spawner> allSpawners = new List<Spawner>();

            foreach(Transform child in transform)
            {
                // Find all spawner scripts attached
                Spawner[] spawnerComponents = child.GetComponents<Spawner>();

                // Add range of spawners
                allSpawners.AddRange(spawnerComponents);
            }

            // Store child spawners
            spawners = allSpawners.ToArray();
        }

        /// <summary>
        /// Returns true if the spawner selection mode is based on distance from a <see cref="SpawnerTarget"/>. 
        /// </summary>
        /// <returns>True if the selection mode is target based or false if not</returns>
        public bool IsSelectionModeTargetBased()
        {
            switch(spawnSelectionMode)
            {
                case SpawnMode.FarthestTarget:
                case SpawnMode.FarthestTargetWithTag:
                case SpawnMode.NearestTarget:
                case SpawnMode.NearestTargetWithTag:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the spawner selection mode uses object tags in its selection mode.
        /// </summary>
        /// <returns>True if the selection mode is tag based or false if not</returns>
        public bool IsSelectionModeTagBased()
        {
            switch(spawnSelectionMode)
            {
                case SpawnMode.FarthestTargetWithTag:
                case SpawnMode.NearestTargetWithTag:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the spawner selection mode uses nearest based calculations in its selection.
        /// </summary>
        /// <returns>True if the spawner is nearest based or false if not</returns>
        public bool IsSelectionModeNearestBased()
        {
            switch(spawnSelectionMode)
            {
                case SpawnMode.NearestTarget:
                case SpawnMode.NearestTargetWithTag:
                    return true;
            }
            return false;
        }
    }
}
