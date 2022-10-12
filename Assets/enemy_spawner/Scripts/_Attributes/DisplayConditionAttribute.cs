using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngineInternal
{
    using UnityEngine;

    /// <summary>
    /// This base attribute class is inehrited by all display attributes.
    /// This base class is not intended for external use but must be public due to the inheritance contract so is moved under the 'internal' namespace to 'hide' the type from the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class DisplayConditionBaseAttribute : PropertyAttribute
    {
        // Private
#if UNITY_EDITOR
        private Type alternateDrawerType = null;
        private PropertyDrawer alternateDrawerInstance = null;
#endif

        // Public
        /// <summary>
        /// The name of the field or method that is the target conditional member.
        /// </summary>
        public string conditionalMember = string.Empty;
        /// <summary>
        /// The <see cref="DisplayType"/> used to determine how the property will be drawn if the condition is not met. 
        /// </summary>
        public DisplayType displayType = DisplayType.Hidden;

        // Properties
#if UNITY_EDITOR
        /// <summary>
        /// Get the shared instance of the alternate property drawer. 
        /// This drawer will be used to display the property if the display condition is met.
        /// </summary>
        public PropertyDrawer AlternateDrawerInstance
        {
            get
            {
#if UNITY_EDITOR
                // Check for alternate drawer
                if (alternateDrawerType == null)
                    return null;
#else
                return null;
#endif

                // Create shared instance
                if (alternateDrawerInstance == null)
                    alternateDrawerInstance = (PropertyDrawer)Activator.CreateInstance(alternateDrawerType);

                return alternateDrawerInstance;
            }
        }
#endif

                // Constructor
                /// <summary>
                /// Initialize this base class.
                /// </summary>
                /// <param name="conditionalMember">The name of the conditional member that will be evaulated as part of the conditional check</param>
                /// <param name="displayType">The <see cref="DisplayType"/> used to determine how the property will be drawn if the condition is not met</param>
                /// <param name="alternateDrawerType">The type of the alternative property drawer to use or null if the default drawer should be used</param>
                protected DisplayConditionBaseAttribute(string conditionalMember, DisplayType displayType, Type alternateDrawerType = null)           
        {
            this.conditionalMember = conditionalMember;
            this.displayType = displayType;

#if UNITY_EDITOR
            this.alternateDrawerType = alternateDrawerType;
#endif
        }
    }
}

namespace UnityEngine
{
    using UnityEngineInternal;

    /// <summary>
    /// The type of condition used to determine whether a property should be drawn.
    /// </summary>
    public enum ConditionType
    {
        /// <summary>
        /// Check for equality.
        /// </summary>
        Equal = 1,
        /// <summary>
        /// Check for non-equality.
        /// </summary>
        NotEqual = 2,
        /// <summary>
        /// Check for greater than.
        /// </summary>
        GreaterThan = 3,
        /// <summary>
        /// Check for less than.
        /// </summary>
        LessThan = 4,
        /// <summary>
        /// Check for greater than or equal to.
        /// </summary>
        GreaterThanOrEqual = 5,
        /// <summary>
        /// Check for less than or equal to.
        /// </summary>
        LessThanOrEqual = 6,
    }

    /// <summary>
    /// The display type used to determine how a property will be drawn when the condition is not met.
    /// </summary>
    public enum DisplayType
    {
        /// <summary>
        /// The property will be drawn as a readonly disabled field.
        /// </summary>
        Disabled,
        /// <summary>
        /// The property will no be drawn at all.
        /// </summary>
        Hidden,
    }        
    
    /// <summary>
    /// Attribute used on serialized fields to create a display condition where the specified method will be called to determine whether or not to display the property.
    /// </summary>
    public class DisplayConditionMethodAttribute : DisplayConditionBaseAttribute
    {
        // Constructor
        /// <summary>
        /// Create a new instance of this attribute.
        /// </summary>
        /// <param name="conditionalMethod">The name of the methood to call when the property draw condition should be evaluated</param>
        /// <param name="displayType">The <see cref="DisplayType"/> used to indicate how the property will be displayed when the condition is not met</param>
        /// <param name="alternateDrawerType">The type of the alternate drawer to use or null if the default drawer should be used</param>
        public DisplayConditionMethodAttribute(string conditionalMethod, DisplayType displayType = DisplayType.Hidden, Type alternateDrawerType = null)
            : base(conditionalMethod, displayType, alternateDrawerType)
        {
        }

