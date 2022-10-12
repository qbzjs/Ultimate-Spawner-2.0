using UnityEngine;
using UnityEngine.AI;

namespace UltimateSpawner.Spawning
{
    public enum SpawnNavMeshMode
    {
        Random,
        NearestTargetRanged,
        NearestTargetWithTagRanged,
        NearestTargetMaximum,
        NearestTargetWithTagMaximum,
        FurthestTargetRanged,
        FurthestTargetWithTagRanged,
        FurthestTargetMaximum,
        FurthestTargetWithTagMaximum,
        RandomTargetRanged,
        RandomTargetWithTagRanged,
        RandomTargetMaximum,
        RandomTargetWithTagMaximum,
    }

    /// <summary>
    /// A <see cref="SpawnNavMesh"/> is a special end point spawner that works in conjunction with a baked navigation mesh in the scene. 
    /// The baked nav mesh is used to find a random spawn location within walkable areas of the game level.
    /// </summary>
    public sealed class SpawnNavMesh : EndPointSpawner
    {
        // Private
        private Bounds navMeshBounds;
        private Vector3 navMeshOffset;
        private float navMeshRange;


        // Public
        /// <summary>
        /// Should the final spawn location be raised above the navmesh by '0.5' units.
        /// </summary>
        public bool isAboveGround = true;
        /// <summary>
        /// Should the random rotation be applied to the spawned item.
        /// </summary>
        public SpawnRotationApplyMode applyRandomRotation = SpawnRotationApplyMode.YRotation;

        /// <summary>
        /// The method used to determine whether the <see cref="SpawnNavMesh"/> location is occupied or not.   
        /// </summary>
        [Tooltip("The method used to determine whether the selected nav mesh location is occupied or not")]
        public OccupiedCheck occupiedCheck = OccupiedCheck.None;

        [DisplayCondition("occupiedCheck", OccupiedCheck.PhysicsOverlap, ConditionType.Equal)]
        public float spawnRadius = 0.5f;

        /// <summary>
        /// The layer that should be used for all collision checks.
        /// Only used when <see cref="occupiedCheck"/> is set to <see cref="OccupiedCheck.PhysicsOverlap"/> or <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("The layer to check collisions on for occupied checks")]
        [DisplayCondition("occupiedCheck", OccupiedCheck.PhysicsOverlap, ConditionType.Equal)]
        public LayerMask collisionLayer = 1;

        [DisplayCondition("occupiedCheck", OccupiedCheck.PhysicsOverlap, ConditionType.Equal)]
        public int maxSearchAttemptsPerFrame = 10;

        [DisplayCondition("occupiedCheck", OccupiedCheck.PhysicsOverlap, ConditionType.Equal)]
        public bool allowTriggerOccupiedChecks = false;



        public SpawnNavMeshMode navMeshMode = SpawnNavMeshMode.Random;

        [DisplayConditionMethod("IsSpawnModeRangedOrMaximum", DisplayType.Hidden)]
        public float maxDistanceToTarget = 16;

        [DisplayConditionMethod("IsSpawnModeRanged", DisplayType.Hidden)]
        public float minDistanceFromTarget = 6;


#if UNITY_EDITOR
        [DisplayConditionMethod("IsSpawnModeRangedOrMaximum", DisplayType.Hidden, typeof(TagCollectionDrawer))]
#endif
        public string spawnerTargetTag = null;


        /// <summary>
        /// A value used to control how close to the edge of the navmesh an item can spawn. The default value of 1 allows for the best spread but in square areas may cause items to be grouped near the edge.
        /// In that case, a smaller value may be useful to shift item spawning away from the edge of the navmesh adn closer to the center.
        /// </summary>
        public float navMeshEdgeFactor = 1f;

        /// <summary>
        /// A mask value used to filter specific NavMeshes during spawn requests.
        /// </summary>
        [NavMeshArea]
        public int areaMask = -1;

