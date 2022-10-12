using UnityEngine;
using UltimateSpawner.Spawning;

namespace UltimateSpawner.Util
{
    public static class SpawnerUtility
    {
        // Methods
        public static Quaternion GetRotationFromTransform(Transform transform, SpawnRotationApplyMode applyRotation)
        {
            switch(applyRotation)
            {
                default:
                case SpawnRotationApplyMode.NoRotation:
                    return Quaternion.identity;

                case SpawnRotationApplyMode.FullRotation:
                    return transform.rotation;

                case SpawnRotationApplyMode.YRotation:
                    return Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

                case SpawnRotationApplyMode.ZRotation:
                    return Quaternion.Euler(0f, 0f, transform.eulerAngles.z);
            }
        }

        public static Quaternion GetRotationForSpawnable(Transform spawnerTransform, SpawnRotationApplyMode spawnerRotation, SpawnRotationApplyMode randomRotation)
        {
            // Check for no rotation
            if (spawnerRotation == SpawnRotationApplyMode.NoRotation && randomRotation == SpawnRotationApplyMode.NoRotation)
                return Quaternion.identity;

            // Use full spawner rotation
            if (spawnerRotation == SpawnRotationApplyMode.FullRotation)
                return spawnerTransform.rotation;

            // Use full random rotation
            if (randomRotation == SpawnRotationApplyMode.FullRotation)
                return SpawnRandomizer.GetRandomRotation();

            Quaternion result = Quaternion.identity;

            // Apply spawner rotation
            if (spawnerRotation == SpawnRotationApplyMode.YRotation) result *= Quaternion.Euler(0f, spawnerTransform.eulerAngles.y, 0f);
            if (spawnerRotation == SpawnRotationApplyMode.ZRotation) result *= Quaternion.Euler(0f, 0f, spawnerTransform.eulerAngles.z);

            // Apply random rotation
            if (randomRotation == SpawnRotationApplyMode.YRotation) result *= Quaternion.Euler(0f, SpawnRandomizer.GetRandomRotationAxis(), 0f);
            if (randomRotation == SpawnRotationApplyMode.ZRotation) result *= Quaternion.Euler(0f, 0f, SpawnRandomizer.GetRandomRotationAxis());

            return result;
        }
    }
}
