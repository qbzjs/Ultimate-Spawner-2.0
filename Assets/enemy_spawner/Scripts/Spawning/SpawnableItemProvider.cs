using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UltimateSpawner.Spawning
{
    [Serializable]
    public abstract class SpawnableItemProvider : ScriptableObject
    {
        // Properties
        public abstract bool IsAssigned
        {
            get;
        }

        public abstract string ItemName
        {
            get;
        }

        public virtual string ItemTag
        {
            get { return null; }
        }

        // Methods
        public abstract Object CreateSpawnableInstance(SpawnLocation spawnLocation, SpawnRotationApplyMode applyRotation);

        public abstract void DestroySpawnableInstance(Object spawnableInstance);
    }
}