        // Methods
        internal Func<bool> CreateMethodDelegate(Type instanceType, object instance)
        {
            // Try to find the method
            MethodInfo method = instanceType.GetMethod(conditionalMember, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // Check for error
            if (method == null)
            {
                Debug.LogWarningFormat("Conditional method '{0}' could not be found. Is the method name correct?", conditionalMember);
                return null;
            }

            // Check for return type
            if (method.ReturnType != typeof(bool))
            {
                Debug.LogWarningFormat("Conditional method '{0}' must have a return type of '{1}'", conditionalMember, typeof(bool));
                return null;
            }

            // Check for no arguments
            if (method.GetParameters().Length > 0)
            {
                Debug.LogWarningFormat("Conditional method '{0}' must not accept any arguments", conditionalMember);
                return null;
            }

            // Dont pass an instance if the method is static
            instance = (method.IsStatic == true) ? null : instance;

            // Get the target delegate
            return (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), instance, method, true);
        }
    }

    /// <summary>
    /// Attribute used on serialized fields to create a display condition where the specified field will be evalated agains the specified value to determine in the property should be drawn or not.
    /// </summary>
    public class DisplayConditionAttribute : DisplayConditionBaseAttribute
    {
        // Public
        /// <summary>
        /// The value used to compare the target conditional field against.
        /// </summary>
        public object conditionalValue = null;
        /// <summary>
        /// The <see cref="ConditionType"/> used to determine what comparison operation will be used in the condition check. 
        /// </summary>
        public ConditionType conditionType = ConditionType.Equal;

