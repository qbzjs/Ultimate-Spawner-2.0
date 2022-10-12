using System;
using Trivial.ImGUI;
using UnityEngine;
using UnityEditor;
using UltimateSpawner.Spawning;
using System.Collections.Generic;

namespace UltimateSpawner.Editor.Drawer
{
    internal struct SpawnableMaskInfo
    {
        public int spawnableID;
        public string spawnableName;
        public bool spawnableMasked;
        public bool spawnableInheritMasked;
    }

    [CustomPropertyDrawer(typeof(SpawnableMask))]
    internal sealed class SpawnableMaskDrawer : ImPropertyDrawer<SpawnableMask>
    {
        // Methods
        public override float OnImGUICalculatePropertyHeight(float defaultHeight, SerializedProperty property, GUIContent content)
        {
            // Get the number of spawnable items
            int spawnableCount = FindSpawnableItemCount();

            // Check for no mask
            if (InspectedValue.NoMask == true)
                spawnableCount = 0;

            int requiredCount = spawnableCount + 1;

            // Check for all masked
            if (InspectedValue.IsAllMasked() == true)
                requiredCount++;

            // Calcualte required pixel height
            return requiredCount * InspectorLabelHeight;
        }

        public override void OnImGUI(Rect area, GUIContent content)
        {
            // Property label
            ImGUI.SetNextTooltip(content.tooltip);
            ImGUI.SetNextPosition(area.position);
            ImGUI.SetNextSize(InspectorLabelWidth, area.height);
            ImGUI.Label(content.text);

            // Create the next rect
            Rect next = new Rect(area.x + InspectorLabelWidth, area.y, area.width - InspectorLabelWidth, area.height);
            
            // Display a background box
            ImGUI.SetNextStyle(ImGUIStyle.HelpBox);
            ImGUI.SetNextPosition(next.position);
            ImGUI.SetNextSize(next.size);
            ImGUI.Box(string.Empty);

            // Update next height
            next.height = InspectorLabelHeight;

            // Get the spawnable items
            SpawnableMaskInfo[] items = FindSpawnableItems();

            // Check for no items
            if(items.Length == 0)
            {
                ImGUI.SetNextPosition(next.position);
                ImGUI.SetNextSize(next.size);
                ImGUI.Label("<No spawnable items defined>");
            }
            else
            {
                // Render a mask field
                InspectedValue.NoMask = GUIMaskField(next, "All (No Masked Items)", InspectedValue.NoMask);

                
                // Check for no mask
                if(InspectedValue.NoMask == false)
                {
                    // Separator line
                    Rect line = new Rect(next.x, next.y + 16, next.width, 1);

                    GUI.Box(line, GUIContent.none);


                    // Update next rect
                    next.y += InspectorLabelHeight;                    

                    // Draw all spawnable items
                    for(int i = 0; i < items.Length; i++)
                    {
                        ImGUI.PushEnabledVisualState(items[i].spawnableInheritMasked == false);

                        // Draw the masked field
                        bool isChecked = GUIMaskField(next, items[i].spawnableName, items[i].spawnableMasked);

                        // Check for toggle changed
                        if(isChecked != items[i].spawnableMasked)
                        {
                            // Mask or unmask item
                            if (isChecked == true)
                                InspectedValue.UnmaskItem(items[i].spawnableID);
                            else
                                InspectedValue.MaskItem(items[i].spawnableID);
                        }

                        ImGUI.PopVisualState();

                        // Update spacing
                        next.y += InspectorLabelHeight;
                    }

                    // Check for all masked items
                    if(InspectedValue.IsAllMasked() == true)
                    {
                        ImGUI.SetNextTooltip("This object will not be able to spawn any items because every item is masked. You should unmask atleast 1 item to ensure that spawning can occur");
                        ImGUI.SetNextPosition(next.position);
                        ImGUI.SetNextSize(next.size);
                        ImGUI.Label("<All items are masked!>");
                    }
                }
                else
                {
                    // Make sure any masked items are cleared
                    InspectedValue.ClearMask();
                }
            }
        }

        private bool GUIMaskField(Rect area, string name, bool isChecked)
        {
            int toggleWidth = 30;

            // Create gui rects
            Rect labelArea = new Rect(area.x, area.y, area.width - toggleWidth, area.height);
            Rect toggleArea = new Rect(area.x + (area.width - toggleWidth), area.y, toggleWidth, area.height);

            // Draw the label
            ImGUI.SetNextPosition(labelArea.position);
            ImGUI.SetNextSize(labelArea.size);
            ImGUI.Label(name);

            // Draw the toggle
            ImGUI.SetNextPosition(toggleArea.position);
            ImGUI.SetNextSize(toggleArea.size);
            return ImGUI.Toggle(isChecked);
        }

        private SpawnableMaskInfo[] FindSpawnableItems()
        {
            SpawnableItem[] items = null;

            //// Check for error
            //if (InspectedValue.SpawnableItems == null || InspectedValue.SpawnableItems.items == null)
            //{
            //    if(InspectedValue.Parent == null)
            //        return new SpawnableMaskInfo[0];
            //}


            SpawnableMask mask = InspectedValue;

            // Move up the hierarchy to inherit the spawner items
            while(mask != null)
            {
                if(mask.Parent != null)
                {
                    // Move up the hierarchy
                    mask = mask.Parent;
                }
                else
                {
                    // Store items
                    if (mask.SpawnableItems != null)
                        items = mask.SpawnableItems.items;

                    mask = null;
                }
            }

            if (items == null)
                return new SpawnableMaskInfo[0];


            List<SpawnableMaskInfo> maskedItems = new List<SpawnableMaskInfo>();
            
            foreach(SpawnableItem item in items)
            {
                // Check for parent
                if(InspectedValue.Parent != null)
                {
                    if(InspectedValue.Parent.IsMasked(item.SpawnableID) == true)
                    {
                        // Make sure the item is masked
                        InspectedValue.MaskItem(item.SpawnableID);
                    }
                }

                // Check for empt slots
                if (item.provider == null || item.provider.IsAssigned == false)
                    continue;

                // Create the info
                maskedItems.Add(new SpawnableMaskInfo
                {
                    spawnableID = item.SpawnableID,
                    spawnableName = item.provider.ItemName,
                    spawnableMasked = InspectedValue.IsMasked(item.SpawnableID) == false,
                    spawnableInheritMasked = (InspectedValue.Parent != null) ? InspectedValue.Parent.IsMasked(item.SpawnableID) : false,
                });
            }
            return maskedItems.ToArray();
        }

        private int FindSpawnableItemCount()
        {
            // Check for error
            if (InspectedValue.SpawnableItems == null || InspectedValue.SpawnableItems.items == null)
                return 0;

            // Count items
            int count = 0;

            foreach(SpawnableItem item in InspectedValue.SpawnableItems.items)
            {
                // Skip empty slots
                if (item.provider == null || item.provider.IsAssigned == false)
                    continue;

                // Increase size
                count++;
            }

            return count;
        }
    }
}
