using Trivial.ImGUI;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace UltimateSpawner.Editor
{
    [CustomEditor(typeof(SpawnController), true)]
    internal sealed class SpawnControllerInspector : ImInspectorWindow<SpawnController>
    {
        // Methods
        public override void OnImGUI()
        {
            // Mono script field
            ImGUI.PushEnabledVisualState(false);
            ImGUILayout.PropertyField(MonoScriptProperty());
            ImGUI.PopVisualState();

            // Display visible fields
            foreach (SerializedProperty property in VisibleProperties())
            {
                // Draw the property
                ImGUILayout.PropertyField(property);
            }

            // Display events
            GUIEvents();

            GUIControllerInformation();
        }

        private void GUIEvents()
        {
            // Show the foldout
            InspectedValue.editorEventsExpanded = ImGUILayout.Foldout("Controller Events", InspectedValue.editorEventsExpanded);

            // Check for expanded
            if (InspectedValue.editorEventsExpanded == true)
            {
                ImGUI.Indent++;

                // Find all unity events
                foreach (SerializedProperty property in AllProperties<UnityEventBase>())
                {
                    // Draw the property
                    ImGUILayout.PropertyField(property);
                }

                ImGUI.Indent--;
            }
        }

        private void GUIControllerInformation()
        {
            // Check for spawn group
            if (InspectedValue is TriggerSpawnController)
            {
                bool validTrigger = false;

                foreach (Collider col in InspectedValue.GetComponents<Collider>())
                {
                    if (col.isTrigger == true)
                    {
                        validTrigger = true;
                        break;
                    }
                }

                foreach (Collider2D col in InspectedValue.GetComponents<Collider2D>())
                {
                    if (col.isTrigger == true)
                    {
                        validTrigger = true;
                        break;
                    }
                }

                if (validTrigger == false)
                {
                    ImGUILayout.HelpBox("This trigger controller does not have a valid trigger collider attached. You must attach a 2D or 3D trigger collider in order for it to function correctly", MessageType.Warning);
                }
            }
        }
    }
}