        // Properties
        /// <summary>
        /// Returns true if the <see cref="SpawnNavMesh"/> is not occupied or false if it is occupied. 
        /// Note that occupied checks are not supported by a <see cref="SpawnNavMesh"/> component so this value will always return true.
        /// </summary>
        public override bool IsAvailable
        {
            get { return true; } // Occupied check is not supported on navmesh 
        }

        /// <summary>
        /// Returns the number of items that this <see cref="SpawnNavMesh"/> can accomodate.
        /// Note that an infinite number of items can be spawned by a <see cref="SpawnNavMesh"/> so this value will always return '999'.
        /// </summary>
        public override int SpawnableItemCapacity
        {
            get { return 999; }
        }

        /// <summary>
        /// Returns the number of available spawn locations for this <see cref="SpawnNavMesh"/>.
        /// Note that occupied checks are not supported by a <see cref="SpawnNavMesh"/> component and an infinite number of items can be spawned so this value will always return '999'.
        /// </summary>
        public override int AvailableSpawnableItemCapacity
        {
            get { return 999; }
        }

        public int AreaMaskValue
        {
            get
            {
                if (areaMask == NavMesh.AllAreas)
                    return NavMesh.AllAreas;

                return 1 << areaMask;
            }
        }

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public override void Awake()
        {
            // Be sure to call the base method
            base.Awake();

            // Update the navmesh boundaries
            RebuildBounds();
        }

#if UNITY_EDITOR
        public override void OnValidate()
        {
            base.OnValidate();

            if (minDistanceFromTarget > maxDistanceToTarget)
                minDistanceFromTarget = maxDistanceToTarget;

            if (maxDistanceToTarget < minDistanceFromTarget)
                maxDistanceToTarget = minDistanceFromTarget;
        }
#endif

        /// <summary>
        /// Attempt to spawn a <see cref="SpawnableItem"/> at this <see cref="SpawnNavMesh"/>.
        /// This method will only fail if there is no baked navigation mesh in the scene. Occupied checks are not supported.
        /// This method will automatically create a <see cref="SpawnableItem"/> using the settings specified in the inspector. 
        /// </summary>
        /// <param name="itemRef">An item reference of the spawnable item that should be spawned if possible. This item can only be spawned if it is not masked and can be resolved. Use null to select a spawnable item randomly using the spawn chance value</param>
        /// <returns>The transform of the newly spawned item or null if the spawn failed</returns>
        public override Transform Spawn(SpawnableItemRef itemRef = null)
        {
            // Try to get a spawn location
            SpawnLocation location = GetLocation();

            // Check for error
            if (location.IsValid == false)
            {
                Debug.LogWarning("Failed to spawn - No valid NavMesh location could be sampled from the active scene. Make sure navigation meshes are baked");
                return null;
            }

            // Create the item
            Transform result = CreateSpawnableItem(itemRef, location, applyRandomRotation);

            if (result == null)
            {
                Debug.LogWarning("Failed to create spawnable item");
                return null;
            }

            // Spawn the item
            return result;
        }

        /// <summary>
        /// Attempt to spawn the specified object transform at this <see cref="SpawnNavMesh"/>. 
        /// This method will only fail if there is no baked navigation mesh in the scene. Occupied checks are not supported.
        /// </summary>
        /// <param name="toSpawn">The transform component of the object to spawn</param>
        /// <returns>True if the spawn was successful or false if the <see cref="SpawnNavMesh"/> does not have any baked navigation data</returns>
        public override bool Spawn(Transform toSpawn)
        {
            // Try to get a spawn location
            SpawnLocation location = GetLocation();

            // Check for error
            if (location.IsValid == false)
            {
                Debug.LogWarning("Failed to spawn - No valid NavMesh location could be sampled from the active scene. Make sure navigation meshes are baked");
                return false;
            }

            // Spawn the object - Dont check availablity because nav mesh spawners are always available
            return location.Spawn(toSpawn, applyRandomRotation);
        }

