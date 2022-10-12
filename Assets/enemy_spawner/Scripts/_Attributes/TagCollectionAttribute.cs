using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine
{
    /// <summary>
    /// An attribute which displays all available tags in a popup when fields of string or string array types are decorated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class TagCollectionAttribute : PropertyAttribute
    {
        // Empty class
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TagCollectionAttribute), true)]
    internal sealed class TagCollectionDrawer : PropertyDrawer
    {        
        // Methods
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {    
            // Check for string property
            if(property.propertyType == SerializedPropertyType.String)
            {
                // Check for null string
                if (property.stringValue == string.Empty)
                    property.stringValue = "Untagged";

                // Start the property
                EditorGUI.BeginProperty(position, label, property);
                {
                    // Draw the tag field
                    property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
                }                
                EditorGUI.EndProperty();
            }
            else
            {
                // Draw default
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
#endif
}
