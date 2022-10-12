using System;
using System.Collections;
using UnityEngine;

namespace UltimateSpawner
{
    /// <summary>
    /// A simple lightweight timer that can measue elapsed time.
    /// Used by <see cref="SpawnController"/> to determine when time conditions have been met.
    /// </summary>
    public sealed class Timer
    {
        // Private
        private float startTime = 0;

        // Properties
        /// <summary>
        /// Get the time in seconds that this <see cref="Timer"/> started measuring time. 
        /// </summary>
        public float StartTime
        {
            get { return startTime; }
        }

        /// <summary>
        /// Get the amount of time in seconds that have passed since this <see cref="Timer"/> started measuring time. 
        /// </summary>
        public float ElapsedTime
        {
            get { return Time.time - startTime; }
        }

        /// <summary>
        /// Get the amount of time in milliseconds that have passed since this <see cref="Timer"/> started measuring time. 
        /// </summary>
        public float ElapsedMilliseconds
        {
            get { return ElapsedTime * 1000f; }
        }

        // Methods
        /// <summary>
        /// Causes the <see cref="Timer"/> to restart timing causing <see cref="ElapsedTime"/> to become 0.  
        /// </summary>
        public void Reset()
        {
            // Restart the timer
            startTime = Time.time;
        }

        /// <summary>
        /// Check whether the specified amount of time in seconds has passed since this <see cref="Timer"/> started measuring time. 
        /// </summary>
        /// <param name="amount">The amount of time in seconds</param>
        /// <returns>True if the specified time amount has elapsed or false if it has not</returns>
        public bool HasElapsed(float amount)
        {
            // Call through
            return HasElapsed(amount, false);
        }

        /// <summary>
        /// Check whether the specified amount of time in seconds has passed since this <see cref="Timer"/> started measuring time. 
        /// </summary>
        /// <param name="amount">The amount of time in seconds</param>
        /// <param name="autoReset">Should the timer reset its self once the specified time has elapsed. This may cause the method to return true for a single frame only</param>
        /// <returns>True if the specified time amount has elasped or false if it has not</returns>
        public bool HasElapsed(float amount, bool autoReset)
        {
            // Check if the amount of time has passed
            if (ElapsedTime > amount)
            {
                // Check if we should auto rest the timer
                if (autoReset == true)
                    Reset();

                // The time is up
                return true;
            }

            // Time has not passed yet
            return false;
        }
    }
}
