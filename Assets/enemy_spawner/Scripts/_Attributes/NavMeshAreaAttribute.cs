using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.AI;
#endif

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class NavMeshAreaAttribute : PropertyAttribute
    {
        // Empty class
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(NavMeshAreaAttribute), true)]
    internal sealed class NavMeshAreaDrawer : PropertyDrawer
    {
        // Private
        private GUIContent[] options = null;

        // Methods
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BuildOptions();

            if(property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginProperty(position, label, property);
                {
                    int selected = property.intValue + 1;

                    int result = EditorGUI.Popup(position, label, selected, options);

                    if (result != selected)
                    {
                        property.intValue = result - 1;
                    }
                }
                EditorGUI.EndProperty();
            }
            else
            {
                // Draw default
                EditorGUI.PropertyField(position, property, label);
            }
        }

        private void BuildOptions()
        {
            if(options == null)
            {
                List<GUIContent> temp = new List<GUIContent>();

                temp.Add(new GUIContent("All Areas"));

                foreach(string name in GameObjectUtility.GetNavMeshAreaNames())
                {
                    temp.Add(new GUIContent(name));
                }

                options = temp.ToArray();
            }
        }
    }
#endif
}
