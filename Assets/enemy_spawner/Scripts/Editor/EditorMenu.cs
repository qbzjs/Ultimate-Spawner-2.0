using System;
using UnityEngine;
using UnityEditor;

using UltimateSpawner.Spawning;
using UltimateSpawner.Despawning;

namespace UltimateSpawner.Editor
{
    public class EditorMenu
    {
        // Public
        public const string gameObjectMenuPath = "GameObject/Ultimate Spawner/";
        public const string componentMenuPath = "Component/Ultimate Spawner/";

        // Methods
        [MenuItem(gameObjectMenuPath + "Spawn Transform", false, 19)]
        public static SpawnTransform CreateSpawnTransform()
        {
            // Create the transform
            return CreateObjectWithMainComponent<SpawnTransform>("Spawn Transform");
        }

        [MenuItem(gameObjectMenuPath + "Spawn Point", false, 20)]
        public static SpawnPoint CreateSpawnPoint()
        {
            // Create the spawn point
            return CreateObjectWithMainComponent<SpawnPoint>("Spawn Point");
        }

        [MenuItem(gameObjectMenuPath + "Spawn Area", false, 21)]
        public static SpawnArea CreateSpawnArea()
        {
            // Create the spawn area
            return CreateObjectWithMainComponent<SpawnArea>("Spawn Area");
        }

        [MenuItem(gameObjectMenuPath + "Spawn Nav Mesh", false, 22)]
        public static SpawnNavMesh CreateSpawnNavMesh()
        {
            return CreateObjectWithMainComponent<SpawnNavMesh>("Spawn Nav Mesh");
        }

        [MenuItem(gameObjectMenuPath + "Spawn Bounds", false, 23)]
        public static SpawnBounds CreateSpawnBounds()
        {
            return CreateObjectWithMainComponent<SpawnBounds>("Spawn Bounds");
        }

        [MenuItem(gameObjectMenuPath + "Spawn Collider Bounds", false, 24)]
        public static SpawnColliderBounds CreateSpawnColliderBounds()
        {
            return CreateObjectWithMainComponent<SpawnColliderBounds>("Spawn Collider Bounds");
        }

        [MenuItem(gameObjectMenuPath + "Spawn Group", false, 43)]
        public static SpawnerGroup CreateSpawnGroup()
        {
            // Create the spawn group
            return CreateObjectWithMainComponent<SpawnerGroup>("Spawn Group");
        }

        [MenuItem(gameObjectMenuPath + "Spawn Trigger Volume", false, 44)]
        public static SpawnTriggerVolume CreateSpawnTriggerVolume()
        {
            // Create the spawn trigger volume
            return CreateObjectWithMainComponent<SpawnTriggerVolume>("Spawn Trigger Volume");
        }


        // #### Spawn controllers ####
        [MenuItem(gameObjectMenuPath + "Infinite Controller", false, 61)]
        public static InfiniteSpawnController CreateInfiniteSpawnController()
        {
            // Create the infinite spawn controller
            return CreateObjectWithMainComponent<InfiniteSpawnController>("Infinite Spawn Controller");
        }

        [MenuItem(gameObjectMenuPath + "Trigger Controller", false, 62)]
        public static TriggerSpawnController CreateTriggerSpawnController()
        {
            // Create the trigger spawn controller
            return CreateObjectWithMainComponent<TriggerSpawnController>("Trigger Spawn Controller");
        }

        [MenuItem(gameObjectMenuPath + "Event Controller", false, 63)]
        public static EventSpawnController CreateEventSpawnController()
        {
            // Create the event spawn controller
            return CreateObjectWithMainComponent<EventSpawnController>("Event Spawn Controller");
        }


        // #### Despawners ####
        [MenuItem(componentMenuPath + "Despawn After Time", false, 81)]
        public static void CreateDespawnAfterTime()
        {
            AttachComponentToSelection<DespawnAfterTime>();
        }

        [MenuItem(componentMenuPath + "Despawn After Amount", false, 82)]
        public static void CreateDespawnAfterAmount()
        {
            AttachComponentToSelection<DespawnAfterAmount>();
        }

        [MenuItem(componentMenuPath + "Despawn On Collision", false, 83)]
        public static void CreateDespawnOnCollision()
        {
            AttachComponentToSelection<DespawnOnCollision>();
        }

        [MenuItem(componentMenuPath + "Despawn On Trigger", false, 84)]
        public static void CreateDespawnOnTrigger()
        {
            AttachComponentToSelection<DespawnOnTrigger>();
        }

        [MenuItem(componentMenuPath + "Despawn On Event", false, 85)]
        public static void CreateDespawnOnEvent()
        {
            AttachComponentToSelection<DespawnOnEvent>();
        }

        [MenuItem(componentMenuPath + "Despawn Distance", false, 86)]
        public static void CreateDespawnDistance()
        {
            AttachComponentToSelection<DespawnDistance>();
        }


        protected static T CreateObjectWithMainComponent<T>(string objectName, bool select = true) where T : Component
        {
            // Check for selected object
            GameObject selected = Selection.activeGameObject;

            // Create the game object
            GameObject go = new GameObject(objectName, typeof(T));

            // Select the object
            Selection.activeGameObject = go;

            // Make child of selection
            if(selected != null)
                go.transform.SetParent(selected.transform);

            // Setup undo functionality
            Undo.RegisterCreatedObjectUndo(go, "Create " + objectName);

            // Return the component
            return go.GetComponent<T>();
        }

        protected static void AttachComponentToSelection<T>() where T : Component
        {
            // Get all valid selected game objects
            GameObject[] filteredSelection = Selection.GetFiltered<GameObject>(SelectionMode.Editable);

            // Add undo snapshot
            Undo.RegisterCompleteObjectUndo(filteredSelection, "Add " + typeof(T).Name);

            // Get active objects
            foreach(GameObject selected in filteredSelection)
            {
                // Add the new component
                selected.AddComponent<T>();
            }
        }
    }
}
