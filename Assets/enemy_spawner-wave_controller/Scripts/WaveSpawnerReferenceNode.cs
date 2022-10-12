using System;
using UltimateSpawner.Spawning;
using UnityEngine;
using XNode;

namespace UltimateSpawner.Waves
{
    [Serializable]
    [CreateNodeMenu("Waves/Spawner Reference")]
    public sealed class WaveSpawnerReferenceNode : WaveReferenceNode
    {
        // Public
        public const string spawnerPortName = "Spawner";

        [Output]
        public WaveSpawnerReferenceNode Spawner;

        // Properties
        public override string NodeDisplayName
        {
            get { return "Wave Spawner Reference"; }
        }

        // Methods
        public override bool CanConnectTo(NodePort from, NodePort to)
        {
            if (from.fieldName == spawnerPortName)
            {
                if (to.fieldName == spawnerPortName)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public Spawner ResolveSpawnerReference(WaveSpawnController controller)
        {
            // Try to find the spawner
            Spawner targetSpawner = null;

            switch (referenceMode)
            {
                case ReferenceMode.UseName:
                    {
                        // Try to find by name
                        targetSpawner = controller.spawner.FindSpawnerWithName(referenceName);

                        // Check for error
                        if (targetSpawner == null)
                            Debug.LogWarningFormat("Failed to find spawner attached to '{0}' with a name of '{1}'. Falling back to default spawner selection", controller, referenceName);

                        break;
                    }
                case ReferenceMode.UseTag:
                    {
                        // Try to find by tag
                        targetSpawner = controller.spawner.FindSpawnerWithTag(referenceTag);

                        // Check for error
                        if (targetSpawner == null)
                            Debug.LogWarningFormat("Failed to find spawner attached to '{0}' with tag '{1}'. Falling back to default spawner selection", controller, referenceTag);

                        break;
                    }
                case ReferenceMode.UseID:
                    {
                        // Try to find by id
                        targetSpawner = controller.spawner.FindSpawnerWithID(referenceID);

                        // Check for error
                        if (targetSpawner == null)
                            Debug.LogWarningFormat("Failed to find spawner attached to '{0}' with an index id of '{1}'. Falling back to default spawner selection", controller, referenceID);

                        break;
                    }
            }

            return targetSpawner;
        }
    }
}
