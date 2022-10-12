using System;
using System.Collections;
using System.Collections.Generic;
using UltimateSpawner.Spawning;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateSpawner
{
    /// <summary>
    /// A <see cref="TriggerSpawnController"/> is a spawn controller that will issue a single spawn request to its assigned spawner when its trigger volume has been activated.
    /// This is useful if you want to spawn items at certain points in the game triggered by the player moving through the level or similar.
    /// </summary>
    [Serializable]
    public class TriggerSpawnController : SpawnController
    {
        // Types
        /// <summary>
        /// The trigger mode used to indicate when an istem spawn request should be issues in relation to the collision interaction.
        /// </summary>
        public enum TriggerSpawnMode
        {
            /// <summary>
            /// The item will be spawned when a physical object enters the trigger volume.
            /// </summary>
            OnEnter,
            /// <summary>
            /// The item will be spawned when a physical object exits the trigger volume.
            /// </summary>
            OnExit,
        }

        // Private
        private HashSet<Collider> colliding3D = new HashSet<Collider>();        // 3D colliders that are inside the spawn point
        private HashSet<Collider2D> colliding2D = new HashSet<Collider2D>();    // 2D colliders that are inside the spawn point
        private int spawnRequests = 0;
        private int spawnCount = 0;

        // Public
        /// <summary>
        /// When true, the controller will only be able to trigger a spawn request once and then will ignore any further trigger interactions.
        /// </summary>
        [Tooltip("Is the controller limited to triggering one spawn request or more")]
        public bool triggerOnce = false;
        /// <summary>
        /// The trigger mode used to determine when an item spawn request should be issued.
        /// </summary>
        [Tooltip("Determines when the item spawn request is triggered")]
        public TriggerSpawnMode triggerMode = TriggerSpawnMode.OnEnter;

        /// <summary>
        /// Tags that are able to activate and deactivate a <see cref="SpawnTriggerVolume"/>.
        /// By default, 'Player' is the only trigger tag.
        /// </summary>
        [TagCollection]
        public string[] triggerTags =
        {
            "Player",
        };

        // Events
        /// <summary>
        /// Called when the tigger controller has been activated and will send a spawn request to the associated spawner.
        /// Note that this event is triggered immediatley but <see cref="SpawnController.OnControllerSpawnedItem(Transform)"/> may occur much later as it depends upon the availablity of the spawner.
        /// </summary>
        [HideInInspector]
        [Tooltip("Called when the controller has been triggered to spawn a new item")]
        public UnityEvent OnSpawnerTrigger;

        // Methods
        /// <summary>
        /// The main spawn routine for the trigger controller.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator SpawnRoutine()
        {
            // Loop while the spawner is running
            while(IsSpawning == true)
            {
                // Check for any spawn requests
                if(spawnRequests > 0)
                {
                    // Spawn the item immediatley
                    yield return StartCoroutine(ItemSpawnRoutine());

                    // Reduce request couter
                    spawnRequests--;
                }

                // Wait a frame
                yield return null;
            }
        }

        /// <summary>
        /// Indicate that the trigger controller should attempt to spawn an item at its assigned spawner.
        /// This method will fail silently if <see cref="triggerOnce"/> or <see cref="spawnCount"/> conditions are not met.
        /// </summary>
        public void TriggerSpawn()
        {
            // Check for trigger once
            if (triggerOnce == true && spawnCount > 0)
                return;
            
            // Add to spawn requests
            spawnRequests++;
            spawnCount++;

            // Trigger event
            OnSpawnerTrigger.Invoke();
        }

        // Methods
        #region PhysicsEvents
        /// <summary>
        /// Called by Unity and is used to detect when 3D physics colliders enter the <see cref="SpawnTriggerVolume"/>. 
        /// </summary>
        /// <param name="other">The collider that caused the event</param>
        public void OnTriggerEnter(Collider other)
        {
            // Dont detect trigger objects and only continue if trigger events are used
            if (other.isTrigger == true)
                return;

            // Check if the colliding object is tagged
            if (UltimateSpawning.IsTagged(other.gameObject, triggerTags) == true)
            {
                // Make sure the collider does not already exist for some unknown reason
                if (colliding3D.Contains(other) == false)
                {
                    // Add to collision list
                    colliding3D.Add(other);

                    // Spawn an item
                    if(triggerMode == TriggerSpawnMode.OnEnter)
                    {
                        // Trigger an item spawn
                        TriggerSpawn();
                    }
                }
            }
        }

        /// <summary>
        /// Called by Unity and is used to detect when 3D physics colliders exit the <see cref="SpawnTriggerVolume"/>. 
        /// </summary>
        /// <param name="other">The collider that caused the event</param>
        public void OnTriggerExit(Collider other)
        {
            // Dont detect trigger objects and only continue if trigger events are used
            if (other.isTrigger == true)
                return;

            // Check if the object is registered with our spawn point
            if (colliding3D.Contains(other) == true)
            {
                // Remove from colliding list
                colliding3D.Remove(other);

                // Spawn an item
                if (triggerMode == TriggerSpawnMode.OnExit)
                {
                    // Trigger an item spawn
                    TriggerSpawn();
                }
            }
        }

        /// <summary>
        /// Called by Unity and is used to detect when 2D physics colliders enter the <see cref="SpawnTriggerVolume"/>. 
        /// </summary>
        /// <param name="other">The collider that caused the event</param>
        public void OnTriggerEnter2D(Collider2D other)
        {
            // Dont detect trigger objects and only continue if trigger events are used
            if (other.isTrigger == true)
                return;

            // Check if the colliding object is tagged
            if (UltimateSpawning.IsTagged(other.gameObject, triggerTags) == true)
            {
                // Make sure the collider does not already exist for some unknown reason
                if (colliding2D.Contains(other) == false)
                {
                    // Add to collision list
                    colliding2D.Add(other);

                    // Spawn an item
                    if (triggerMode == TriggerSpawnMode.OnEnter)
                    {
                        // Trigger an item spawn
                        TriggerSpawn();
                    }
                }
            }
        }

        /// <summary>
        /// Called by unity and is used to detect when 2D physics colliders exit the <see cref="SpawnTriggerVolume"/>. 
        /// </summary>
        /// <param name="other">The collider that caused the event</param>
        public void OnTriggerExit2D(Collider2D other)
        {
            // Dont detect trigger objects and only continue if trigger events are used
            if (other.isTrigger == true)
                return;

            // Check if the object is registered with our spawn point
            if (colliding2D.Contains(other) == true)
            {
                // Remove from colliding list
                colliding2D.Remove(other);

                // Spawn an item
                if (triggerMode == TriggerSpawnMode.OnExit)
                {
                    // Trigger an item spawn
                    TriggerSpawn();
                }
            }
        }
        #endregion
    }
}