        /// <summary>
        /// Get the next <see cref="SpawnLocation"/> for this <see cref="SpawnNavMesh"/>.  
        /// </summary>
        /// <returns>The <see cref="SpawnLocation"/> representing the position and rotation of the next <see cref="SpawnNavMesh"/> location</returns>
        public override SpawnLocation GetLocation()
        {
# if UNITY_EDITOR
            // Force rebuild bounds
            if (Application.isPlaying == false)
                RebuildBounds();
#endif

            int searchAttempts = (maxSearchAttemptsPerFrame > 0) ? maxSearchAttemptsPerFrame : 1;
            int iterationCounter = 0;

            // Check for occupied check mode
            if (occupiedCheck == OccupiedCheck.PhysicsTrigger) Debug.LogWarning("'PhysicsTrigger' occupied check is not supported by the nav mesh spawner. Use 'PhysicsOverlap' or 'None'");


            // Loop until we find a suitable location
            while (iterationCounter < searchAttempts)
            {
                Vector3 samplePosition = GetSampleLocation();

                // Find a random rotation
                Quaternion randomRotation = Random.rotation;

                NavMeshHit hit;

                // Sample the navmesh
                if (NavMesh.SamplePosition(samplePosition + navMeshOffset, out hit, navMeshRange, AreaMaskValue) == true)
                {
                    samplePosition = hit.position;

                    // Move the item above ground
                    if (isAboveGround == true)
                        samplePosition += new Vector3(0, 0.5f, 0);

                    // Check for occupied
                    if (occupiedCheck == OccupiedCheck.None)
                    {
                        // Create the spawn location
                        return new SpawnLocation(this, samplePosition, randomRotation);
                    }
                    else
                    {
                        if (IsOccupied(samplePosition) == false)
                        {
                            // Create the spawn location
                            return new SpawnLocation(this, samplePosition, randomRotation);
                        }
                    }
                }

                // Increment loop counter
                iterationCounter++;
            }

            if (iterationCounter >= searchAttempts)
                Debug.LogWarning("Failed to find suitable nav mesh spawn location within the allowed number of iterations");

            // Failed to get a valid location - probably no navmesh
            return SpawnLocation.invalid;
        }

        /// <summary>
        /// Force the bounds of the active navigation meshes in the active scene to be calcualted. 
        /// These bounds are then used for all subsequent spawn requests.
        /// </summary>
        public void RebuildBounds()
        {
            float searchScope = 0xFFFF;

            navMeshBounds = new Bounds();
            NavMeshHit hit;

            // Sample left
            if (NavMesh.SamplePosition(Vector3.left * searchScope, out hit, searchScope, AreaMaskValue) == true)
                navMeshBounds.min = new Vector3(hit.position.x, navMeshBounds.min.y, navMeshBounds.min.z);

            // Sample right
            if (NavMesh.SamplePosition(Vector3.right * searchScope, out hit, searchScope, AreaMaskValue) == true)
                navMeshBounds.max = new Vector3(hit.position.x, navMeshBounds.max.y, navMeshBounds.max.z);

            // Sample forward
            if (NavMesh.SamplePosition(Vector3.forward * searchScope, out hit, searchScope, AreaMaskValue) == true)
                navMeshBounds.max = new Vector3(navMeshBounds.max.x, navMeshBounds.max.y, hit.position.z);

            // Sample backward
            if (NavMesh.SamplePosition(Vector3.back * searchScope, out hit, searchScope, AreaMaskValue) == true)
                navMeshBounds.min = new Vector3(navMeshBounds.min.x, navMeshBounds.min.y, hit.position.z);


            // Calcualte offset and range of the navmesh
            navMeshOffset = navMeshBounds.center;
            navMeshRange = (navMeshBounds.extents.x + navMeshBounds.extents.z) / 2;

            // Calcualte hypotenuse for corner coverage
            navMeshRange = Mathf.Sqrt(navMeshRange * navMeshRange + navMeshRange * navMeshRange) * navMeshEdgeFactor;

            if (navMeshRange == 0)
                Debug.LogWarning("NavMesh spawning may fail because the bounds of the NavMesh could not be sampled!");
        }

