using System;
using UltimateSpawner.Util;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateSpawner.Spawning
{
    public class SpawnColliderBounds : EndPointSpawner
    {
        // Private
        private Collider collider3D = null;

        // Public
        /// <summary>
        /// Should the rotation of the spawner be used to rotate the spawned item to the same orientation.
        /// After changing this value via code you should call <see cref="RebuildColliders"/> to update the colliders if the <see cref="occupiedCheck"/> is equal to <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("Should the spawner apply rotation to the spawned item or should only the position element of the transform be modified")]
        public SpawnRotationApplyMode applyRotation = SpawnRotationApplyMode.FullRotation;

        public SpawnRotationApplyMode applyRandomRotation = SpawnRotationApplyMode.NoRotation;

        // Properties
        public override bool IsAvailable
        {
            get { return true; }
        }

        public override int SpawnableItemCapacity
        {
            get { return 999; }
        }

        public override int AvailableSpawnableItemCapacity
        {
            get { return 999; }
        }

        // Methods
        public override void Awake()
        {
            base.Awake();

            // Try to get collider
            collider3D = GetComponent<Collider>();

            // Check for none
            if (collider3D == null)
                Debug.LogWarningFormat("Spawner '{0}' will notaccept spawn request because a collider component has not been attached", ToString());
        }

        public override Transform Spawn(SpawnableItemRef itemRef = null)
        {
            SpawnLocation location = GetLocation();

            // Check for error
            if (location.IsValid == false)
            {
                Debug.LogWarning("Failed to spawn - No valid collider location could be sampled from the active scene. Make sure collider components are attached");
                return null;
            }

            // Create the item
            Transform result = CreateSpawnableItem(itemRef, location, applyRotation);

            if(result == null)
            {
                Debug.LogWarning("Failed to create spawnable item");
                return null;
            }

            // Spawn the item
            return result;
        }

        public override bool Spawn(Transform toSpawn)
        {
            SpawnLocation location = GetLocation();

            // Check for error
            if(location.IsValid == false)
            {
                Debug.LogWarning("Failed to spawn - No valid collider location could be sampled from the active scene. Make sure collider components are attached");
                return false;
            }

            // Try to spawn the transform at the location
            return location.Spawn(toSpawn, applyRotation);
        }

        public override SpawnLocation GetLocation()
        {
            // Check for no colliders
            if (/*(is2DSpawner == true && collider2D == null) || */(is2DSpawner == false && collider3D == null))
                return SpawnLocation.invalid;

            Bounds colliderBounds = new Bounds();// collider2D.bounds;

            // Select 3d bounds if spawner is in 3d mode
            if (is2DSpawner == false)
                colliderBounds = collider3D.bounds;

            if (is2DSpawner == true)
            {
                throw new NotSupportedException("SpawnBounds is not supported for 2D colliders in this Unity version");
            }

            Vector3 randomLocalPoint1 = SpawnRandomizer.GetRandomPosition(colliderBounds);
            Vector3 randomLocalPoint2 = SpawnRandomizer.GetRandomPosition(colliderBounds);

            // Get 2 random points on the collider surface
            Vector3 point1 = collider3D.ClosestPoint(randomLocalPoint1);
            Vector3 point2 = collider3D.ClosestPoint(randomLocalPoint2);

            // Randomly interpolate beteen points
            Vector3 finalLocalPoint = Vector3.Lerp(point1, point2, Random.Range(0f, 1f));

            // Transform to world poisiton
            Vector3 finalPoint = transform.TransformPoint(finalLocalPoint);

            // Get the end rotation
            Quaternion rotation = SpawnerUtility.GetRotationForSpawnable(transform, applyRotation, applyRandomRotation);

            // Create the final spawn location
            return new SpawnLocation(this, finalPoint, rotation);
        }
    }
}
