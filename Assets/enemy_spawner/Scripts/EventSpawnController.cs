using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateSpawner
{
    /// <summary>
    /// A MonoBehaviour based spawn controller that uses Unity events to issue item spawn requests.
    /// You can specify which unity event will trigger an item spawn request using the <see cref="SpawnEvent"/> enumeration.
    /// </summary>
    [Serializable]
    public class EventSpawnController : SpawnController
    {
        // Types
        /// <summary>
        /// Spawn event methods that can cause the controller to trigger an item spawn request.
        /// </summary>
        public enum SpawnEvent
        {
            /// <summary>
            /// Trigger a spawn request when the Unity 'Awake' method is called.
            /// </summary>
            Awake,
            /// <summary>
            /// Trigger a spawn request when the Unity 'Start' method is called.
            /// </summary>
            Start,
            /// <summary>
            /// Trigger a spawn request when the Unity 'OnEnable' method is called.
            /// </summary>
            OnEnable,
            /// <summary>
            /// Trigger a spawn request when the Unity 'OnDisable' method is called.
            /// </summary>
            OnDisable,
            /// <summary>
            /// Trigger a spawn request when the Unity 'OnDisable' method is called.
            /// </summary>
            OnDestroy,
        }

        // Private
        private int spawnRequests = 0;

        // Public
        /// <summary>
        /// The <see cref="MonoBehaviour"/> event method used to trigger the item spawn request.
        /// </summary>
        public SpawnEvent spawnEvent = SpawnEvent.Start;
        /// <summary>
        /// The amount of items to spawn when the behaviour event is triggered.
        /// </summary>
        public int spawnAmount = 1;
        /// <summary>
        /// The amount of time to wait before issuing all item spawn requests. Thre will be no waiting between item spawn requests, only once the initial behaviour event has fired.
        /// </summary>
        public float spawnDelay = 0f;

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
        /// Called by Unity. Will trigger an item spawn if <see cref="spawnEvent"/> is set to <see cref="SpawnEvent.Awake"/>.
        /// </summary>
        public override void Awake()
        {
            // Call base
            base.Awake();

            // Spawn an item
            if (spawnEvent == SpawnEvent.Awake)
                TriggerItemSpawn();
        }

        /// <summary>
        /// Called by Unity. Will trigger an item spawn if <see cref="spawnEvent"/> is set to <see cref="SpawnEvent.Start"/>.
        /// </summary>
        public override void Start()
        {
            // Call base
            base.Start();

            // Spawn an item
            if (spawnEvent == SpawnEvent.Start)
                TriggerItemSpawn();
        }

        /// <summary>
        /// Called by Unity. Will trigger an item spawn if <see cref="spawnEvent"/> is set to <see cref="SpawnEvent.OnEnable"/>.
        /// </summary>
        public virtual void OnEnable()
        {
            // Spawn an item
            if (spawnEvent == SpawnEvent.OnEnable)
                TriggerItemSpawn();
        }

        /// <summary>
        /// Called by Unity. Will trigger an item spawn if <see cref="spawnEvent"/> is set to <see cref="SpawnEvent.OnDisable"/>.
        /// </summary>
        public virtual void OnDisable()
        {
            // Spawn an item
            if (spawnEvent == SpawnEvent.OnDisable)
                TriggerItemSpawn();
        }

        /// <summary>
        /// Called by Unity. Will trigger an item spawn if <see cref="spawnEvent"/> is set to <see cref="SpawnEvent.OnDestroy"/>.
        /// </summary>
        public override void OnDestroy()
        {
            // Call base
            base.OnDestroy();

            // Spawn an item
            if (spawnEvent == SpawnEvent.OnDestroy)
                TriggerItemSpawn();
        }

        /// <summary>
        /// Trigger an item spawn request to be sent to the active spawner.
        /// <see cref="spawnAmount"/> must be a number greater than '0' or this method will do nothing.
        /// </summary>
        public void TriggerItemSpawn()
        {
            // Check for invalid spawn amounts
            if (spawnAmount <= 0)
                return;

            // Post spawn request
            spawnRequests += spawnAmount;

            // Trigger event
            OnSpawnerTrigger.Invoke();
        }

        /// <summary>
        /// The main spawn routine for the trigger controller.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator SpawnRoutine()
        {
            // Loop while spawner is running
            while (IsSpawning == true)
            {
                // Check for any spawn requests
                if (spawnRequests > 0)
                {
                    // Wait for delay
                    if (spawnDelay > 0)
                        yield return WaitForSecondsNonAlloc.WaitFor(spawnDelay);

                    // Spawn specified amount
                    for (int i = 0; i < spawnRequests; i++)
                    {
                        // Spawn the item immediatley
                        yield return StartCoroutine(ItemSpawnRoutine());
                    }

                    // Reset request counter
                    spawnRequests = 0;
                }
                else
                {
                    // Only stop when atleast 1 item has spawned
                    if (TotalSpawnedItemCount > 0)
                        StopSpawning();
                }

                // Wait a frame
                yield return null;
            }
        }
    }
}
