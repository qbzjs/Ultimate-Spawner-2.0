using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using UltimateSpawner.Spawning;
using UltimateSpawner.Despawning;
using System.Collections.Generic;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UltimateSpawner
{
#if UNITY_EDITOR
    internal sealed class InspectorValueWatcher
    {
        // Private
        private string fieldName = null;
        private FieldInfo field = null;
        private object lastValue = null;

        // Constructor
        public InspectorValueWatcher(string fieldName)
        {
            this.fieldName = fieldName;
        }

        // Methods
        /// <summary>
        /// Returns true if the watched value has been changed.
        /// </summary>
        /// <param name="instance">The instance of the type to check the field of</param>
        /// <returns>True if the value has change since the last poll or false if not</returns>
        public bool HasChanged(object instance)
        {
            // Check for null initials
            if (field == null && lastValue == null)
            {
                // Get the field
                this.field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                // Check for field
                if (field == null)
                {
                    Debug.LogWarning("Failed to find the watched field '" + fieldName + "'");
                    lastValue = new object();
                    return false;
                }


                // Get the value
                this.lastValue = field.GetValue(instance);
            }

            // Check for null field
            if (field == null)
                return false;

            // Get the current value#
            object currentValue = field.GetValue(instance);


            bool result = false;

            // Check for equality
            if (lastValue.Equals(currentValue) == false)
            {
                result = true;
                lastValue = currentValue;
            }

            return result;
        }
    }
#endif

    /// <summary>
    /// Utility class for Ultimate Spawner.
    /// </summary>
    public class UltimateSpawning
    {
        // Events
        /// <summary>
        /// Delegate that will be invoked when Ultimate Spawner needs to instantiate an object
        /// If this delegate is null then Ultimate Spawner will instantiate the object.
        /// This allows you to easily hook up pooling support by managing the instantiate behaviour.
        /// </summary>
        public static Func<Object, Vector3, Quaternion, Object> OnUltimateSpawnerInstantiate;

        public static Action<Object> OnUltimateSpawnerDestroy;

        // Public 
        /// <summary>
        /// The default 2d mode for t Ultimate Spawner.
        /// </summary>
        public const bool in2DModeDefault = false;

        // Methods
        /// <summary>
        /// Called by Ultimate Spawner whenever an object needs to be instantiated.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="pos">The position to instantiate at</param>
        /// <param name="rot">The initial rotation of the object</param>
        /// <returns></returns>
        public static Object UltimateSpawnerInstantiate(Object prefab, Vector3 pos, Quaternion rot)
        {
            // Check for user event
            if(OnUltimateSpawnerInstantiate != null)
            {
                try
                {
                    // Try to create instance
                    Object result = OnUltimateSpawnerInstantiate(prefab, pos, rot);

                    // Check for valid result
                    if (result != null)
                    {
                        return result;
                    }
                    else
                    {
                        Debug.LogWarning("User callback 'OnUltimateSpawnerInstantiate': returned an invalid value (null). Falling back to default instantiate method to avoid error");
                    }
                }
                catch(Exception e)
                {
                    Debug.LogWarningFormat("Exception thrown by user callback 'OnUltimateSpawnerInstantiate': {0}. Falling back to defualt instantiate method to avoid error", e);
                }
            }

            // Use default method
            return Object.Instantiate(prefab, pos, rot);
        }

        public static void UltimateSpawnerDestroy(Object target)
        {
            if(OnUltimateSpawnerDestroy != null)
            {
                try
                {
                    // Try to despawn
                    OnUltimateSpawnerDestroy(target);
                    return;
                }
                catch(Exception e)
                {
                    Debug.LogWarningFormat("Exception thrown by user callback 'OnUltimateSpawnerDestroy': {0}. Falling back to defualt destroy method to avoid error", e);
                }
            }

            // Destroy the object
            Object.Destroy(target);
        }

        public static Transform Spawn(SpawnableItems spawnableItemsPool, SpawnableItemRef spawnableItemRef = null, SpawnableMask spawnItemMask = null, SpawnLocation spawnLocation = default(SpawnLocation), SpawnRotationApplyMode applyRotation = SpawnRotationApplyMode.FullRotation)
        {
            Transform spawnedItem = spawnableItemsPool.CreateSpawnableItem(spawnableItemRef, spawnItemMask, spawnLocation, applyRotation);

            if(spawnedItem != null)
            {
                // Apply despawners
                spawnLocation.Owner.ApplyDespawners(spawnedItem);

                // Trigger spawned events
                foreach (ISpawnEventReceiver receiver in spawnedItem.GetComponentsInChildren<ISpawnEventReceiver>())
                {
                    try
                    {
                        // Trigger the event
                        receiver.OnSpawned(spawnLocation);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            return spawnedItem;
        }

        public static void Despawn(Object spawnableItem)
        {
            if(spawnableItem is GameObject)
            {
                // Get the destroy object
                GameObject destroyObject = spawnableItem as GameObject;

                // Check for the spawnable identity component
                SpawnableIdentity id = destroyObject.GetComponent<SpawnableIdentity>();

                if(id != null)
                {
                    Despawn(id);
                }
                else
                {
                    // Fallback to default destroy behaviour with support for pooling
                    UltimateSpawnerDestroy(spawnableItem);
                }
            }
            else if(spawnableItem is Component)
            {
                if(spawnableItem is SpawnableIdentity)
                {
                    // Despawn the identity direct
                    Despawn((spawnableItem as SpawnableIdentity));
                }
                else
                {
                    // Pass object for destruction
                    Despawn((spawnableItem as Component).gameObject);
                }
            }
        }        

        public static void Despawn(SpawnableIdentity spawnItemIdentity)
        {
            // Check for null
            if (spawnItemIdentity == null)
                throw new ArgumentNullException("spawnItemIdentity");

            // Trigger despawn event
            foreach (IDespawnEventReceiver receiver in spawnItemIdentity.GetComponentsInChildren<IDespawnEventReceiver>())
            {
                try
                {
                    // Trigger the event
                    receiver.OnDespawned();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // Allow the provider to handle destruction
            spawnItemIdentity.SpawnProvider.DestroySpawnableInstance(spawnItemIdentity.gameObject);
        }

        public static DespawnAfterTime DespawnAfterTime(GameObject despawnObject, float timeInSeconds)
        {
            // Check for error
            if (despawnObject == null)
                return null;

            // Add the despawner
            DespawnAfterTime despawner = despawnObject.AddComponent<DespawnAfterTime>();

            // Despawner properties
            despawner.despawnAfterTime = timeInSeconds;

            return despawner;
        }

        public static DespawnAfterAmount DespawnAfterAmount(GameObject despawnObject, int maxAliveCount)
        {
            // Check for error
            if (despawnObject == null)
                return null;

            // Add the despawner
            DespawnAfterAmount despawner = despawnObject.AddComponent<DespawnAfterAmount>();

            // Despawner properties
            despawner.maxAllowedCount = maxAliveCount;

            return despawner;
        }

        public static DespawnOnCollision DespawnOnCollision(GameObject despawnObject, DespawnTarget despawnTarget = DespawnTarget.ThisObject, bool is2DDespawner = false, LayerMask collisionLayer = default(LayerMask), params string[] collisionTags)
        {
            // Check for error
            if (despawnObject == null)
                return null;

            // Add the despawner
            DespawnOnCollision despawner = despawnObject.AddComponent<DespawnOnCollision>();

            // Despawner properties
            despawner.despawnTarget = despawnTarget;
            despawner.is2DDespawner = is2DDespawner;
            despawner.collisionLayer = collisionLayer;
            despawner.collisionTags = collisionTags;

            return despawner;
        }

        public static DespawnOnTrigger DespawnOnTrigger(GameObject despawnObject, DespawnTarget despawnTarget = DespawnTarget.ThisObject, bool is2DDespawner = false, LayerMask triggerLayer = default(LayerMask), params string[] triggerTags)
        {
            // Check for error
            if (despawnObject == null)
                return null;

            // Add the despawner
            DespawnOnTrigger despawner = despawnObject.AddComponent<DespawnOnTrigger>();

            // Despawner properties
            despawner.despawnTarget = despawnTarget;
            despawner.is2DSpawner = is2DDespawner;
            despawner.triggerLayer = triggerLayer;
            despawner.triggerTags = triggerTags;

            return despawner;
        }

        public static DespawnOnEvent DespawnOnEvent(GameObject despawnObject, UnityEvent despawnEvent)
        {
            // Check for error
            if (despawnObject == null || despawnEvent == null)
                return null;

            // Add the despawner
            DespawnOnEvent despawner = despawnObject.AddComponent<DespawnOnEvent>();

            // Add listener
            despawnEvent.AddListener(despawner.DespawnItem);

            return despawner;
        }

        public static DespawnDistance DespawnDistance(GameObject despawnObject, DespawnDistanceMode distanceMode, float distance, string despawnerTargetTag = null)
        {
            if (despawnObject == null)
                return null;

            // Add the depsawner
            DespawnDistance despawner = despawnObject.AddComponent<DespawnDistance>();

            // Despawner properties
            despawner.despawnMode = distanceMode;
            despawner.distance = distance;
            despawner.despawnTargetTag = despawnerTargetTag;

            return despawner;
        }

        /// <summary>
        /// Log a message to the Unity editor console.
        /// The message will only be logged if 'ULTIMATE_SPAWNER_DEBUG' is defined in the player settings.
        /// </summary>
        /// <param name="format">The message format</param>
        /// <param name="args">The message arguments</param>
        public static void Log(string format, params object[] args)
        {
#if ULTIMATESPAWNER_DEBUG
            Debug.LogFormat("US 2.0 > " + format, args);
#endif
        }

        /// <summary>
        /// Returns true if the editor scene view is currently in 2D mode or false if it is not.
        /// </summary>
        /// <returns>A value indicating whether the editor is in 2d mode</returns>
        public static bool IsEditorIn2DMode()
        {
#if UNITY_EDITOR
            SceneView activeScene = null;

            // Try to find the current scene view
            activeScene = SceneView.currentDrawingSceneView;

            // Try to find the last scene view
            if (activeScene == null)
                activeScene = SceneView.lastActiveSceneView;

            // Check for error
            if (activeScene == null)
                return in2DModeDefault;

            // Check for 2d mode in the scene view
            return activeScene.in2DMode;
#else
            return in2DModeDefault;
#endif
        }

        /// <summary>
        /// Returns true if the specified game object is tagged with one of the specified tag strings.
        /// </summary>
        /// <param name="go">The game object to check</param>
        /// <param name="tags">An array of tags which should be searched for on the specified object</param>
        /// <returns>True if the specified game object is tagged with one of the specified tags or false if not</returns>
        public static bool IsTagged(GameObject go, params string[] tags)
        {
            // Check for null
            if (go == null)
                throw new ArgumentNullException("go");

            // Check for no tag
            if (tags == null || tags.Length == 0)
                if (go.CompareTag("Untagged") == true)
                    return true;

            // Check for any matching tags
            foreach (string tag in tags)
                if (go.CompareTag(tag) == true)
                    return true;

            // No matches
            return false;
        }
    }
}
