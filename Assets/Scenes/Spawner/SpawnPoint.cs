using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// A <see cref="SpawnPoint"/> represents a position and rotation in 3D or 2D space where a <see cref="SpawnableItem"/> can be spawned.
    /// The location is represented by a sphere volume which is used to determine whether or not the location is available for spawning.
    /// This ensures that multiple items cannot be spawned overlapping.
    /// </summary>
    public sealed class SpawnPoint : EndPointSpawner
    {
        // Internal
#if UNITY_EDITOR
        internal InspectorValueWatcher is2DSpawnerWatcher = new InspectorValueWatcher("is2DSpawner");
#endif

        // Private
        [SerializeField, HideInInspector]
        private SpawnLocation location;                                         // Cached spawn info for this spawn point
        private HashSet<Collider> colliding3D = new HashSet<Collider>();        // 3D colliders that are inside the spawn point
        private HashSet<Collider2D> colliding2D = new HashSet<Collider2D>();    // 2D colliders that are inside the spawn point
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
        /// The radius of the <see cref="SpawnPoint"/> which is used in occupied checks.
        /// After changing this value via code you should call <see cref="RebuildColliders"/> to update the colliders if the <see cref="occupiedCheck"/> is equal to <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("The radius of the spawn point. Used for occupied checks")]
        [SerializeField]
        private float spawnRadius = 0.5f;

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
        /// The radius of the spawn point used for occupied checks.
        /// </summary>
        public float SpawnRadius
        {
            get { return spawnRadius; }
            set
            {
                spawnRadius = value;
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
        /// Returns true if the <see cref="SpawnPoint"/> is not occupied or false if it is occupied. 
        /// </summary>
        public override bool IsAvailable
        {
            get { return IsOccupied() == false; }
        }

        /// <summary>
        /// Returns the number of items that this <see cref="SpawnPoint"/> can accomodate.
        /// This value will always return '1'.
        /// </summary>
        public override int SpawnableItemCapacity
        {
            get { return 1; }
        }

        /// <summary>
        /// Returns the number of available spawn locations for this <see cref="SpawnPoint"/>.
        /// This value will return 1 when the <see cref="SpawnPoint"/> is unoccupied or 0 when it is occupied. 
        /// </summary>
        public override int AvailableSpawnableItemCapacity
        {
            get
            {
                // Check for available
                if (IsAvailable == true)
                    return 1;

                return 0;
            }
        }

        /// <summary>
        /// Get the center position of the spawn point.
        /// The center will be calculated taking into account the value of <see cref="isAboveGround"/>. 
        /// </summary>
        public Vector3 Center
        {
            get
            {
                // Get the default position
                Vector3 center = transform.position;

                // Move the spawner up above ground
                if (isAboveGround == true)
                    center.y += spawnRadius;

                // Get th center
                return center;
            }
        }

        // Methods
#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity when the component is added.
        /// </summary>
        public override void Reset()
        {
            // Call the base method
            base.Reset();

            // Update the default value for 2d spawn point rotation
            if (is2DSpawner == true)
                applyRotation = SpawnRotationApplyMode.ZRotation;

            // Rebuild when added to scene
            Rebuild();
        }

        /// <summary>
        /// Called by Unity when one of the serialzied fields are changed.
        /// </summary>
        public override void OnValidate()
        {
            // Call the base method
            base.OnValidate();

            // Check for changed values
            if (is2DSpawnerWatcher.HasChanged(this) == true)
            {
                // Udpate rotation mode
                if (is2DSpawner == true)
                    applyRotation = SpawnRotationApplyMode.ZRotation;
                else
                    applyRotation = SpawnRotationApplyMode.FullRotation;
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

            // Create the location
            location = new SpawnLocation(this, Center, transform.rotation);

            // Check mode
            if (occupiedCheck == OccupiedCheck.PhysicsTrigger)
            {
                // Create the necessary physics components
                RebuildColliders();
            }

            // Rebuild if necessary
            if (location.IsValid == false)
                Rebuild();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Update()
        {
            // Check if the spawn point is able to move dynamically
            if(isStatic == false)
            {
                // If the spawn point can move then we need to ensure that the spawn location keeps in sync
                Rebuild();
            }
        }

        /// <summary>
        /// Attempt to spawn a <see cref="SpawnableItem"/> at this <see cref="SpawnPoint"/>.
        /// This method will automatically create a <see cref="SpawnableItem"/> using the settings specified in the inspector. 
        /// </summary>
        /// <param name="itemRef">An item reference of the spawnable item that should be spawned if possible. This item can only be spawned if it is not masked and can be resolved. Use null to select a spawnable item randomly using the spawn chance value</param>
        /// <returns>The transform of the newly spawned item or null if the spawn failed</returns>
        public override Transform Spawn(SpawnableItemRef itemRef = null)
        {
#if UNITY_EDITOR
            // Initialize the spawn location
            location = new SpawnLocation(this, transform.position, transform.rotation);

            Rebuild();
#endif

            // Make sure the spawner is available
            if (IsAvailable == false)
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
        /// Attempt to spawn the specified object transform at this <see cref="SpawnPoint"/>. 
        /// </summary>
        /// <param name="toSpawn">The transform component of the object to spawn</param>
        /// <returns>True if the spawn was successful or false if the <see cref="SpawnPoint"/> is not available</returns>
        public override bool Spawn(Transform toSpawn)
        {
            // Make sure the spawn point is available
            if (IsAvailable == false)
                return false;

            // Spawn the object - Dont check availablity because we already have
            return location.Spawn(toSpawn, applyRotation);
        }

        /// <summary>
        /// Get the <see cref="SpawnLocation"/> for this <see cref="SpawnPoint"/>.  
        /// </summary>
        /// <returns>The <see cref="SpawnLocation"/> representing the position and rotation of this <see cref="SpawnPoint"/></returns>
        public override SpawnLocation GetLocation()
        {
            // Get cached spawn info
            return location;
        }

        /// <summary>
        /// Enumerate all child spawners.
        /// Note that a <see cref="SpawnPoint"/> cannot have any child spawners. 
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Spawner> GetEnumerator()
        {
            // No children
            yield break;
        }

        /// <summary>
        /// Forces the spawn point to resynchronize.
        /// Use this method after you move a spawn point that has <see cref="IsStatic"/> set to true. 
        /// </summary>
        public void Rebuild()
        {
            // Update our spawn info with the correct location
            location.Update(Center, transform.rotation);
        }

        /// <summary>
        /// Force the spawn point to regenerate the colliders it uses for occupied checks.
        /// This will cause physics components to be added as needed depending upon the current <see cref="OccupiedCheck"/> value. 
        /// </summary>
        public void RebuildColliders()
        {
            // Should only be used at runtime
            if (Application.isPlaying == true)
            {
                // Destroy any exists collider components - This must be done in the current frame otherwise the next frame will fail to add physics components due to conflicts
                while (createdComponents.Count > 0)
                    Destroy(createdComponents.Pop());


                // Check for trigger mode
                if (occupiedCheck == OccupiedCheck.PhysicsTrigger)
                {
                    // Configure colliders
                    if (is2DSpawner == true)
                    {
                        // We need a collider to be present
                        Collider2D collider = GetComponent<Collider2D>();

                        // Create a collider if one does not exist
                        if (collider == null)
                        {
                            // Add the component and register with this spawn point
                            collider = gameObject.AddComponent<CircleCollider2D>();
                            createdComponents.Push(collider);
                        }

                        // Check for circle collider
                        CircleCollider2D circle = collider as CircleCollider2D;

                        if (circle != null)
                        {
                            // Update the radius
                            circle.radius = spawnRadius;
                            circle.isTrigger = true;

                            // Check for above ground
                            if (isAboveGround == true)
                                circle.offset = new Vector2(0, spawnRadius);
                        }
                    }
                    else
                    {
                        // We need a collider to be present
                        Collider collider = GetComponent<Collider>();

                        // Create a collider if one does not exist
                        if (collider == null)
                        {
                            // Add the component and register with this spawn point
                            collider = gameObject.AddComponent<SphereCollider>();
                            createdComponents.Push(collider);
                        }

                        // Check for sphere collider
                        SphereCollider sphere = collider as SphereCollider;

                        if (sphere != null)
                        {
                            // Update the radius
                            sphere.center = new Vector3(0, (isAboveGround == true) ? spawnRadius : 0, 0);
                            sphere.radius = spawnRadius;
                            sphere.isTrigger = true;

                            // Check for above ground
                            if (isAboveGround == true)
                                sphere.center = new Vector3(0, spawnRadius, 0);
                        }
                    }
                }
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
            if(colliding3D.Contains(other) == false)
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
            if(colliding3D.Contains(other) == true)
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
            if(colliding2D.Contains(other) == false)
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
            if(colliding2D.Contains(other) == true)
            {
                // Remove from colliding list
                colliding2D.Remove(other);
            }
        }
#endregion

        private bool IsOccupied()
        {
            // Gt the check mode
            OccupiedCheck check = occupiedCheck;

            // Always use overlap sphere mode in editor
            if (Application.isPlaying == false)
                check = OccupiedCheck.PhysicsOverlap;

            // Perform check based on specified method
            switch(check)
            {
                // Default check always returns true
                default:
                case OccupiedCheck.None:
                    break;

                // Use the ovelap sphere method
                case OccupiedCheck.PhysicsOverlap:
                    {
                        if(is2DSpawner == true)
                        {
                            // Overlap a circle
                            int collidersInsideArea = Physics2D.OverlapCircleNonAlloc(Center, spawnRadius, sharedCollider2DBuffer, collisionLayer);

                            // Check if we  have completley filled the array
                            if (collidersInsideArea == sharedBufferSize)
                                Debug.LogWarning("Overlap check has filled the shared buffer. It is possible that some colliders have been missed by the check. You may need to increase the buffer size if your scene has a large amount of physics objects");
                            
                            // Check all colliding objects
                            for (int i = 0; i < collidersInsideArea; i++)
                            {
                                // Only detect non-trigger physics colliders
                                if(sharedCollider2DBuffer[i].isTrigger == false)
                                {
                                    // The spawn point is occupied
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            // Overlap a sphere
                            int collidersInsideArea = Physics.OverlapSphereNonAlloc(Center, spawnRadius, sharedColliderBuffer, collisionLayer);

                            // Check if we  have completley filled the array
                            if (collidersInsideArea == sharedBufferSize)
                                Debug.LogWarning("Overlap check has filled the shared buffer. It is possible that some colliders have been missed by the check. You may need to increase the buffer size if your scene has a large amount of physics objects");
                            
                            // Check all colliding objects
                            for (int i = 0; i < collidersInsideArea; i++)
                            {
                                // Only detect non-trigger physics colliders
                                if (sharedColliderBuffer[i].isTrigger == false)
                                {
                                    // The spawn point is occupied
                                    return true;
                                }
                            }
                        }
                        break;
                    }

                    // Use the trigger method
                case OccupiedCheck.PhysicsTrigger:
                    {
                        // Get the number of colliding objects
                        int count = colliding2D.Count + colliding3D.Count;

                        // Expect 0 for success
                        return count != 0;
                    }
            }

            // Default is success
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Called by Unity.
        /// Draws the spawn point in the editor.
        /// </summary>
        public void OnDrawGizmos()
        {
            // Always update in editor
            Rebuild();


            // Calcualte the camera normal
            Vector3 camNormal = Vector3.back;

            // Find the vector from the object to the editor camera
            if (is2DSpawner == false && Camera.current != null)
                camNormal = (transform.position - Camera.current.transform.position).normalized;


            if (is2DSpawner == true)
            {
                // Create the scaled matrix
                Matrix4x4 scale = Matrix4x4.Scale(new Vector3(1f, 1f, 0f));

                Gizmos.matrix = scale;
                Handles.matrix = scale;
            }

            // Draw main sphere
            Gizmos.color = (IsAvailable == true) ? availableColor : unavailableColor;
            Gizmos.DrawSphere(Center, spawnRadius);

            // Draw outline
            Handles.color = highlightColor;
            Handles.DrawWireDisc(Center, camNormal, spawnRadius - 0.01f);

            if (is2DSpawner == false)
            {
                // Draw underline
                Handles.color = highlightColor;
                Handles.DrawWireDisc(transform.position, Vector3.up, spawnRadius);
            
                // Draw direction indicator
                Gizmos.color = highlightColor;
                
                DrawGizmoTriangle(transform.position + (transform.forward * (spawnRadius * 1.5f)),
                    transform.rotation * Quaternion.Euler(90, 0, 0),
                    new Vector2(0.5f, 0.25f));
            }

            // Check for selection
            if (Selection.activeGameObject == gameObject)
            {
                Handles.color = selectedColor;
                Handles.DrawWireDisc(Center, camNormal, spawnRadius);
                Handles.DrawWireDisc(Center, camNormal, spawnRadius - 0.01f);
            }
        }
#endif
    }
}