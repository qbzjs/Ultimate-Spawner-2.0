using System;
using UltimateSpawner.Spawning;
using UnityEngine;
using XNode;

namespace UltimateSpawner.Waves
{
    [Serializable]
    [CreateNodeMenu("Waves/Spawnable Reference")]
    public sealed class WaveSpawnableReferenceNode : WaveReferenceNode
    {
        // Public
        public const string spawnablePortName = "Spawnable";

        [Output]
        public WaveSpawnableReferenceNode Spawnable;

        // Properties
        public override string NodeDisplayName
        {
            get { return "Wave Spawnable Item"; }
        }

        // Methods
        public void Reset()
        {
            referenceTag = "Spawnable";
            referenceName = "Prefab 1";
        }

        public override bool CanConnectTo(NodePort from, NodePort to)
        {
            if(from.fieldName == spawnablePortName)
            {
                if(to.fieldName == spawnablePortName)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public SpawnableItemRef ResolveSpawnableReference(WaveSpawnController controller)
        {
            SpawnableItemRef targetItem = null;

            switch (referenceMode)
            {
                case ReferenceMode.UseName:
                    {
                        // Try to find with name
                        targetItem = SpawnableItemRef.WithName(referenceName);

                        // Check for error
                        if (targetItem.IsValid == false || controller.HasSpawnableItem(targetItem) == false)
                            Debug.LogWarningFormat("Failed to find spawnable item with a name of '{1}'. Falling back to default item selection", controller, referenceName);

                        break;
                    }

                case ReferenceMode.UseTag:
                    {
                        // Try to find with tag
                        targetItem = SpawnableItemRef.WithTag(referenceTag);

                        // Check for error
                        if (targetItem.IsValid == false || controller.HasSpawnableItem(targetItem) == false)
                            Debug.LogWarningFormat("Failed to find spawnable item with tag '{1}'. Falling back to default item selection", controller, referenceTag);

                        break;
                    }

                case ReferenceMode.UseID:
                    {
                        // Try to find with id
                        targetItem = SpawnableItemRef.WithID(referenceID);

                        // Check for error
                        if (targetItem.IsValid == false || controller.HasSpawnableItem(targetItem) == false)
                            Debug.LogWarningFormat("Failed to find spawnable item with an index id of '{1}'. Falling back to default item selection", controller, referenceID);

                        break;
                    }
            }

            return targetItem;
        }
    }
}
