using Trivial.ImGUI;
using UltimateSpawner.Waves;
using UnityEngine;
using UnityEditor;

namespace UltimateSpawner.Editor
{
    [CustomPropertyDrawer(typeof(WaveConfiguration))]
    internal sealed class WaveConfigurationDrawer : ImPropertyDrawer<WaveConfiguration>
    {
        // Private
        private int buttonWidth = 50;

        // Methods
        public override void OnImGUI(Rect area, GUIContent content)
        {
            // Get the property value
            WaveConfiguration config = GetPropertyValue();

            // Create label rect
            Rect labelRect = area;
            labelRect.width = InspectorLabelWidth;

            // Create field rect
            Rect fieldRect = new Rect(area);
            fieldRect.x += InspectorLabelWidth;
            fieldRect.width -= InspectorLabelWidth + buttonWidth;

            // Create button rect
            Rect buttonRect = new Rect(area);
            buttonRect.x += InspectorLabelWidth + fieldRect.width;
            buttonRect.width = buttonWidth;

            // Display the label
            ImGUI.SetNextPosition(labelRect.position);
            ImGUI.SetNextSize(labelRect.size);
            ImGUI.SetNextTooltip(content.tooltip);
            ImGUI.Label(content.text);

            // Display the field
            ImGUI.SetNextPosition(fieldRect.position);
            ImGUI.SetNextSize(fieldRect.size);
            WaveConfiguration resultConfig = ImGUI.ObjectField<WaveConfiguration>(config);

            // Check for change
            if (resultConfig != config)
            {
                config = resultConfig;
                SetSerializedValue(resultConfig);
            }

            // Display button
            ImGUI.SetNextPosition(buttonRect.position);
            ImGUI.SetNextSize(buttonRect.size);

            if(config == null)
            {
                ImGUI.SetNextWidth(buttonWidth);
                ImGUI.SetNextTooltip("Create a new wave configuration and open the wave editor window");
                if(ImGUI.Button("New") == true)
                {
                    // Show the dialog
                    string result = EditorUtility.SaveFilePanel("Create Wave COnfiguration Asset", "Assets", "Wave Configuration 1", "asset");

                    // Check for valid filename
                    if(string.IsNullOrEmpty(result) == false)
                    {
                        // Get relative path
                        string relative = FileUtil.GetProjectRelativePath(result);

                        if (string.IsNullOrEmpty(relative) == false)
                        {
                            // Create the asset instance
                            WaveConfiguration newConfig = ScriptableObject.CreateInstance<WaveConfiguration>();
                            
                            // Create the asset
                            AssetDatabase.CreateAsset(newConfig, relative);
                            EditorUtility.SetDirty(newConfig);
                            AssetDatabase.SaveAssets();

                            // Set the assigned value
                            SetSerializedValue(newConfig);

                            // Add start nodes
                            if (newConfig.nodes.Count == 0)
                            {
                                newConfig.AddNode<WaveStartNode>();
                                EditorUtility.SetDirty(newConfig);
                            }

                            // Open the editor window
                            XNodeEditor.NodeEditorWindow.OnOpen(newConfig.GetInstanceID(), 0);
                        }
                    }
                }
            }
            else
            {
                ImGUI.SetNextWidth(buttonWidth);
                ImGUI.SetNextTooltip("Edit the assigned wave configuration in the wave editor window");
                if(ImGUI.Button("Edit") == true)
                {
                    // Add start nodes
                    if (config.nodes.Count == 0)
                    {
                        config.AddNode<WaveStartNode>();
                    }

                    // Open the editor window
                    XNodeEditor.NodeEditorWindow.OnOpen(config.GetInstanceID(), 0);
                }
            }
        }
    }
}
