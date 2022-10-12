using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// A very simple spawner which is able to spawn an item wtih a specific position and rotation.
    /// This is the simplest end point spawn and does not offer any occupied checks.
    /// </summary>
    public class SpawnTransform : EndPointSpawner
    {
        // Public
        /// <summary>
        /// Should the rotation of the spawner be used to rotate the spawned item to the same orientation.
        /// After changing this value via code you should call <see cref="RebuildColliders"/> to update the colliders if the <see cref="occupiedCheck"/> is equal to <see cref="OccupiedCheck.PhysicsTrigger"/>.   
        /// </summary>
        [Tooltip("Should the spawner apply rotation to the spawned item or should only the position element of the transform be modified")]
        public SpawnRotationApplyMode applyRotation = SpawnRotationApplyMode.FullRotation;

        // Properties
        public override bool IsAvailable
        {
            get { return true; }
        }

        public override int SpawnableItemCapacity
        {
            get { return 1; }
        }

        public override int AvailableSpawnableItemCapacity
        {
            get { return 1; }
        }
                
        // Methods
        public override Transform Spawn(SpawnableItemRef itemRef = null)
        {
            // Create the spawnable item
            Transform result = CreateSpawnableItem(itemRef, GetLocation(), applyRotation);

            if(result == null)
            {
                Debug.LogWarning("Failed to create spawnable item");
                return null;
            }

            return result;
        }

        public override bool Spawn(Transform toSpawn)
        {
            // Simple spawn
            return GetLocation().Spawn(toSpawn, applyRotation);
        }

        public override SpawnLocation GetLocation()
        {
            return new SpawnLocation(this, transform.position, transform.rotation);
        }

#if UNITY_EDITOR
        private Texture2D tex = null;

        public void OnDrawGizmos()
        {
            if(tex == null)
            {
                tex = new Texture2D(5, 5);
                tex.Apply();
            }

            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawSphere(Vector3.zero, 0.1f);

            Handles.color = new Color(0f, 1f, 0f, 0.2f);
            Handles.matrix = transform.localToWorldMatrix;

            Handles.ConeHandleCap(1, Vector3.forward, Quaternion.identity, 0.5f, EventType.Repaint);

            Handles.matrix *= Matrix4x4.Scale(new Vector3(0.1f, 0.1f, 0.65f));
            Handles.CylinderHandleCap(2, (Vector3.forward / 2) * 1.3f, Quaternion.identity, 1f, EventType.Repaint);

        }
#endif
    }
}
