using UltimateSpawner.Spawning;
using UnityEngine;

namespace UltimateSpawner.Despawning
{
    public enum DespawnTarget
    {
        ThisObject,
        OtherObject,
    }

    public abstract class Despawner : MonoBehaviour, ISpawnEventReceiver
    {
        // Internal
#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        internal bool despawnerExpanded = false;
#endif

        // Private
        private bool isDespawned = false;
        private bool isDespawnConditionMet = false;

        [SerializeField, HideInInspector]
        private bool applyDespawnerToSpawnedItems = true;

        // Protected
        protected SpawnableIdentity spawnItemIdentity = null;

        // Properties
        public virtual bool IsDespawnConditionMet
        {
            get { return isDespawnConditionMet; }
        }

        public bool ApplyDespawnerToSpawnedItems
        {
            get { return applyDespawnerToSpawnedItems; }
            set { applyDespawnerToSpawnedItems = value; }
        }

        public bool ShouldAllowDespawn
        {
            get { return GetComponent<Spawner>() == null; }
        }

        // Methods
        public abstract void CloneFrom(Despawner cloneFrom);

        public virtual void OnSpawned(SpawnLocation location)
        {
            spawnItemIdentity = GetComponent<SpawnableIdentity>();
        }

        protected void MarkDespawnConditionAsMet()
        {
            isDespawnConditionMet = true;
        }

        protected virtual void Despawn()
        {
            if (isDespawned == false)
            {
                if (spawnItemIdentity != null)
                {
                    // Call the despawn method
                    UltimateSpawning.Despawn(spawnItemIdentity);
                }
                else
                {
                    // Call the despawn method
                    UltimateSpawning.Despawn(gameObject);
                }
                isDespawned = true;
            }
        }
    }
}
