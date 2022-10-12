using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// A <see cref="SpawnTriggerVolume"/> is a trigger collider of any shape which detects when certain objects are within the volume.
    /// Volumes are only able to spawn when a tagged object (Usually a player) is inside the volume. 
    /// This allows spawning to only take place near the player. This is especially useful for large or complex maps.
    /// The volume is considered available when one or more specified tagged objects are within the volume and atleast one of its child <see cref="Spawner"/> is available. 
    /// </summary>
    public sealed class SpawnTriggerVolume : SpawnerGroup
    {
        // Private
        private HashSet<Collider> colliding3D = new HashSet<Collider>();        // 3D colliders that are inside the spawn point
        private HashSet<Collider2D> colliding2D = new HashSet<Collider2D>();    // 2D colliders that are inside the spawn point
        
        // Public
        /// <summary>
        /// Tags that are able to activate and deactivate a <see cref="SpawnTriggerVolume"/>.
        /// By default, 'Player' is the only trigger tag.
        /// </summary>
        [TagCollection]
        public string[] triggerTags =
        {
            "Player",
        };

        // Methods
        /// <summary>
        /// Returns true if the volume is triggered and any child <see cref="Spawner"/> is available for spawning or false if there are none available. 
        /// The volume is triggered when one or more physical game objects with matching trigger tags are inside the collider bounds. 
        /// </summary>
        public override bool IsAvailable
        {
            get
            {
                // Check for available child spawners
                bool available = base.IsAvailable;

                // Check for available
                if (available == true)
                {
                    if (colliding3D.Count == 0 && colliding2D.Count == 0)
                        available = false;
                }

                return available;
            }
        }

        /// <summary>
        /// Returns the number of items that can be spawned at this <see cref="Spawner"/> on the current frame.
        /// Note that this property takes into account the availablity of the <see cref="Spawner"/>. 
        /// </summary>
        public override int AvailableSpawnableItemCapacity
        {
            get
            {
                int available = base.AvailableSpawnableItemCapacity;

                if(available > 0)
                {
                    if (colliding3D.Count == 0 && colliding2D.Count == 0)
                        available = 0;
                }

                return available;
            }
        }

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
            }
        }
        #endregion
    }
}
