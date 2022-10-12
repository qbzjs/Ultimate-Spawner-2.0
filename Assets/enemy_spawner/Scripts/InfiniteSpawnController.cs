using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace UltimateSpawner
{
    /// <summary>
    /// The stop condition method used to determine when the <see cref="InfiniteSpawnController"/> should stop spawning. 
    /// </summary>
    public enum InfiniteStopCondition
    {
        /// <summary>
        /// The controller should spawn infinitley and never stop automatically.
        /// </summary>
        DontStop,
        /// <summary>
        /// The controller should stop after a certain amount of time has passed.
        /// </summary>
        TimeElapsed,
        /// <summary>
        /// The controller should stop after a certain number of items have been spawned.
        /// </summary>
        SpawnableCount,
    }
    
    /// <summary>
    /// An <see cref="InfiniteSpawnController"/> is a versatile spawn controller that can be used for many different spawning tasks.
    /// It is intended to continuously spawn items based upon time delays and target spawned item counts to ensure that there is always atleast the desired amount of items spawned into the scene.
    /// It is also possible to specify termination conditions that will cause the controller to stop spawneing after a certain target is reached.
    /// </summary>
    [Serializable]
    public class InfiniteSpawnController : SpawnController
    {
        // Private
        private Timer stopTimer = new Timer();

        // Public
        /// <summary>
        /// The minimum number of items that should exist in the scene at any time. 
        /// </summary>
        [Tooltip("The minimum number of items that should be spawned in the scene at any time. Note that it may take some time to respawn items based upon the 'spawnDelay' values")]
        public int minimumSpawnCount = 4;

        /// <summary>
        /// The amount of time between item spawns.
        /// </summary>
        [Tooltip("The minimum amount of time that must pass before another item can spawn")]
        public float spawnDelay = 0.3f;
        
        /// <summary>
        /// The amount of randomness to add the the <see cref="spawnDelay"/>. 
        /// </summary>
        [Tooltip("The amount of randomness that should be added to the spawn delay")]
        public float spawnDelayRandomness = 0.2f;

        /// <summary>
        /// The <see cref="InfiniteStopCondition"/> used to determine when the controller should stop spawning automatically.
        /// When the stop condition is met, <see cref="OnSpawnerStopCondition"/> will be called. 
        /// </summary>
        [Tooltip("Should the controller have a stop condition or should it run forever")]
        public InfiniteStopCondition stopAfter = InfiniteStopCondition.DontStop;

        /// <summary>
        /// The amount of time that should pass before the controller stops spawning automatically.
        /// This value will only be considered when <see cref="stopAfter"/> is equal to <see cref="InfiniteStopCondition.TimeElapsed"/>.  
        /// </summary>
        [DisplayCondition("stopAfter", InfiniteStopCondition.TimeElapsed)]
        [Tooltip("The amount of time (in seconds) that must pass before stopping the controller")]
        public float stopAfterTime = 15;

        /// <summary>
        /// The number of items that can spawn before the controller stops spawning automatically.
        /// This value will only be consiered when <see cref="stopAfter"/> is equal to <see cref="InfiniteStopCondition.SpawnableCount"/>. 
        /// </summary>
        [DisplayCondition("stopAfter", InfiniteStopCondition.SpawnableCount)]
        [Tooltip("The amount of items to spawn before stopping the controller")]
        public int stopAfterSpawnedCount = 16;

        // Events
        /// <summary>
        /// Unity event that is called when the <see cref="stopAfter"/> condition for the controller has been met. 
        /// The event will not be called as a result of <see cref="SpawnController.StopSpawning"/>. 
        /// </summary>
        [HideInInspector]
        [Tooltip("Called when the controller will stop spawning because it has reached its stop condition")]
        public UnityEvent OnSpawnerStopCondition;

        // Methods
        /// <summary>
        /// The main spawn routine for this controller.
        /// </summary>
        /// <returns>Enumerator routine</returns>
        public override IEnumerator SpawnRoutine()
        {
            // Loop forever or until we stop spawning
            while(IsSpawning == true)
            {
                // Check for stop condition
                if(StopConditionReached() == true)
                {
                    // Stop this controller
                    StopSpawning();

                    // Trigger end event
                    OnSpawnerStopCondition.Invoke();

                    // Exit routine
                    yield break;                    
                }


                // Minimum item spawning
                {
                    // Check if we need to spawn some items
                    if (SpawnedItemCount < minimumSpawnCount)
                    {
                        // Wait for time to pass
                        yield return WaitForSecondsNonAlloc.WaitFor(spawnDelay);

                        // The condition could have been reached while waiting
                        if (StopConditionReached() == true)
                            continue;

                        // Get the item spawn routine
                        yield return StartCoroutine(ItemSpawnRoutine());
                    }
                }


                // Chance item spawning
                {
                    // Wait for time to pass
                    yield return WaitForSecondsNonAlloc.WaitFor(spawnDelay + Random.Range(0, spawnDelayRandomness));
                    
                    // Calculate the chance of spawning
                    int target = maximumSpawnCount - minimumSpawnCount;

                    if (Random.Range(0, target + 1) > (target / 2))
                    {
                        // The condition could have been reached while waiting
                        if (StopConditionReached() == true)
                            continue;

                        // Get the item spawn routine
                        yield return StartCoroutine(ItemSpawnRoutine());
                    }
                }
            }
        }

        /// <summary>
        /// Resets the state of this <see cref="InfiniteSpawnController"/>. 
        /// </summary>
        public override void ResetState()
        {
            stopTimer.Reset();
        }

        private bool StopConditionReached()
        {
            switch(stopAfter)
            {
                // No stop condition - contiue spawning forever
                default:
                case InfiniteStopCondition.DontStop:
                    break;

                case InfiniteStopCondition.TimeElapsed:
                    {
                        // Check if the stop time has passed
                        if (stopTimer.HasElapsed(stopAfterTime) == true)
                        {
                            // The condition is reached
                            return true;
                        }
                        break;
                    }

                case InfiniteStopCondition.SpawnableCount:
                    {
                        if(TotalSpawnedItemCount >= stopAfterSpawnedCount)
                        {
                            // The condition is met
                            return true;
                        }
                        break;
                    }
            }

            // Default - continue spawning
            return false;
        }
    }
}
