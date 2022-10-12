using System.Collections.Generic;
using UnityEngine;

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// A <see cref="SpawnerTarget"/> is required when any <see cref="Spawner"/> component uses the <see cref="SpawnMode.NearestTarget"/> or <see cref="SpawnMode.FarthestTarget"/> mode to spawn items.
    /// The target should be attached to the player object which will then allows enemies to spawn near ar far away from the player.
    /// If there are multiple players in the scene each with their own <see cref="SpawnerTarget"/> then the spawning system will simply select a random target for each spawn. 
    /// </summary>
    public sealed class SpawnerTarget : MonoBehaviour
    {
        // Private
        private static HashSet<SpawnerTarget> allTargets = new HashSet<SpawnerTarget>();

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Awake()
        {
            // Register this as a target
            allTargets.Add(this);
        }

        /// <summary>
        /// Called by unity.
        /// </summary>
        public void OnDestroy()
        {
            // un-register this as a target
            allTargets.Remove(this);
        }

        /// <summary>
        /// Select a random <see cref="SpawnerTarget"/> from all active instances in the scene.
        /// </summary>
        /// <returns>A random <see cref="SpawnerTarget"/> or null if there are no active instances in the scene</returns>
        public static SpawnerTarget FindRandomSpawnerTarget(string tag = null)
        {
#if UNITY_EDITOR
            GetEditorSpawnerTargets();
#endif

            // Get the number of targets
            int size = allTargets.Count;

            // Check for no target
            if (size == 0)
                return null;

            // Select a random target
            int index = Random.Range(0, size);

            int count = 0;

            // Has sets dont have index operators
            foreach(SpawnerTarget target in allTargets)
            {
                // Check for index match
                if (count == index)
                {
                    // Check for tag
                    if (tag == null || target.CompareTag(tag) == true)
                    {
                        return target;
                    }
                }

                // increment current counter
                count++;
            }

            // Error value
            return null;
        }

        /// <summary>
        /// Attempts to find a <see cref="SpawnerTarget"/> in the scene that is closest to the specifieid position. 
        /// </summary>
        /// <param name="position">The world position to find the closest target</param>
        /// <param name="tag">An optional tag that the target shoud have or null if no tag check is required</param>
        /// <returns>The <see cref="SpawnerTarget"/> that is nearest to the specified position or null if no target could be found</returns>
        public static SpawnerTarget FindNearestSpawnerTarget(Vector3 position, string tag = null)
        {
#if UNITY_EDITOR
            GetEditorSpawnerTargets();
#endif

            float nearestDistance = float.MaxValue;
            SpawnerTarget nearestTarget = null;

            // Check all distances
            foreach(SpawnerTarget target in allTargets)
            {
                // Find the square distance
                float sqrDistance = (target.transform.position - position).sqrMagnitude;

                // Check for smaller value
                if(sqrDistance < nearestDistance)
                {
                    // Check for tag
                    if (tag == null || target.CompareTag(tag) == true)
                    {
                        nearestDistance = sqrDistance;
                        nearestTarget = target;
                    }
                }
            }

            return nearestTarget;
        }

        /// <summary>
        /// Attempts to find a <see cref="SpawnerTarget"/> in the scene that is farthest to the specifieid position. 
        /// </summary>
        /// <param name="position">The world position to find the farthest target</param>
        /// <param name="tag">An optional tag that the target shoud have or null if no tag check is required</param>
        /// <returns>The <see cref="SpawnerTarget"/> that is farthest to the specified position or null if no target could be found</returns>
        public static SpawnerTarget FindFarthestSpawnerTarget(Vector3 position, string tag = null)
        {
#if UNITY_EDITOR
            GetEditorSpawnerTargets();
#endif

            float farthestDistance = float.MinValue;
            SpawnerTarget farthestTarget = null;

            // Check all distances
            foreach (SpawnerTarget target in allTargets)
            {
                // Find the square distance
                float sqrDistance = (target.transform.position - position).sqrMagnitude;

                // Check for smaller value
                if (sqrDistance > farthestDistance)
                {
                    // Check for tag
                    if (tag == null || target.CompareTag(tag) == true)
                    {
                        farthestDistance = sqrDistance;
                        farthestTarget = target;
                    }
                }
            }

            return farthestTarget;
        }

        private static void GetEditorSpawnerTargets()
        {
            if (Application.isPlaying == false)
            {
                allTargets.Clear();

                // Find all components
                SpawnerTarget[] targets = Component.FindObjectsOfType<SpawnerTarget>();

                foreach (SpawnerTarget target in targets)
                    allTargets.Add(target);
            }
        }
    }
}