        public bool IsOccupied(Vector3 navMeshPosition)
        {
            if (is2DSpawner == true)
            {
                // Overlap a circle
                int collidersInsideArea = Physics2D.OverlapCircleNonAlloc(navMeshPosition, spawnRadius, sharedCollider2DBuffer, collisionLayer);

                // Check if we  have completley filled the array
                if (collidersInsideArea == sharedBufferSize)
                    Debug.LogWarning("Overlap check has filled the shared buffer. It is possible that some colliders have been missed by the check. You may need to increase the buffer size if your scene has a large amount of physics objects");

                // Check all colliding objects
                for (int i = 0; i < collidersInsideArea; i++)
                {
                    // Only detect non-trigger physics colliders
                    if (allowTriggerOccupiedChecks == true || sharedCollider2DBuffer[i].isTrigger == false)
                    {
                        // The spawn point is occupied
                        return true;
                    }
                }
            }
            else
            {
                // Overlap a sphere
                int collidersInsideArea = Physics.OverlapSphereNonAlloc(navMeshPosition, spawnRadius, sharedColliderBuffer, collisionLayer, allowTriggerOccupiedChecks == true ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);

                // Check if we  have completley filled the array
                if (collidersInsideArea == sharedBufferSize)
                    Debug.LogWarning("Overlap check has filled the shared buffer. It is possible that some colliders have been missed by the check. You may need to increase the buffer size if your scene has a large amount of physics objects");

                // Check all colliding objects
                for (int i = 0; i < collidersInsideArea; i++)
                {
                    // Only detect non-trigger physics colliders
                    if (allowTriggerOccupiedChecks == true || sharedColliderBuffer[i].isTrigger == false)
                    {
                        // The spawn point is occupied
                        return true;
                    }
                }
            }
            return false;
        }

        private Vector3 GetSampleLocation()
        {
            // Get target object
            SpawnerTarget target = GetTarget();

            Vector3 samplePosition = Vector3.zero;

            if (target != null)
            {
                float maxDistance = navMeshRange;

                // Find the max distance to the target
                if (IsSpawnModeRangedOrMaximum() == true)
                    maxDistance = maxDistanceToTarget;

                // Take away the min distance because we wil use that as an offset
                if (IsSpawnModeRanged() == true)
                    maxDistance -= minDistanceFromTarget;

                // Find random position inside range
                Vector3 randomPosition = Random.insideUnitSphere * maxDistance;

                if (Random.Range(0, 1) == 0) randomPosition.x = -randomPosition.x;
                if (Random.Range(0, 1) == 0) randomPosition.z = -randomPosition.z;


                // Find the actual world position
                Vector3 relativeWorldPosition = target.transform.position + randomPosition;

                // Check for ranged, in which case we should make sure that the position is not too close
                if (IsSpawnModeRanged() == true)
                {
                    // Get the unit direction
                    Vector3 unitDirection = randomPosition.normalized;

                    // Get an offset position representing a point on the min distance border in local space
                    Vector3 baseOffset = unitDirection * minDistanceFromTarget;

                    // Calculate the final position
                    relativeWorldPosition = target.transform.position + baseOffset + randomPosition;

                    // Set the sample position
                    samplePosition = relativeWorldPosition;
                }
            }
            else
            {
                // Find a random position
                Vector3 randomPosition = Random.insideUnitSphere * navMeshRange;

                // Set the sample position
                samplePosition = randomPosition;
            }

            return samplePosition;
        }

