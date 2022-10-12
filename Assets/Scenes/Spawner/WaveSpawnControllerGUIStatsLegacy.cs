using UnityEngine;

namespace UltimateSpawner.DebugUtil
{
    public class WaveSpawnControllerGUIStatsLegacy : SpawnControllerGUIStatsLegacy
    {
        // Methods
        public void Reset()
        {
            labelWidth = 240;
        }

        public override void Awake()
        {
            // Auto find the spawn controller if one is not assigned
            if (observedController == null)
                observedController = GetComponent<WaveSpawnController>();

            if (observedController == null)
                observedController = Component.FindObjectOfType<WaveSpawnController>();
        }

        public override void OnGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                DisplayControllerStats("Wave Spawn Controller Stats");
                DisplayWaveControllerStats();
            }
            GUILayout.EndVertical();
        }

        public void DisplayWaveControllerStats()
        {
            // Get wave controller
            WaveSpawnController controller = observedController as WaveSpawnController;

            // Check for no controller
            if (controller == null)
                return;

            // Controller.CurrentWave
            DisplayControllerStat("Current Wave", controller.CurrentWave);

            DisplayControllerStat("Current Wave Sub-Waves Count", (controller.CurrentState != null) ? controller.CurrentState.SubWaves.Length : 0);

            DisplayControllerStat("Current Wave Node", controller.CurrentNodeType);

            DisplayControllerStat("Current Wave Spawn Count", (controller.CurrentState != null) ? controller.CurrentState.WaveSpawnCount : 0);

            DisplayControllerStat("Current Wave Spawn Frequency", (controller.CurrentState != null) ? controller.CurrentState.WaveSpawnFrequency : 0);

            DisplayControllerStat("Current Wave Spawn Randomness", (controller.CurrentState != null) ? controller.CurrentState.WaveSpawnRandomness : 0);

            // Controller.CurrentWaveSpawnedItemCount
            DisplayControllerStat("Current Wave Spawned Item Count", controller.CurrentWaveSpawnedItemCount);

            // Controller.CurrentWaveDestroyedItemCount
            DisplayControllerStat("Current Wave Destroyed Item Count", controller.CurrentWaveDestroyedItemCount);

            DisplayControllerStat("Current Wave Target Spawner", (controller.CurrentState != null) ? ((controller.CurrentState.TargetSpawner != null) ? controller.CurrentState.TargetSpawner.name : "Any") : "None");

            DisplayControllerStat("Current Wave Target Spawnable", (controller.CurrentState != null) ? ((controller.CurrentState.TargetSpawnable != null) ? controller.CurrentState.TargetSpawnable.Name : "Any") : "None");

        }
    }
}
