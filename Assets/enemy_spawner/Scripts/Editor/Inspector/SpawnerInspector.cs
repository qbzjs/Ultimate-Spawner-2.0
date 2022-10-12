using System;
using System.Collections.Generic;
using System.Reflection;
using Trivial.ImGUI;
using UltimateSpawner.Despawning;
using UltimateSpawner.Spawning;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace UltimateSpawner.Editor
{
    [CustomEditor(typeof(Spawner), true)]
    internal sealed class SpawnerInspector : ImInspectorWindow<Spawner>
    {
        // Private
        private bool hasValidEndPoint = true;

        // Methods
        public override void OnImGUI()
        {
            // Mono script field
            ImGUI.PushEnabledVisualState(false);
            ImGUILayout.PropertyField(MonoScriptProperty());
            ImGUI.PopVisualState();

            // Main spawner fields
            GUISpawnerFields();

            // Display public properties
            foreach(SerializedProperty property in VisibleProperties())
            {
                // Draw the property
                ImGUILayout.PropertyField(property);
            }

            // Display despawner info
            GUIDespawnerFields();

            // Display spawner information
            GUISpawnInformation();
        }

        private void GUISpawnerFields()
        {
            // Spawn items
            if(InspectedValue.Parent == null)
            {
                // Display the default property
                ImGUILayout.PropertyField(serializedObject.FindProperty("spawnItems"));
            }
            else
            {
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    ImGUI.PushEnabledVisualState(false);

                    // Label
                    ImGUI.SetNextWidth(InspectorLabelWidth);
                    ImGUILayout.Label("Spawn Items");

                    // Field
                    ImGUILayout.TextField(string.Format("<Items inherited from '{0}'>", InspectedValue.Parent.name));

                    ImGUI.PopVisualState();
                }
                ImGUILayout.EndLayout();
            }


            // Spawn mask
            ImGUILayout.PropertyField(serializedObject.FindProperty("spawnMask"));

            if(InspectedValue.Parent != null)
            {
                if(InspectedValue.Parent.SpawnableMask.NoMask == false)
                {
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        ImGUILayout.Space((int)InspectorLabelWidth);
                        ImGUILayout.HelpBox(string.Format("Some items may be masked by parent '{0}'", InspectedValue.Parent.name));
                    }
                    ImGUILayout.EndLayout();
                }
            }
        }

        private void GUIDespawnerFields()
        {
            Spawner spawner = target as Spawner;

            // Get all despawners
            IList<Despawner> despawners = spawner.Despawners;

            if(despawners.Count > 0)
            {
                // Make sure all despawners components are hidden
                foreach (Despawner despawner in despawners)
                    if ((despawner.hideFlags & HideFlags.HideInInspector) == 0)
                        despawner.hideFlags |= HideFlags.HideInInspector;

                spawner.despawnersExpanded = EditorGUILayout.Foldout(spawner.despawnersExpanded, "Despawners");

                if (spawner.despawnersExpanded == true)
                {
                    // Draw all despawners
                    foreach (Despawner despawner in despawners)
                    {
                        GUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Space(16);

                                // Display foldout
                                despawner.despawnerExpanded = EditorGUILayout.Foldout(despawner.despawnerExpanded, despawner.GetType().Name);

                                // Push to right
                                GUILayout.FlexibleSpace();

                                // Include toggle
                                despawner.ApplyDespawnerToSpawnedItems = GUILayout.Toggle(despawner.ApplyDespawnerToSpawnedItems, new GUIContent(string.Empty, "Apply this despawner to all spawned items"));

                                // Delete button
                                if(GUILayout.Button(new GUIContent("X", "Delete this despawner component")) == true)
                                {
                                    // Destroy the despawner
                                    DestroyImmediate(despawner);
                                }
                            }
                            GUILayout.EndHorizontal();

                            if (despawner.despawnerExpanded == true)
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Space(16);

                                    GUILayout.BeginVertical();
                                    {
                                        if (despawner != null)
                                        {
                                            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(despawner);

                                            editor.DrawDefaultInspector();
                                        }
                                    }
                                    GUILayout.EndVertical();
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                        GUILayout.EndVertical();
                    }
                }
            }
        }

        private void GUISpawnInformation()
        {
            // Check for spawn group
            if(InspectedValue is SpawnerGroup)
            {
                if((InspectedValue as SpawnerGroup).IsSelectionModeTargetBased() == true)
                {
                    if (FindObjectOfType<SpawnerTarget>() == null)
                    {
                        ImGUILayout.HelpBox("Distance spawning methods require a 'SpawnerTarget' script to be active in the scene in order for distance measurements to be taken. If you plan to instantiate an object with a 'SpawnerTarget' script attached at runtime then you can ignore this message", MessageType.Warning);
                    }
                }

                // Get the spawner group
                SpawnerGroup group = InspectedValue as SpawnerGroup;

                // Check for change
                if (Event.current.type == EventType.Layout)
                    hasValidEndPoint = group.HasValidEndPointSpawner();

                // Check for valid end point
                if (hasValidEndPoint == false)
                {
                    ImGUILayout.HelpBox("This group spawner does not have a valid end point spawner. You must have a valid end point spawner such as a 'Spawn Point' as a child of this group spawner otherwise spawn requests will fail", MessageType.Warning);
                }

                if(group is SpawnTriggerVolume)
                {
                    bool validTrigger = false;

                    foreach (Collider col in group.GetComponents<Collider>())
                    {
                        if (col.isTrigger == true)
                        {
                            validTrigger = true;
                            break;
                        }
                    }

                    foreach(Collider2D col in group.GetComponents<Collider2D>())
                    {
                        if(col.isTrigger == true)
                        {
                            validTrigger = true;
                            break;
                        }
                    }

                    if(validTrigger == false)
                    {
                        ImGUILayout.HelpBox("This trigger spawner does not have a valid trigger collider attached. You must attach a 2D or 3D trigger collider in order for it to function correctly", MessageType.Warning);
                    }
                }
            }
            else if(InspectedValue is SpawnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(Vector3.zero, out hit, float.MaxValue, 1) == false)
                    ImGUILayout.HelpBox("This Nav Mesh spawner will be unusable in this scene because there is no baked navmesh data", MessageType.Warning);


                if ((InspectedValue as SpawnNavMesh).IsSpawnModeDistanceBased() == true)
                {
                    if (FindObjectOfType<SpawnerTarget>() == null)
                    {
                        ImGUILayout.HelpBox("Distance spawning methods require a 'SpawnerTarget' script to be active in the scene in order for distance measurements to be taken. If you plan to instantiate an object with a 'SpawnerTarget' script attached at runtime then you can ignore this message", MessageType.Warning);
                    }
                }

                if((InspectedValue as SpawnNavMesh).occupiedCheck == OccupiedCheck.PhysicsTrigger)
                {
                    ImGUILayout.HelpBox("'Physics Trigger' occupied checks are not supported by this component!", MessageType.Warning);
                }
            }

            // Availablity and spawning
            ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
            {
                // Push to right
                ImGUILayout.Space((int)InspectorLabelWidth);

                // Begin main layout
                ImGUI.SetNextStyle(ImGUIStyle.HelpBox);
                ImGUILayout.BeginLayout(ImGUILayoutType.Vertical);
                {
                    //// Available field
                    //ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    //{
                    //    // Label
                    //    ImGUILayout.Label("Is Available:");
                    //    ImGUILayout.Space();

                    //    // Toggle
                    //    ImGUI.SetNextWidth(26);
                    //    ImGUILayout.Toggle(InspectedValue.IsAvailable);
                    //}
                    //ImGUILayout.EndLayout();


                    //// Spawn capacity field
                    //ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    //{
                    //    // Label
                    //    ImGUILayout.Label("Spawn Item Capacity:");                        
                    //    ImGUILayout.Space();

                    //    // Number field
                    //    ImGUI.SetNextWidth(60);
                    //    ImGUILayout.IntField(InspectedValue.SpawnableItemCapacity);
                    //}
                    //ImGUILayout.EndLayout();


                    ImGUI.PushEnabledVisualState(false);
                    {
                        foreach (MemberInfo member in GetMembersWithAttribute<Spawner.SpawnerInfoAttribute>(InspectedValue.GetType()))
                        {
                            // Check for attribute
                            if (member.IsDefined(typeof(Spawner.SpawnerInfoAttribute), true) == true)
                            {
                                // Get the attribute
                                Spawner.SpawnerInfoAttribute attrib = member.GetCustomAttributes(typeof(Spawner.SpawnerInfoAttribute), false)[0] as Spawner.SpawnerInfoAttribute;

                                // Get the display name
                                string name = (attrib.displayName != null) ? attrib.displayName : member.Name;

                                if (member.MemberType == MemberTypes.Field)
                                {
                                    FieldInfo field = member as FieldInfo;

                                    // Draw the field
                                    GUIGenericInfoField(name, field.GetValue(InspectedValue));
                                }
                                else if (member.MemberType == MemberTypes.Property)
                                {
                                    PropertyInfo property = member as PropertyInfo;

                                    // Draw the field
                                    GUIGenericInfoField(name, property.GetValue(InspectedValue, null));
                                }
                            }
                        }
                    }
                    ImGUI.PopVisualState();

                    // Spawn button
                    if(ImGUILayout.Button("Spawn Item") == true)
                    {
                        // Try to spawn
                        InspectedValue.Spawn();
                    }
                }
                ImGUILayout.EndLayout();
            }
            ImGUILayout.EndLayout();
        }

        private void GUIGenericInfoField(string name, object value)
        {
            if (value == null)
                return;

            // Get title case string            
            name = ObjectNames.NicifyVariableName(name);

            ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
            {
                // Label
                ImGUI.SetNextSize(200, 20, ImGUISizeMode.MaxSize);
                ImGUILayout.Label(name + ":");
                ImGUILayout.Space();

                // Number field
                ImGUI.SetNextWidth(60);

                if (value is bool) ImGUILayout.Toggle((bool)value);
                else if (value is int) ImGUILayout.IntField((int)value);
                else if (value is float) ImGUILayout.FloatField((float)value);
                else if (value is string) ImGUILayout.TextField((string)value);
                else ImGUILayout.TextField("Cannot display!");
            }
            ImGUILayout.EndLayout();
        }

        private IEnumerable<MemberInfo> GetMembersWithAttribute<T>(Type type) where T : Attribute
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            // Check for base type - depth first
            if (type.BaseType != null)
                foreach (MemberInfo info in GetMembersWithAttribute<T>(type.BaseType))
                    yield return info;

            // Check for this type
            foreach (MemberInfo member in type.GetMembers(flags))
            {
                // Check for attribute
                if(member.IsDefined(typeof(T), false) == true)
                    yield return member;
            }
        }
    }
}
