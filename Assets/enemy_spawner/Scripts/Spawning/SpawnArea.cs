using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// A <see cref="SpawnArea"/> represents a volume in space with a rotation that defines an area where many items can be spawned.
    /// The area is represented by a rectangular volume which is capable of performing physics overlap checks to spawn items without overlapping.
    /// </summary>
    public sealed class SpawnArea : EndPointSpawner
    {
        [Serializable]
        private class SpawnAreaLocation
        {
            // Public
            public SpawnLocation location;
            public Vector3 localOffest = Vector3.zero;
            [HideInInspector]
            public bool isOccupied = false;
        }

        // Internal
#if UNITY_EDITOR
        internal InspectorValueWatcher is2DSpawnerWatcher = new InspectorValueWatcher("is2DSpawner");
        internal InspectorValueWatcher isAboveGroundWatcher = new InspectorValueWatcher("isAboveGround");
        internal InspectorValueWatcher sizeWatcher = new InspectorValueWatcher("size");
        internal InspectorValueWatcher spawnRadiusWatcher = new InspectorValueWatcher("spawnRadius");
        internal InspectorValueWatcher ceilingHeightWatcher = new InspectorValueWatcher("ceilingHeight");
        internal InspectorValueWatcher occupiedCheckWatcher = new InspectorValueWatcher("occupiedCheck");
#endif

        // Private
        [SerializeField, HideInInspector]
        private List<SpawnAreaLocation> spawnLocations = new List<SpawnAreaLocation>();
        private List<Collider> colliding3D = new List<Collider>();              // 3D colliders that are inside the spawn area
        private List<Collider2D> colliding2D = new List<Collider2D>();          // 2D colliders that are inside the spawn area
        private Stack<Component> createdComponents = new Stack<Component>();    // A collection of components that have been created by this spwner
        
        /// <summary>
        /// Is the spawn point placed on a ground object. If so, the center of the <see cref="SpawnPoint"/> will be placed above the ground to avoid collisions with it.
        /// If false, the center will not be modified.
        /// After changing this value via code you should call <see cref="RebuildColliders"/> to update the colliders if the <see cref="occupiedCheck"/> is equal to <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("Is the spawn point intended to be placed on a ground plane. When true, this will cause the spawn point colliders to be moved above the surface to prevent collisions with the ground from interfering with the occupied check")]
        [SerializeField]
        private bool isAboveGround = true;

        /// <summary>
        /// Is the <see cref="SpawnPoint"/> static or is it likley to move during game play.
        /// After changing this value via code you should call <see cref="RebuildColliders"/> to update the colliders if the <see cref="occupiedCheck"/> is equal to <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("Is the spawn point liable to move in the scene. If false, the spawn location will be re-synced automatically. Note: You can move a static spawn point but you should call 'Rebuild()' to update the spawn point afterwards")]
        [SerializeField]
        private bool isStatic = true;

        /// <summary>
        /// The size of the bounding volume.
        /// </summary>
        [SerializeField]
        private Vector2 size = new Vector2(6, 4);

        /// <summary>
        /// The minimum amount of space that items should be apart when spawning.
        /// </summary>
        [Tooltip("The radius of the spawn point. Used for occupied checks")]
        [SerializeField]
        private float spawnRadius = 0.5f;

        /// <summary>
        /// The maximum headroom required to spawn an item.
        /// </summary>
        [DisplayCondition("is2DSpawner", false)]
        [SerializeField]
        private float ceilingHeight = 1;

        /// <summary>
        /// The method used to determine whether the <see cref="SpawnPoint"/> is occupied or not. 
        /// After changing this value via code you should call <see cref="RebuildColliders"/> to update the colliders if the <see cref="occupiedCheck"/> is equal to <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("The method used to determine whether the spawn point is occupied or not")]
        [SerializeField]
        private OccupiedCheck occupiedCheck = OccupiedCheck.PhysicsOverlap;

        // Public
        /// <summary>
        /// The layer that should be used for all collision checks.
        /// Only used when <see cref="occupiedCheck"/> is set to <see cref="OccupiedCheck.PhysicsOverlap"/> or <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("The layer to check collisions on for occupied checks")]
        [DisplayCondition("occupiedCheck", OccupiedCheck.None, ConditionType.NotEqual)]
        public LayerMask collisionLayer = 1;

        /// <summary>
        /// Should the rotation of the spawner be used to rotate the spawned item to the same orientation.
        /// After changing this value via code you should call <see cref="RebuildColliders"/> to update the colliders if the <see cref="occupiedCheck"/> is equal to <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("Should the spawner apply rotation to the spawned item or should only the position element of the transform be modified")]
        public SpawnRotationApplyMode applyRotation = SpawnRotationApplyMode.FullRotation;

        /// <summary>
        /// Limit the total number of spawn locations but eveny distribute the spawn requests.
        /// </summary>
        public int maxAvailableLocations = 16;


#if ULTIMATESPAWNER_DEBUG
        /// <summary>
        /// Should the sub locations be displayed in editor.
        /// </summary>
        [Header("Debug")]
        public bool drawLocationGizmos = true;
#endif

        // Properties
        /// <summary>
        /// Get a value idicating whether the spawner is setup as a 3D or 2D spawner.
        /// This value can be changed at runtime and will cause the spawner to rebuild its components if necessary.
        /// </summary>
        public override bool Is2DSpawner
        {
            get { return base.Is2DSpawner; }
            set
            {
                base.Is2DSpawner = value;
                RebuildColliders();
                RebuildSpawnLocations();
            }
        }

        /// <summary>
        /// Is the spawn point above the ground. 
        /// When true, all occupied collision checks will be moved upwards by the amount of <see cref="SpawnRadius"/> so that collisions checks with the ground can be avoided.
        /// </summary>
        public bool IsAboveGround
        {
            get { return isAboveGround; }
            set
            {
                isAboveGround = value;
                RebuildColliders();
            }
        }

        /// <summary>
        /// Is the spawn point static.
        /// A static spawn point shuould not be moved in the scene during gameplay but will have a very slight performance increase.
        /// If you need to move a static spawn point very rarley then you can mark it as static and call <see cref="Rebuild"/> after moving to resync all position data. 
        /// </summary>
        public bool IsStatic
        {
            get { return isStatic; }
            set
            {
                isStatic = value;
                Rebuild();
            }
        }

        /// <summary>
        /// The size of this <see cref="SpawnArea"/>.
        /// Defines the <see cref="SpawnArea"/> spawning limit bounds. 
        /// </summary>
        public Vector2 Size
        {
            get { return size; }
            set
            {
                size = value;
                RebuildColliders();
                RebuildSpawnLocations();
            }
        }

        /// <summary>
        /// The radius required by each spawnable item.
        /// This value determines the spacing used between item spawns.
        /// </summary>
        public float SpawnRadius
        {
            get { return spawnRadius; }
            set
            {
                spawnRadius = value;
                RebuildSpawnLocations();
            }
        }

        /// <summary>
        /// The height of the <see cref="SpawnArea"/> voume.
        /// This height value is used for occupied checks and determines the head room required to spawn an item.
        /// </summary>
        public float CeilingHeight
        {
            get { return ceilingHeight; }
            set
            {
                ceilingHeight = value;
                RebuildColliders();
            }
        }

        /// <summary>
        /// The <see cref="OccupiedCheck"/> used to determine whether the spawn point is available for spawning or not. 
        /// </summary>
        public OccupiedCheck OccupiedCheck
        {
            get { return occupiedCheck; }
            set
            {
                occupiedCheck = value;
                RebuildColliders();
            }
        }
        
        /// <summary>
        /// Returns true if the <see cref="SpawnArea"/> is able to spawn atleast 1 item. 
        /// </summary>
        public override bool IsAvailable
        {
            get { return IsOccupied() == false; }
        }

        /// <summary>
        /// Returns the total number of items that this <see cref="SpawnArea"/> can accomodate.
        /// Note that this value will be limited to <see cref="maxAvailableLocations"/> unless it is set to '-1' 
        /// </summary>
        public override int SpawnableItemCapacity
        {
            get
            {
                int capacity = spawnLocations.Count;

                if(maxAvailableLocations != -1)
                    if (maxAvailableLocations < capacity)
                        capacity = maxAvailableLocations;

                return capacity;
            }
        }

        /// <summary>
        /// Returns the number of available spawn locations for this <see cref="SpawnArea"/>.
        /// </summary>
        [SpawnerInfo("Available Capacity")]
        public override int AvailableSpawnableItemCapacity
        {
            get
            {
                int capacity = 0;

                foreach (SpawnAreaLocation areaLocation in spawnLocations)
                    if (areaLocation.isOccupied == false)
                        capacity++;

                if (maxAvailableLocations != -1)
                    if (maxAvailableLocations < capacity)
                        capacity = maxAvailableLocations;

                return capacity;
            }
        }

        /// <summary>
        /// Get the planar <see cref="Rect"/> representing the inner spawning plane of this <see cref="SpawnArea"/>.  
        /// </summary>
        public Rect LocalPlane
        {
            get
            {
                float diameter = SpawnDiameter;

                // Create the plane rect with padding
                return new Rect((size.x / 2f) + diameter, 
                    (size.y / 2f) + diameter / 2, 
                    size.x - (diameter/* * 2f*/), 
                    size.y - (diameter/* * 2f*/));
            }                    
        }

        /// <summary>
        /// Gets the squared radius of the spawn area which is equal to the diameter.
        /// </summary>
        public float SpawnDiameter
        {
            get { return spawnRadius * 2; }
        }

        // Methods
#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity editor.
        /// </summary>
        public override void Reset()
        {
            // Call the base method
            base.Reset();

            // Update the default value for 2d spawn area rotation
            if (is2DSpawner == true)
                applyRotation = SpawnRotationApplyMode.ZRotation;

            // Force rebuild
            RebuildSpawnLocations();
            Rebuild();
        }

        /// <summary>
        /// Called by Unity editor.
        /// </summary>
        public override void OnValidate()
        {
            base.OnValidate();

            // Limit spawn radius
            if (spawnRadius < 0.2f) spawnRadius = 0.2f;

            // Limit available locations
            if (maxAvailableLocations < -1) maxAvailableLocations = -1;
            if (maxAvailableLocations > SpawnableItemCapacity && SpawnableItemCapacity != 0) maxAvailableLocations = SpawnableItemCapacity;

            // Check for changed values
            if (is2DSpawnerWatcher.HasChanged(this) == true)
            {
                // Udpate rotation mode
                if (is2DSpawner == true)
                    applyRotation = SpawnRotationApplyMode.ZRotation;
                else
                    applyRotation = SpawnRotationApplyMode.FullRotation;
            }

            // Check for modified values
            if (isAboveGroundWatcher.HasChanged(this) == true ||
                sizeWatcher.HasChanged(this) == true ||
                spawnRadiusWatcher.HasChanged(this) == true ||
                ceilingHeightWatcher.HasChanged(this) == true ||
                occupiedCheckWatcher.HasChanged(this) == true)
            {
                RebuildColliders();
                RebuildSpawnLocations();
            }
        }
#endif

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public override void Awake()
        {
            // Be sure to call the base method
            base.Awake();

            // Rebuild the colliders
            if(occupiedCheck == OccupiedCheck.PhysicsTrigger)
            {
                // Create the necessary physcs components
                RebuildColliders();         
            }
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Rebuild available locations
            UpdateAvailableSpawnLocations();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Update()
        {
            if(isStatic == false)
            {
                // If the spawn area can move then we need to ensure that the spawn location keeps in sync
                Rebuild();
            }
        }

        /// <summary>
        /// Attempt to spawn a <see cref="SpawnableItem"/> at this <see cref="SpawnArea"/>.
        /// This method will automatically create a <see cref="SpawnableItem"/> using the settings specified in the inspector. 
        /// </summary>
        /// <param name="itemRef">An item reference of the spawnable item that should be spawned if possible. This item can only be spawned if it is not masked and can be resolved. Use null to select a spawnable item randomly using the spawn chance value</param>
        /// <returns>The transform of the newly spawned item or null if the spawn failed</returns>
        public override Transform Spawn(SpawnableItemRef itemRef = null)
        {
            SpawnLocation location;

            // Try to find a spawn location
            if (FindNextSpawnLocation(out location) == false)
            {
                Debug.LogWarning("Failed to spawn - No available spawn location");
                return null;
            }

            // Create a spawnable item
            Transform result = CreateSpawnableItem(itemRef, location, applyRotation);

            // Check for error
            if (result == null)
            {
                Debug.LogWarning("Failed to create spawnable item");
                return null;
            }

            return result;
        }

        /// <summary>
        /// Attempt to spawn the specified object transform at this <see cref="SpawnArea"/>. 
        /// </summary>
        /// <param name="toSpawn">The transform component of the object to spawn</param>
        /// <returns>True if the spawn was successful or false if the <see cref="SpawnArea"/> is not available</returns>
        public override bool Spawn(Transform toSpawn)
        {
            // Get a spawn location
            SpawnLocation location;

            // Check for valid location
            if (FindNextSpawnLocation(out location) == false)
                return false;

            // Spawn the object = Dont check availablity because we already have
            return location.Spawn(toSpawn, applyRotation);
        }

        /// <summary>
        /// Get the next <see cref="SpawnLocation"/> for this <see cref="SpawnArea"/>.  
        /// </summary>
        /// <returns>The <see cref="SpawnLocation"/> representing the position and rotation of this <see cref="SpawnArea"/></returns>
        public override SpawnLocation GetLocation()
        {
            // Get a spawn location
            SpawnLocation location;

            // Check for valid location
            if (FindNextSpawnLocation(out location) == false)
                return SpawnLocation.invalid;
            
            return location;
        }

        /// <summary>
        /// Enumerate all child spawners.
        /// Note that a <see cref="SpawnArea"/> cannot have any child spawners. 
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Spawner> GetEnumerator()
        {
            // No children
            yield break;
        }

        /// <summary>
        /// Forces all sub spawn locations availablities to be updated based on the current <see cref="occupiedCheck"/>. 
        /// </summary>
        public void UpdateAvailableSpawnLocations()
        {
            // Mark all locations as available
            SetAllSpawnLocationsOccupied(false);

            // Get the check mode
            OccupiedCheck check = occupiedCheck;

            // Always use physics overlay in editor
            if (Application.isPlaying == false)
                check = OccupiedCheck.PhysicsOverlap;

            // Check for no availablity check
            if (check == OccupiedCheck.None)
                return;
            
            if (is2DSpawner == true)
            {
                int collidersInsideArea = 0;
                IList<Collider2D> colliderList = null;

                if (check == OccupiedCheck.PhysicsOverlap)
                {
                    // Overlap a box
                    collidersInsideArea = Physics2D.OverlapBoxNonAlloc(transform.position, size, transform.rotation.z, sharedCollider2DBuffer, collisionLayer);

                    // Check if we  have completley filled the array
                    if (collidersInsideArea == sharedBufferSize)
                        Debug.LogWarning("Overlap check has filled the shared buffer. It is possible that some colliders have been missed by the check. You may need to increase the buffer size if your scene has a large amount of physics objects");

                    // Get the collider list
                    colliderList = sharedCollider2DBuffer;
                }
                else if (check == OccupiedCheck.PhysicsTrigger)
                {
                    // Get the collider list
                    collidersInsideArea = colliding3D.Count;
                    colliderList = colliding2D;
                }

                // Check if there are too many objects inside the area
                if (collidersInsideArea >= SpawnableItemCapacity)
                {
                    SetAllSpawnLocationsOccupied(true);
                    return;
                }

                // Check all potential spawn locations
                foreach (SpawnAreaLocation potentialLocation in spawnLocations)
                {
                    bool tooClose = false;

                    // Check each collider
                    for (int i = 0; i < collidersInsideArea; i++)
                    {
                        // Get the collider
                        Collider2D collider = colliderList[i];

                        // Skip trigger colliders
                        if (collider.isTrigger == true)
                            continue;

                        // Get direction to collider
                        Vector3 dir = collider.transform.position - potentialLocation.location.SpawnPosition;

                        // Scale the vector
                        Vector3 checkPoint = dir.normalized * spawnRadius;

                        // Check if the point lies within the colliders bounds
                        if (collider.bounds.Contains(potentialLocation.location.SpawnPosition + checkPoint) == true)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    // Check if there are any close colliders
                    if (tooClose == true)
                        potentialLocation.isOccupied = true;
                }
            }
            else
            {
                int collidersInsideArea = 0;
                IList<Collider> colliderList = null;

                if (check == OccupiedCheck.PhysicsOverlap)
                {
                    // Overlap a box
                    collidersInsideArea = Physics.OverlapBoxNonAlloc(transform.position + new Vector3(0, ceilingHeight / 2, 0), new Vector3(size.x, ceilingHeight, size.y), sharedColliderBuffer, transform.rotation, collisionLayer);
                    
                    // Check if we  have completley filled the array
                    if (collidersInsideArea == sharedBufferSize)
                        Debug.LogWarning("Overlap check has filled the shared buffer. It is possible that some colliders have been missed by the check. You may need to increase the buffer size if your scene has a large amount of physics objects");

                    // Get the collider list
                    colliderList = sharedColliderBuffer;
                }
                else if (check == OccupiedCheck.PhysicsTrigger)
                {
                    // Get the collider list
                    collidersInsideArea = colliding3D.Count;
                    colliderList = colliding3D;
                }

                // Check if there are too many objects inside the area
                if (collidersInsideArea >= SpawnableItemCapacity)
                {
                    SetAllSpawnLocationsOccupied(true);
                    return;
                }

                // Check all potential spawn locations
                foreach (SpawnAreaLocation potentialLocation in spawnLocations)
                {
                    bool tooClose = false;

                    // Check each collider
                    for (int i = 0; i < collidersInsideArea; i++)
                    {
                        // Get the collider
                        Collider collider = colliderList[i];

                        // Skip trigger colliders
                        if (collider.isTrigger == true)
                            continue;

                        // Get the collider position but eliminate the y plane
                        Vector3 colliderPlanePosition = collider.transform.position;
                        //colliderPlanePosition.y = potentialLocation.location.SpawnPosition.y;

                        Vector3 dir = colliderPlanePosition - potentialLocation.location.SpawnPosition;

                        // Scale the vector
                        Vector3 checkPoint = dir.normalized * spawnRadius;

                        Vector3 floorCheckPoint = potentialLocation.location.SpawnPosition + checkPoint - new Vector3(0, ceilingHeight / 2, 0);
                        Vector3 rayLength = transform.up * ceilingHeight;

                        Ray ray = new Ray(floorCheckPoint, rayLength);

                        // Check if the point lies within the colliders bounds
                        //if (collider.bounds.Contains(potentialLocation.location.SpawnPosition + checkPoint) == true)
                        if(collider.bounds.IntersectRay(ray) == true)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    // Check if there are any close colliders
                    if (tooClose == true)
                        potentialLocation.isOccupied = true;
                }
            }
        }

        /// <summary>
        /// Forces the spawn area to resynchronize.
        /// Use this method after you move a spawn area that has <see cref="IsStatic"/> set to true. 
        /// </summary>
        public void Rebuild()
        {
            // Sync all locations with the transform
            foreach (SpawnAreaLocation areaLocation in spawnLocations)
            {
                // Sync the positon and rotation
                areaLocation.location.Update(
                    transform.position + areaLocation.localOffest,
                    transform.rotation);
            }
        }

        /// <summary>
        /// Force the spawn area to regenerate the colliders it uses for occupied checks.
        /// This will cause physics components to be added as needed depending upon the current <see cref="OccupiedCheck"/> value. 
        /// </summary>
        public void RebuildColliders()
        {
            // Should only be used at runtime
            if (Application.isPlaying == true)
            {
                // Destroy any existing collider components - This must be done in the current frame otherwise the next frame will fail to add physics components due to conflicts
                while (createdComponents.Count > 0)
                    Destroy(createdComponents.Pop());


                // Check for trigger mode
                if (occupiedCheck == OccupiedCheck.PhysicsTrigger)
                {
                    // Configure colliders
                    if (is2DSpawner == true)
                    {
                        // We need to have a box collider
                        BoxCollider2D collider = GetComponent<BoxCollider2D>();

                        // Create a collider if one does not exist
                        if (collider == null)
                        {
                            // Add the component and register with this spawn area
                            collider = gameObject.AddComponent<BoxCollider2D>();
                            createdComponents.Push(collider);
                        }

                        // Update size
                        collider.size = size;
                        collider.isTrigger = true;
                    }
                    else
                    {
                        // We need a collider to be present
                        BoxCollider collider = GetComponent<BoxCollider>();

                        // Create a collider if one does not exist
                        if (collider == null)
                        {
                            // Add the component and register with this spawn area
                            collider = gameObject.AddComponent<BoxCollider>();
                            createdComponents.Push(collider);
                        }

                        // Update the size
                        collider.size = new Vector3(size.x, ceilingHeight, size.y);
                        collider.isTrigger = true;

                        // Check for above ground
                        if (isAboveGround == true)
                            collider.center = new Vector3(0, ceilingHeight / 2, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Force the spawn area to regenerate all potential spawn locations based on the area size and spacing values.
        /// </summary>
        public void RebuildSpawnLocations()
        {
            foreach (Transform child in transform)
                DestroyImmediate(child.gameObject);

            // Clear old locations
            spawnLocations.Clear();

            // Get the local rect
            Rect local = LocalPlane;

            // Get the square spawn radius
            float diameter = SpawnDiameter;

            // Find the number of cells
            int xPoints = (int)(local.width / diameter) + 1;
            int yPoints = (int)(local.height / diameter) + 1;

            // Find the ramining distance so we can dynamically expand and contract the spacing
            Vector2 remainingOffset = new Vector2(local.width - (diameter * xPoints), local.height - (diameter * yPoints));

            remainingOffset.x /= (xPoints + 1);
            remainingOffset.y /= (yPoints + 1);
            
            // Build the locations
            for (int x = 0; x < xPoints; x++)
            {
                for (int y = 0; y < yPoints; y++)
                {
                    // Find the local point offset for the spawn position
                    Vector2 pointOffset = new Vector2(
                        (local.width / 2) - diameter - (x * diameter),
                        (local.height / 2) - diameter - (y * diameter))
                        + new Vector2(diameter, diameter);

                    // Find the spawn height
                    float height = (isAboveGround == true) ? (ceilingHeight / 2f) : 0;

                    // Create the local spawn locaiton
                    Vector3 point = new Vector3(pointOffset.x, height, pointOffset.y);

                    // Check for 2d Mode
                    if (is2DSpawner == true)
                        point = new Vector3(pointOffset.x, pointOffset.y, 0);

                    // Transform the location
                    point = transform.TransformPoint(point);

                    // Create the spawn location
                    SpawnLocation location = new SpawnLocation(this, point, transform.rotation);

                    // Create the area location
                    SpawnAreaLocation areaLocation = new SpawnAreaLocation
                    {
                        location = location,
                        localOffest = pointOffset,
                        isOccupied = false,
                    };

                    // Register the location
                    spawnLocations.Add(areaLocation);
                }
            }

            // Randomize spawn locations
            RandomizeLocations();
        }

        /// <summary>
        /// Causes the spawn location lists to be randomized so that the next spawn location is not selected in grid formation.
        /// </summary>
        public void RandomizeLocations()
        {
            // Create a random object
            System.Random rand = new System.Random();

            // Get the list count
            int count = spawnLocations.Count;

            for (int i = count - 1; i > 1; i--)
            {
                int random = rand.Next(i + 1);

                // Swap with random index
                SpawnAreaLocation location = spawnLocations[random];

                // Switch locations
                spawnLocations[random] = spawnLocations[i];
                spawnLocations[i] = location;
            }
        }

        #region PhysicsEvents
        /// <summary>
        /// Called by Unity and is used to detect when 3D physics colliders enter the <see cref="SpawnPoint"/>. 
        /// </summary>
        /// <param name="other">The collider that caused the event</param>
        public void OnTriggerEnter(Collider other)
        {
            // Dont detect trigger objects and only continue if trigger events are used
            if (other.isTrigger == true)
                return;

            // Make sure the collider does not already exist for some unknown reason
            if (colliding3D.Contains(other) == false)
            {
                // Add to collision list
                colliding3D.Add(other);
            }
        }

        /// <summary>
        /// Called by Unity and is used to detect when 3D physics colliders exit the <see cref="SpawnPoint"/>. 
        /// </summary>
        /// <param name="other">The collider that caused the event</param>
        public void OnTriggerExit(Collider other)
        {
            // Dont detect trigger objects and only continue if trigger events are used
            if (other.isTrigger == true)
                return;

            // Check if the object is registered with our spawn point
            if (colliding3D.Contains(other) == true)
            {
                // Remove from colliding list
                colliding3D.Remove(other);
            }
        }

        /// <summary>
        /// Called by Unity and is used to detect when 2D physics colliders enter the <see cref="SpawnPoint"/>. 
        /// </summary>
        /// <param name="other">The collider that caused the event</param>
        public void OnTriggerEnter2D(Collider2D other)
        {
            // Dont detect trigger objects and only continue if trigger events are used
            if (other.isTrigger == true)
                return;

            // Make sure the collider does not already exist for some unknown reason
            if (colliding2D.Contains(other) == false)
            {
                // Add to collision list
                colliding2D.Add(other);
            }
        }

        /// <summary>
        /// Called by unity and is used to detect when 2D physics colliders exit the <see cref="SpawnPoint"/>. 
        /// </summary>
        /// <param name="other">The collider that caused the event</param>
        public void OnTriggerExit2D(Collider2D other)
        {
            // Dont detect trigger objects and only continue if trigger events are used
            if (other.isTrigger == true)
                return;

            // Check if the object is registered with our spawn point
            if (colliding2D.Contains(other) == true)
            {
                // Remove from colliding list
                colliding2D.Remove(other);
            }
        }
        #endregion

        private bool IsOccupied()
        {
            SpawnLocation location;

            // Check if we can find a next spawn location
            if (FindNextSpawnLocation(out location) == false)
                return true;

            // Default is success
            return false;
        }

        private bool FindNextSpawnLocation(out SpawnLocation location)
        {
            // Rrefresh all available spawn locations
            UpdateAvailableSpawnLocations();

            // Get the available count
            int availableCount = AvailableSpawnableItemCapacity;

            // Check for error
            if(availableCount == 0)
            {
                location = SpawnLocation.invalid;
                return false;
            }

            // Select a random spawn location
            SpawnLocation resultLocation = SpawnLocation.invalid;
            int index = Random.Range(0, availableCount);
            int current = 0;

            // Find the first available location
            foreach(SpawnAreaLocation areaLocation in spawnLocations)
            {
                if(areaLocation.isOccupied == false)
                {
                    if(index == current)
                    {
                        // Found the location
                        resultLocation = areaLocation.location;
                        break;
                    }
                    current++;
                }
            }

            // Default is success
            location = resultLocation;
            return resultLocation.IsValid;
        }

        private void SetAllSpawnLocationsOccupied(bool occupied)
        {
            foreach (SpawnAreaLocation areaLocation in spawnLocations)
                areaLocation.isOccupied = occupied;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity.
        /// Draws the spawn point in the editor.
        /// </summary>
        public void OnDrawGizmos()
        {
            RebuildSpawnLocations();
            UpdateAvailableSpawnLocations();
            
            // Setup colors
            Color availableInner = availableColor;
            Color unavailableInner = unavailableColor;
            availableInner.a = 0.6f;
            unavailableInner.a = 0.6f;


            // Find the blen amount
            float colorBlend = Mathf.InverseLerp(0, SpawnableItemCapacity, AvailableSpawnableItemCapacity);

            // Find half ceiling height
            float height = (isAboveGround == true) ? (ceilingHeight / 2f) : 0;

            // Get planar sizes
            Vector3 outerPlaneSize = size;
            Vector3 innerPlaneSize = new Vector2(LocalPlane.width, LocalPlane.height);

            // Check for 3d spawner
            if(is2DSpawner == false)
            {
                outerPlaneSize = new Vector3(size.x, 0, size.y);
                innerPlaneSize = new Vector3(LocalPlane.width, 0, LocalPlane.height);
            }

            // Setup matrices
            Matrix4x4 mat = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Gizmos.matrix = mat;
            Handles.matrix = mat;

            // Draw bounding volume
            Color bounds = Color.Lerp(unavailableInner, availableInner, colorBlend);
            bounds.a = 0.1f;

            Gizmos.color = bounds;
            Gizmos.DrawCube(new Vector3(0, height, 0), new Vector3(outerPlaneSize.x, ceilingHeight, outerPlaneSize.z));


            // Draw inner spawning plane
            Gizmos.color = Color.Lerp(unavailableInner, availableInner, colorBlend);
            Gizmos.DrawCube(Vector3.zero, innerPlaneSize);
            
            // Draw outer bounds plane
            Gizmos.color = Color.Lerp(unavailableColor, availableColor, colorBlend);
            Gizmos.DrawCube(Vector3.zero, outerPlaneSize);
            
            // Draw outline
            Gizmos.color = highlightColor;
            Gizmos.DrawWireCube(Vector3.zero, outerPlaneSize);
            

            if (is2DSpawner == false)
            {
                // Draw direction indicator
                Gizmos.color = highlightColor;
                
                // Draw the direction triangle
                DrawGizmoTriangle(transform.position + (transform.forward * size.y / 1.67f),
                    transform.rotation * Quaternion.Euler(90, 0, 0),
                    new Vector2(1, 0.5f));
            }


            // Check for selection
            if (Selection.activeGameObject == gameObject)
            {
                Handles.color = selectedColor;
                Handles.DrawWireCube(Vector3.zero, outerPlaneSize);
                Handles.DrawWireCube(Vector3.zero, outerPlaneSize + new Vector3(0.02f, 0.02f, 0.02f));
            }

#if ULTIMATESPAWNER_DEBUG
            if (drawLocationGizmos == true)
            {
                // Draw spawn location discs
                Handles.matrix = Matrix4x4.identity;
                Handles.color = Color.white;

                foreach (SpawnAreaLocation location in spawnLocations)
                {
                    Vector3 offset = new Vector3(0, height, 0);

                    if (is2DSpawner == true)
                        Handles.DrawWireDisc(location.location.SpawnPosition - offset, Vector3.back, spawnRadius);
                    else
                        Handles.DrawWireDisc(location.location.SpawnPosition - offset, Vector3.up, spawnRadius);
                }
            }
#endif
        }
#endif
    }
}
