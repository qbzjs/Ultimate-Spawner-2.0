
using UltimateSpawner.Spawning;
using UnityEngine;

namespace UltimateSpawner.Despawning
{
    public enum DespawnDistanceMode
    {
        NearestTargetGreaterThan,
        NearestTargetLessThan,
        NearestTargetWithTagGreaterThan,
        NearestTargetWithTagLessThan,
        FurthestTargetGreaterThan,
        FurthestTargetLessThan,
        FurthestTargetWithTagGreaterThan,
        FurthestTargetWithTagLessThan,
    }

    public class DespawnDistance : Despawner
    {
        // Public
        public DespawnDistanceMode despawnMode = DespawnDistanceMode.NearestTargetGreaterThan;
        public float distance = 5f;

#if UNITY_EDITOR
        /// <summary>
        /// The target used to filter <see cref="SpawnerTarget"/> objects in the scene. 
        /// </summary>
        [DisplayConditionMethod("IsTaggedCondition", DisplayType.Hidden, typeof(TagCollectionDrawer))]
#endif
        public string despawnTargetTag = null;

        // Methods
        public void Update()
        {
            if (ShouldAllowDespawn == false)
                return;

            // Get the target tag
            string tag = IsTaggedCondition() ? despawnTargetTag : null;

            // Find the spawner target
            SpawnerTarget target = null;

            // Find the desired target
            if (IsNearestTargetCondtiion() == true)
            {
                // Try to find nearest target
                target = SpawnerTarget.FindNearestSpawnerTarget(transform.position, tag);
            }
            else
            {
                // Try to find furthest target
                target = SpawnerTarget.FindFarthestSpawnerTarget(transform.position, tag);
            }

            // Check for no targets in the scene
            if (target == null)
                return;

            // FInd distance to target
            float actualDistance = Vector3.Distance(transform.position, target.transform.position);

            // Check for condition met
            if (IsNearestDistanceCondition() == true)
            {
                if (actualDistance < distance)
                {
                    // Despawn the item
                    MarkDespawnConditionAsMet();
                    Despawn();
                }
            }
            else
            {
                if (actualDistance > distance)
                {
                    // Despawn the item
                    MarkDespawnConditionAsMet();
                    Despawn();
                }
            }
        }

        public override void CloneFrom(Despawner cloneFrom)
        {
            DespawnDistance despawner = cloneFrom as DespawnDistance;

            if (despawner != null)
            {
                despawnMode = despawner.despawnMode;
                distance = despawner.distance;
                despawnTargetTag = despawner.despawnTargetTag;
            }
        }

        private bool IsTaggedCondition()
        {
            return (despawnMode == DespawnDistanceMode.FurthestTargetWithTagGreaterThan ||
                despawnMode == DespawnDistanceMode.FurthestTargetWithTagLessThan ||
                despawnMode == DespawnDistanceMode.NearestTargetWithTagGreaterThan ||
                despawnMode == DespawnDistanceMode.NearestTargetWithTagLessThan);
        }

        private bool IsNearestTargetCondtiion()
        {
            return (despawnMode == DespawnDistanceMode.NearestTargetGreaterThan ||
                despawnMode == DespawnDistanceMode.NearestTargetLessThan ||
                despawnMode == DespawnDistanceMode.NearestTargetWithTagGreaterThan ||
                despawnMode == DespawnDistanceMode.NearestTargetWithTagLessThan);
        }

        private bool IsNearestDistanceCondition()
        {
            return (despawnMode == DespawnDistanceMode.FurthestTargetLessThan ||
                despawnMode == DespawnDistanceMode.FurthestTargetWithTagLessThan ||
                despawnMode == DespawnDistanceMode.NearestTargetLessThan ||
                despawnMode == DespawnDistanceMode.NearestTargetWithTagLessThan);
        }
    }
}
