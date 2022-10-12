using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltimateSpawner.Waves;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace UltimateSpawner.Editor.Drawer
{
    [NodeEditor.CustomNodeEditor(typeof(WaveNode))]
    internal sealed class WaveNodeEditor : NodeEditor
    {
        // Private
        private static readonly string[] specialInputs =
        {
            WaveNode.inputPortName,
            WaveSpawnerReferenceNode.spawnerPortName,
            WaveSpawnableReferenceNode.spawnablePortName,
        };

        private static readonly string[] excludes = 
        {
            "m_Script",
            "graph",
            "position",
            "ports"
        };

        private List<string> usedProperties = new List<string>();

        // Methods
        public override int GetWidth()
        {
            // Get the node width
            return (target as WaveNode).NodeWidth;
        }

        public override void OnBodyGUI()
        {
            // Get the port positions
            portPositions = new Dictionary<XNode.NodePort, Vector2>();

            // Set label size
            EditorGUIUtility.labelWidth = (target as WaveNode).NodeLabelWidth;
            
            // Clear the displayed field
            usedProperties.Clear();

            // Display properties
            DisplayMainInputs();
            DisplayFields();
            DisplayOutputs();
        }

        private void DisplayMainInputs()
        {
            for (int i = 0; i < specialInputs.Length; i++)
            {
                SerializedProperty iterator = serializedObject.GetIterator();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    // CHeck for matching name
                    if (iterator.name != specialInputs[i])
                        continue;

                    Type type = iterator.serializedObject.targetObject.GetType();
                    FieldInfo info = type.GetField(iterator.propertyPath);

                    enterChildren = false;

                    if (excludes.Contains(iterator.name) == true ||
                        usedProperties.Contains(iterator.name) == true ||
                        info.IsDefined(typeof(XNode.Node.InputAttribute), true) == false)
                        continue;

                    // Draw the field
                    NodeEditorGUILayout.PropertyField(iterator, enterChildren);
                    usedProperties.Add(iterator.name);
                }
            }
        }

        private void DisplayFields()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                Type type = iterator.serializedObject.targetObject.GetType();
                FieldInfo info = type.GetField(iterator.propertyPath);

                enterChildren = false;

                if (excludes.Contains(iterator.name) == true ||
                    usedProperties.Contains(iterator.name) == true ||
                    info.IsDefined(typeof(XNode.Node.OutputAttribute), true) == true)
                    continue;

                // Draw the field
                NodeEditorGUILayout.PropertyField(iterator, enterChildren);
                usedProperties.Add(iterator.name);
            }
        }

        private void DisplayOutputs()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                Type type = iterator.serializedObject.targetObject.GetType();
                FieldInfo info = type.GetField(iterator.propertyPath);

                enterChildren = false;

                if (excludes.Contains(iterator.name) == true ||
                    usedProperties.Contains(iterator.name) == true ||
                    info.IsDefined(typeof(XNode.Node.OutputAttribute), false) == false)
                    continue;

                // Draw the field
                NodeEditorGUILayout.PropertyField(iterator, enterChildren);
                usedProperties.Add(iterator.name);
            }
        }
    }
}
