using UnityEngine;

namespace UltimateSpawner.DebugUtil
{
    /// <summary>
    /// Displays controller stats using the legacy immediate mode GUI.
    /// </summary>
    public class SpawnControllerGUIStatsLegacy : MonoBehaviour
    {
        // Public
        /// <summary>
        /// The spawn controller to display stats for. 
        /// If no controller is assigned then this stats component will attempt to find a controller first from the parent game object, and then from the active scene.
        /// </summary>
        public SpawnController observedController;
        /// <summary>
        /// The width of the displayed GUI labels.
        /// </summary>
        public int labelWidth = 180;
        /// <summary>
        /// The Y offset of the displayed GUI labels.
        /// </summary>
        public int labelOffsetY = -6;

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void Awake()
        {
            // Auto find the spawn controller if one is not assigned
            if (observedController == null)
                observedController = GetComponent<SpawnController>();

            if (observedController == null)
                observedController = Component.FindObjectOfType<SpawnController>();
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public virtual void OnGUI()
        {
            // Check for no controller
            if (observedController == null)
            {
                Debug.LogWarning(name + ": No observed controller could be found");
                enabled = false;
                return;
            }

            GUILayout.BeginVertical(GUI.skin.box);
            {
                DisplayControllerStats("Spawn Controller Stats");
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Display all controller stats for the observed controller.
        /// </summary>
        /// <param name="windowTitle"></param>
        public virtual void DisplayControllerStats(string windowTitle)
        {
            TextAnchor oldAnchor = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            // Title
            GUILayout.Label(windowTitle);

            GUI.skin.label.alignment = oldAnchor;

            // Separator
            GUILayout.Label(GUIContent.none, GUILayout.Height(1));

            // Stats
            // Controller.IsSpawning
            DisplayControllerStat("Is Spawning", observedController.IsSpawning);

            // Controller.IsTryingToSpawn
            DisplayControllerStat("Is Trying To Spawn", observedController.IsTryingToSpawn);

            // Controller.IsPaused
            DisplayControllerStat("Is Paused", observedController.IsPaused);

            // Controller.SpawnedItemCount
            DisplayControllerStat("Spawned Item Count", observedController.SpawnedItemCount);

            // Controller.TotalSpawnedItemCount
            DisplayControllerStat("Total Spawned Item Count", observedController.TotalSpawnedItemCount);

            // Controller.TotalDestroyedItemCount
            DisplayControllerStat("Total Destroyed Item Count", observedController.TotalDestroyedItemCount);            
        }

        /// <summary>
        /// Display a single stat in a horizontal layout group.
        /// </summary>
        /// <param name="statName">The label for the stat</param>
        /// <param name="statValue">The value for the stat</param>
        public virtual void DisplayControllerStat(string statName, object statValue)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(statName + ":", GUILayout.Width(labelWidth));
                GUILayout.Label(statValue.ToString());
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(labelOffsetY);
        }
    }
}
