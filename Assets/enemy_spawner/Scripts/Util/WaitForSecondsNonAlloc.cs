using System.Collections;
using UnityEngine;

namespace UltimateSpawner
{
    /// <summary>
    /// A reimplmentation of the Unity WaitForSeconds type which aims to avoid heap allocations by allocating on the stack.
    /// </summary>
    public struct WaitForSecondsNonAlloc : IEnumerator
    {
        // Private
        private bool hasStarted;
        private float startTime;
        private float waitTime;

        // Properties
        /// <summary>
        /// <see cref="IEnumerable"/> implementation.
        /// </summary>
        public object Current
        {
            get { return null; }
        }

        /// <summary>
        /// The amound of time in seconds that this wait object represents.
        /// </summary>
        public float WaitTime
        {
            get { return waitTime; }
        }

        // Constructor
        /// <summary>
        /// Create a new wait object with the specified amount of time in seconds.
        /// </summary>
        /// <param name="seconds">The amount of time to wait in seconds</param>
        public WaitForSecondsNonAlloc(float seconds)
        {
            hasStarted = false;
            startTime = Time.time;
            waitTime = seconds;
        }

        // Methods
        /// <summary>
        /// <see cref="IEnumerable"/> implementation.
        /// Check whether the target wait time has been reached by the application and will cause the iterator to wait a frame if not.
        /// </summary>
        public bool MoveNext()
        {
            // Check if we have started
            if(hasStarted == false)
            {
                // Set start time
                startTime = Time.time;
                hasStarted = true;
            }

            // Check for elapsed time
            return !(Time.time > (startTime + waitTime));
        }

        /// <summary>
        /// <see cref="IEnumerable"/> implementation.
        /// Resets the wait object to a time of '0' seconds.
        /// </summary>
        public void Reset()
        {
            hasStarted = false;
            startTime = 0;
        }

        /// <summary>
        /// Create a new wait object from the specified amount of time.
        /// </summary>
        /// <param name="seconds">The amount of time to wait in seconds</param>
        /// <returns>An yieldable wait object for the specified time</returns>
        public static IEnumerator WaitFor(float seconds)
        {
            return new WaitForSecondsNonAlloc(seconds);
        }
    }
}
