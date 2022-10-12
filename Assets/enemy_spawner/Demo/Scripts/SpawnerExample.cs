using System.Collections.Generic;
using UltimateSpawner.Spawning;
using UnityEngine;

namespace UltimateSpawner.Demo
{
    /// <summary>
    /// A demo script used in some of the Ultimate Spawenr example scenes.
    /// </summary>
    public class SpawnerExample : MonoBehaviour
    {
        // Private
        private Stack<Transform> spawnedItems = new Stack<Transform>();

        // Public
        /// <summary>
        /// The spawner used for the example.
        /// </summary>
        public Spawner spawner;
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
        public void OnGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            {
                TextAnchor oldAnchor = GUI.skin.label.alignment;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;

                // Title
                GUILayout.Label("Spawner Example");

                GUI.skin.label.alignment = oldAnchor;


                // Spawn an item
                if(DisplaySpawnerButton("Create Item", "Spawn") == true)
                {
                    if(spawner != null)
                    {
                        spawnedItems.Push(spawner.Spawn());
                    }
                }

                // Despawn an item
                if(DisplaySpawnerButton("Destroy Item", "Despawn Last") == true)
                {
                    // Check for objects destroyed by other means
                    while (spawnedItems.Count > 0 && spawnedItems.Peek() == null)
                        spawnedItems.Pop();

                    // Destroy the last item
                    if (spawnedItems.Count > 0)
                        Destroy(spawnedItems.Pop().gameObject);
                }

                GUILayout.Space(10);
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Display a label and button with a horizontal layout.
        /// </summary>
        /// <param name="label">Label text</param>
        /// <param name="buttonText">Button text</param>
        /// <returns>True if the button is clicked</returns>
        public virtual bool DisplaySpawnerButton(string label, string buttonText)
        {
            bool clicked = false;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(label + ":", GUILayout.Width(labelWidth));
                clicked = GUILayout.Button(buttonText);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(labelOffsetY);

            return clicked;
        }
    }
}
