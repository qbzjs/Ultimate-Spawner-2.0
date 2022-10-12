using System;
using UnityEngine;
using UnityEditor;
using UltimateSpawner.Waves.Parameters;
using Object = UnityEngine.Object;
using System.Reflection;
using System.Collections.Generic;

namespace UltimateSpawner.Waves.Editor.Drawer
{
    [CustomPropertyDrawer(typeof(WaveParameterSet))]
    public class WaveParameterNodeDrawer : PropertyDrawer
    {
        // Private
        private WaveParameterSet parameterSet = null;
        private Queue<WaveParameter> removeList = new Queue<WaveParameter>();

        // Methods
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GetObject(property);

            return (parameterSet.ParameterCount * EditorGUIUtility.singleLineHeight) + EditorGUIUtility.singleLineHeight * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GetObject(property);

            // Get the controls area
            Rect controlsArea = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * (parameterSet.ParameterCount + 1), position.width, EditorGUIUtility.singleLineHeight);

            if (parameterSet.ParameterCount == 0)
            {
                Rect labelArea = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);                

                // Main label
                EditorGUI.DrawRect(labelArea, Color.grey);
                EditorGUI.LabelField(labelArea, "No Parameters", EditorStyles.label);
            }
            else
            {
                float lineHeight = position.y;

                foreach(WaveParameter parameter in parameterSet.Parameters)
                {
                    // Calcualte the rect
                    Rect area = new Rect(position.x, lineHeight, position.width, EditorGUIUtility.singleLineHeight);

                    // Display the parameter
                    if(DisplayParameter(area, parameter) == true)
                    {
                        // Mark parameter for removal
                        removeList.Enqueue(parameter);
                    }

                    lineHeight += EditorGUIUtility.singleLineHeight;
                }

                // Delete any removed parameters
                while (removeList.Count > 0)
                {
                    parameterSet.RemoveParameter(removeList.Dequeue());
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }

            // Add button rect
            Rect addButton = new Rect(position.xMax - 101, controlsArea.y, 100, controlsArea.height);

            // Display popup button
            int result = EditorGUI.Popup(addButton, -1, new string[] { "Int", "Float", "Bool" }, EditorStyles.toolbarDropDown);

            addButton.x += 5;
            EditorGUI.LabelField(addButton, "Add Parameter", EditorStyles.miniLabel);

            if(result != -1)
            {
                // Add a new parameter
                parameterSet.AddParameter(GetUniqueParameterName(), (WaveParameter.WaveParameterType)result);
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }

        private bool DisplayParameter(Rect area, WaveParameter parameter)
        {
            // Main label
            EditorGUI.DrawRect(area, Color.grey);

            Rect label = new Rect(area.x, area.y, EditorGUIUtility.labelWidth, area.height);
            Rect type = new Rect(label.xMax + 4, label.y, 100, label.height);
            Rect value = new Rect(type.xMax + 4, type.y, 100, type.height);
            Rect close = new Rect(value.xMax + 4, value.y, 20, value.height);


            // Controls
            parameter.parameterName = EditorGUI.TextField (label, parameter.parameterName, EditorStyles.textField);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.Popup(type, 0, new string[] { parameter.parameterType.ToString() });
            EditorGUI.EndDisabledGroup();

            // Display parameter value
            switch(parameter.parameterType)
            {
                default:
                case WaveParameter.WaveParameterType.Int:
                    {
                        parameter.intValue = EditorGUI.IntField(value, parameter.intValue);
                        break;
                    }

                case WaveParameter.WaveParameterType.Float:
                    {
                        parameter.floatValue = EditorGUI.FloatField(value, parameter.floatValue);
                        break;
                    }

                case WaveParameter.WaveParameterType.Bool:
                    {
                        parameter.boolValue = EditorGUI.Toggle(value, parameter.boolValue);
                        break;
                    }
            }            

            bool remove = false;

            if(GUI.Button(close, "X") == true)
            {
                remove = true;
            }

            return remove;
        }

        private void GetObject(SerializedProperty property)
        {
            if (parameterSet == null)
            {
                // Get the target object
                parameterSet = (WaveParameterSet)fieldInfo.GetValue(property.serializedObject.targetObject);
            }
        }

        private string GetUniqueParameterName()
        {
            int count = 1;

            while (parameterSet.HasParameterWithName("Parameter " + count) == true)
                count++;

            return "Parameter " + count;
        }
    }
}