        private SpawnerTarget GetTarget()
        {
            string tag = null;

            // Select the correct tag
            if (IsSpawnModeTagged() == true)
                tag = spawnerTargetTag;

            if (IsSpawnModeNearest() == true)
            {
                return SpawnerTarget.FindNearestSpawnerTarget(transform.position, tag);
            }
            else if (IsSpawnModeFurthest() == true)
            {
                return SpawnerTarget.FindFarthestSpawnerTarget(transform.position, tag);
            }
            else if (IsSpawnModeRandom() == true)
            {
                return SpawnerTarget.FindRandomSpawnerTarget(tag);
            }
            return null;
        }

        private bool IsSpawnModeRangedOrMaximum()
        {
            return IsSpawnModeRanged() == true || IsSpawnModeMaximum() == true;
        }

        public bool IsSpawnModeDistanceBased()
        {
            return navMeshMode != SpawnNavMeshMode.Random;
        }

        private bool IsSpawnModeTagged()
        {
            switch (navMeshMode)
            {
                case SpawnNavMeshMode.FurthestTargetWithTagMaximum:
                case SpawnNavMeshMode.FurthestTargetWithTagRanged:
                case SpawnNavMeshMode.NearestTargetWithTagMaximum:
                case SpawnNavMeshMode.NearestTargetWithTagRanged:
                case SpawnNavMeshMode.RandomTargetWithTagMaximum:
                case SpawnNavMeshMode.RandomTargetWithTagRanged:
                    return true;
            }
            return false;
        }

        private bool IsSpawnModeRanged()
        {
            switch (navMeshMode)
            {
                case SpawnNavMeshMode.FurthestTargetRanged:
                case SpawnNavMeshMode.FurthestTargetWithTagRanged:
                case SpawnNavMeshMode.NearestTargetRanged:
                case SpawnNavMeshMode.NearestTargetWithTagRanged:
                case SpawnNavMeshMode.RandomTargetRanged:
                case SpawnNavMeshMode.RandomTargetWithTagRanged:
                    return true;
            }
            return false;
        }

        private bool IsSpawnModeMaximum()
        {
            switch (navMeshMode)
            {
                case SpawnNavMeshMode.FurthestTargetMaximum:
                case SpawnNavMeshMode.FurthestTargetWithTagMaximum:
                case SpawnNavMeshMode.NearestTargetMaximum:
                case SpawnNavMeshMode.NearestTargetWithTagMaximum:
                case SpawnNavMeshMode.RandomTargetMaximum:
                case SpawnNavMeshMode.RandomTargetWithTagMaximum:
                    return true;
            }
            return false;
        }

        private bool IsSpawnModeNearest()
        {
            switch (navMeshMode)
            {
                case SpawnNavMeshMode.NearestTargetMaximum:
                case SpawnNavMeshMode.NearestTargetRanged:
                case SpawnNavMeshMode.NearestTargetWithTagMaximum:
                case SpawnNavMeshMode.NearestTargetWithTagRanged:
                    return true;
            }
            return false;
        }

        private bool IsSpawnModeFurthest()
        {
            switch (navMeshMode)
            {
                case SpawnNavMeshMode.FurthestTargetMaximum:
                case SpawnNavMeshMode.FurthestTargetRanged:
                case SpawnNavMeshMode.FurthestTargetWithTagMaximum:
                case SpawnNavMeshMode.FurthestTargetWithTagRanged:
                    return true;
            }
            return false;
        }

        private bool IsSpawnModeRandom()
        {
            switch (navMeshMode)
            {
                case SpawnNavMeshMode.RandomTargetMaximum:
                case SpawnNavMeshMode.RandomTargetRanged:
                case SpawnNavMeshMode.RandomTargetWithTagMaximum:
                case SpawnNavMeshMode.RandomTargetWithTagRanged:
                    return true;
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(navMeshBounds.center, navMeshRange);
        }
    }
}