        // Constructor
        /// <summary>
        /// Create a new instance of this attribute.
        /// </summary>
        /// <param name="conditionalField">The name of the field that should be checked against</param>
        /// <param name="conditionalValue">The value used in the condition check with the specified field</param>
        /// <param name="conditiontype">The <see cref="ConditionType"/> used to determine the type of comparison that will be used to evaulate the condition</param>
        /// <param name="displayType">The <see cref="DisplayType"/> used to indicate how the property will be displayed when the condition is not met</param>
        /// <param name="alternateDrawerType">The type of the custom property drawer to use or null if the default drawer should be used</param>
        public DisplayConditionAttribute(string conditionalField, object conditionalValue, ConditionType conditiontype = ConditionType.Equal, DisplayType displayType = DisplayType.Hidden, Type alternateDrawerType = null)
            : base(conditionalField, displayType, alternateDrawerType)
        {
            this.conditionalValue = conditionalValue;
            this.conditionType = conditiontype;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisplayConditionBaseAttribute), true)]
    internal sealed class DisplayConditionDrawer : PropertyDrawer
    {   
        // Private
        private SerializedProperty conditionalProperty = null;
        private Func<bool> cachedMethod = null;
        private bool displayField = true;

        // Methods
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Get the attribute
            DisplayConditionBaseAttribute conditionAttribute = attribute as DisplayConditionBaseAttribute;

            if (conditionAttribute is DisplayConditionMethodAttribute)
            {
                // Get the correct attribute type
                DisplayConditionMethodAttribute conditionMethod = conditionAttribute as DisplayConditionMethodAttribute;

                if (cachedMethod == null)
                {
                    // Create the delegate that we will use to call the method
                    cachedMethod = conditionMethod.CreateMethodDelegate(
                        property.serializedObject.targetObject.GetType(),
                        property.serializedObject.targetObject);
                }

                // Call the method
                try
                {
                    // Check for valid method
                    if (cachedMethod != null)
                    {
                        // Invoke the user method
                        displayField = cachedMethod.Invoke();
                    }
                }
                catch (Exception e)
                {
                    displayField = true;
                    Debug.LogException(e);
                }
            }
            else if (conditionAttribute is DisplayConditionAttribute)
            {
                // Get the correct attribute type
                DisplayConditionAttribute condition = conditionAttribute as DisplayConditionAttribute;

                // Default to visible whenever an error occurs
                displayField = true;

                // Find the referenced property
                if (condition.conditionalMember != null)
                {
                    // Find the referenced property
                    if (conditionalProperty == null)
                    {
                        // Try to cache the property
                        conditionalProperty = property.serializedObject.FindProperty(condition.conditionalMember);
                    }

                    // Check for error
                    if (conditionalProperty == null)
                    {
                        Debug.LogWarningFormat("Conditional field '{0}' could not be found. Are you sure the name is correct", condition.conditionalMember);
                    }
                    else
                    {
                        // Get the property value
                        object value = GetPropertyValue(conditionalProperty);

                        try
                        {
                            // Check for the target condition
                            displayField = IsConditionMet(condition.conditionType, value, condition.conditionalValue);
                        }
                        catch(Exception e)
                        {
                            Debug.LogException(e);

                            Debug.LogWarningFormat("Conditional field '{0}' could not be evaluated. Make sure the specified condition operation '{1}' can be applied to both the specified value '{2}' and the field value '{3}'", condition.conditionalMember, condition.conditionType, condition.conditionalValue, value);
                            displayField = true;
                        }
                    }
                }
            }

            // Check for hidden field
            if (displayField == true || conditionAttribute.displayType == DisplayType.Disabled)
            {
                if (conditionAttribute.AlternateDrawerInstance != null)
                {
                    return conditionAttribute.AlternateDrawerInstance.GetPropertyHeight(property, label);
                }
                else
                {
                    return base.GetPropertyHeight(property, label);
                }
            }

            // Get the default height
            return 0;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the attribute
            DisplayConditionBaseAttribute conditionAttribute = attribute as DisplayConditionBaseAttribute;    

            // Render the field
            if (displayField == true)
            {
                if (conditionAttribute.AlternateDrawerInstance != null)
                {
                    conditionAttribute.AlternateDrawerInstance.OnGUI(position, property, label);
                }
                else
                {
                    // Draw the default field
                    EditorGUI.PropertyField(position, property, label);
                }
            }
            else
            {
                // Draw the field as disabled
                if (conditionAttribute.displayType == DisplayType.Disabled)
                {
                    // Remember the gui state so we cna reset it properly
                    bool oldState = GUI.enabled;

                    // Draw disabled property
                    GUI.enabled = false;
                    if (conditionAttribute.AlternateDrawerInstance != null)
                    {
                        conditionAttribute.AlternateDrawerInstance.OnGUI(position, property, label);
                    }
                    else
                    {
                        EditorGUI.PropertyField(position, property, label);
                    }
                    GUI.enabled = oldState;
                }
            }
        }

        private bool IsConditionMet(ConditionType condition, object a, object b)
        {
            bool conditionMet = false;

            // Check for operations that can be used without type conversion
            if (condition == ConditionType.Equal || 
                condition == ConditionType.NotEqual)
            {
                // Use a to check for equality
                if (a != null)
                {
                    conditionMet = a.Equals(b);
                }
                // Use b to check for equality
                else if (b != null)
                {
                    conditionMet = b.Equals(a);
                }
                else
                {
                    // Both elements are null - equal
                    conditionMet = true;
                }

                // Toggle the conditional result
                if (condition == ConditionType.NotEqual)
                    conditionMet = !conditionMet;
                
                return conditionMet;
            }
            
            // Convert into widest possible type to perform the operations
            decimal numA = Convert.ToDecimal(a);
            decimal numB = Convert.ToDecimal(b);

            switch (condition)
            {
                case ConditionType.GreaterThan:
                    {
                        // Check for greater than
                        conditionMet = (numA > numB);
                        break;
                    }

                case ConditionType.GreaterThanOrEqual:
                    {
                        // Check for greater than or equal to
                        conditionMet = (numA >= numB);
                        break;
                    }

                case ConditionType.LessThan:
                    {
                        // Check for less than
                        conditionMet = (numA < numB);
                        break;
                    }

                case ConditionType.LessThanOrEqual:
                    {
                        // Check for less than or equal to
                        conditionMet = (numA <= numB);
                        break;
                    }
            }

            return conditionMet;
        }

        private object GetPropertyValue(SerializedProperty property)
        {
            // Get the parent type
            Type type = property.serializedObject.targetObject.GetType();

            // Check for error
            if (type == null)
                return null;

            // Get the field
            FieldInfo field = type.GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Check for error
            if (field == null)
                return null;

            // Try to get the value
            return field.GetValue(property.serializedObject.targetObject);
        }
    }
#endif
}
