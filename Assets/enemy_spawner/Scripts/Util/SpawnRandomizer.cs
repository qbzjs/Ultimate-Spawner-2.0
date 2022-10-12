using UnityEngine;
using UltimateSpawner.Spawning;
using Random = UnityEngine.Random;

namespace UltimateSpawner.Util
{
    public class SpawnRandomizer
    {
        // Methods
        public static Vector3 GetRandomPosition(Bounds randomBounds)
        {
            return new Vector3(
                Random.Range(randomBounds.min.x, randomBounds.max.x),
                Random.Range(randomBounds.min.y, randomBounds.max.y),
                Random.Range(randomBounds.min.z, randomBounds.max.z));
        }

        public static float GetRandomRotationAxis()
        {
            return Random.Range(0f, 360f);
        }

        public static Quaternion GetRandomRotation()
        {
            return Random.rotation;
        }

        public static Quaternion GetRandomRotation(SpawnRotationApplyMode applyRotation)
        {
            switch (applyRotation)
            {
                default:
                case SpawnRotationApplyMode.NoRotation:
                    return Quaternion.identity;

                case SpawnRotationApplyMode.FullRotation:
                    return GetRandomRotation();

                case SpawnRotationApplyMode.YRotation:
                    return Quaternion.Euler(0f, GetRandomRotationAxis(), 0f);

                case SpawnRotationApplyMode.ZRotation:
                    return Quaternion.Euler(0f, 0f, GetRandomRotationAxis());
            }
        }
    }
}
