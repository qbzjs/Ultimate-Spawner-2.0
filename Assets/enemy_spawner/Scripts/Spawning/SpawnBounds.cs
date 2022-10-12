using System;
using UltimateSpawner.Util;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateSpawner.Spawning
{
    public class SpawnBounds : EndPointSpawner
    {
        // Public
        /// <summary>
        /// Should the rotation of the spawner be used to rotate the spawned item to the same orientation.
        /// After changing this value via code you should call <see cref="RebuildColliders"/> to update the colliders if the <see cref="occupiedCheck"/> is equal to <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("Should the spawner apply the rotation of its transform component to the spawned item or should only the position element of the transform be modified")]
        public SpawnRotationApplyMode applyRotation = SpawnRotationApplyMode.FullRotation;

        public SpawnRotationApplyMode applyRandomRotation = SpawnRotationApplyMode.NoRotation;

        public Bounds spawnBounds = new Bounds(Vector3.zero, Vector3.one);

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

            if (result == null)
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
            if (location.IsValid == false)
            {
                Debug.LogWarning("Failed to spawn - No valid collider location could be sampled from the active scene. Make sure collider components are attached");
                return false;
            }

            // Try to spawn the transform at the location
            return location.Spawn(toSpawn, applyRotation);
        }

        public override SpawnLocation GetLocation()
        {
            // Select a random position within the bounds
            Vector3 localPoint = SpawnRandomizer.GetRandomPosition(spawnBounds);

            // Randomly interpolate beteen points
            Vector3 finalPoint = transform.TransformPoint(localPoint);

            // Get the random rotation
            Quaternion rotation = SpawnerUtility.GetRotationForSpawnable(transform, applyRotation, applyRandomRotation);

            // Create the final spawn location
            return new SpawnLocation(this, finalPoint, rotation);
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(spawnBounds.center, spawnBounds.size);
        }
    }
}
