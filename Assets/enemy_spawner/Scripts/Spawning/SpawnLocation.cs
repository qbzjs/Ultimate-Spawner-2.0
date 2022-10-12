using System;
using UnityEngine;

namespace UltimateSpawner.Spawning
{
    /// <summary>
    /// The rotation mode used to describe which rotation axes should be modified during spawning.
    /// </summary>
    public enum SpawnRotationApplyMode
    {
        /// <summary>
        /// No rotation will be applied when spawning the object.
        /// </summary>
        NoRotation,
        /// <summary>
        /// Full 3 axis rotation will be applied when spawning the object.
        /// </summary>
        FullRotation,
        /// <summary>
        /// Only Y axis rotation will be applied when spawning the object.
        /// </summary>
        YRotation,
        /// <summary>
        /// Only Z axis rotation will be applied when spawning the object.
        /// </summary>
        ZRotation,
    }

    /// <summary>
    /// Represents a location and rotation in 3D or 2D space where an object can be spawned.
    /// An instance of <see cref="SpawnLocation"/> can only be created by a <see cref="Spawner"/>. 
    /// </summary>
    [Serializable]
    public struct SpawnLocation
    {
        // Private
        [SerializeField, HideInInspector]
        private bool isValid;
        [SerializeField, HideInInspector]
        private Spawner owner;
        [SerializeField, HideInInspector]
        private Vector3 spawnPosition;
        [SerializeField, HideInInspector]
        private Quaternion spawnRotation;

        // Public
        /// <summary>
        /// Get an invalid representation of a <see cref="SpawnLocation"/>. 
        /// </summary>
        public static readonly SpawnLocation invalid = new SpawnLocation();

        // Properties
        /// <summary>
        /// Returns true if this <see cref="SpawnLocation"/> is valid and able to receive <see cref="Spawn(Transform, bool, SpawnRotationApplyMode)"/> requests.  
        /// </summary>
        public bool IsValid
        {
            get { return isValid; }
        }

        /// <summary>
        /// Get the <see cref="Spawner"/> that created this <see cref="SpawnLocation"/>. 
        /// </summary>
        public Spawner Owner
        {
            get { return owner; }
        }

        /// <summary>
        /// Get the spawn location for this <see cref="SpawnLocation"/>. 
        /// </summary>
        public Vector3 SpawnPosition
        {
            get { return spawnPosition; }
            internal set { spawnPosition = value; }
        }

        /// <summary>
        /// Get the spawn rotation for this <see cref="SpawnLocation"/>.
        /// </summary>
        public Quaternion SpawnRotation
        {
            get { return spawnRotation; }
            internal set { spawnRotation = value; }
        }

        // Constructor
        internal SpawnLocation(Spawner owner, Vector3 position, Quaternion rotation)
        {
            // Set the valid flag
            isValid = true;

            // Store the creator of this info
            this.owner = owner;

            // Get spawn location
            this.spawnPosition = position;
            this.spawnRotation = rotation;
        }

        // Methods
        /// <summary>
        /// Attempts to spawn the specified object transform at this <see cref="SpawnLocation"/> location.
        /// The <see cref="Spawner"/> that owns this <see cref="SpawnLocation"/> must be unoccupied and able to spawn an object otherwise the request will fail.  
        /// </summary>
        /// <param name="toSpawn">The transform component of the object to spawn</param>
        /// <param name="checkAvailability">Should the spawn location check whether or not it is already occupied</param>
        /// <param name="applyRotation">Should the rotation be modified and to what extent. In 2D games you might not want any rotation to be applied or you may only want rotation to be applied on the Z axis</param>
        /// <returns>True if the object was spawned sucessfully or false if the <see cref="Spawner"/> is occupied by another object</returns>
        public bool Spawn(Transform toSpawn, SpawnRotationApplyMode applyRotation = SpawnRotationApplyMode.FullRotation)
        {
            // Check if the location is valid
            if (isValid == false && Application.isPlaying == true)
                return false;

            // Apply the transform
            toSpawn.position = spawnPosition;

            // Check if the rotation can be applied
            if (applyRotation == SpawnRotationApplyMode.FullRotation)
            {
                // Apply the rotation
                toSpawn.rotation = spawnRotation;
            }
            else if(applyRotation == SpawnRotationApplyMode.YRotation)
            {
                // Apply the rotation only on the Y axis
                toSpawn.rotation = toSpawn.rotation * Quaternion.AngleAxis(spawnRotation.eulerAngles.y, Vector3.up);
            }
            else if(applyRotation == SpawnRotationApplyMode.ZRotation)
            {
                // Apply the rotation only on the Z axis while keeping x and y rotation at the same values
                toSpawn.rotation = toSpawn.rotation * Quaternion.AngleAxis(spawnRotation.eulerAngles.z, Vector3.forward);
            }

            // Spawned sucessfully
            return true;
        }

        /// <summary>
        /// Synchronises the spawn location with the location of the parent spawner.
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="targetRotation"></param>
        public void Update(Vector3 targetPosition, Quaternion targetRotation)
        {
            // Update spawn location
            this.spawnPosition = targetPosition;
            this.spawnRotation = targetRotation;
        }

        public Quaternion GetSpawnRotation(SpawnRotationApplyMode applyMode)
        {
            switch(applyMode)
            {
                case SpawnRotationApplyMode.FullRotation: return spawnRotation;
                case SpawnRotationApplyMode.YRotation: return Quaternion.AngleAxis(spawnRotation.eulerAngles.y, Vector3.up);
                case SpawnRotationApplyMode.ZRotation: return Quaternion.AngleAxis(spawnRotation.eulerAngles.z, Vector3.forward);
            }

            return Quaternion.identity;
        }
    }
}