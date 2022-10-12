using System;
using System.Collections;
using System.Collections.Generic;
using UltimateSpawner.Spawning;
using UnityEngine;
using XNode;
using Random = UnityEngine.Random;

namespace UltimateSpawner.Waves
{
    public enum WaveContinueMode
    {
        Never = 0,
        Instant,
        WhenAllDead,
        WhenAllSpawned,
    }

    public enum WaveCounterMode
    {
        OnWaveStart,
        OnWaveEnd,
    }

    [Serializable]
    [NodeTint(200, 200, 240)]
    [CreateNodeMenu("Waves/Wave")]
    public sealed class WaveMasterNode : WaveSpawnNode
    {
        // Private
        private HashSet<IEnumerator> runningSubWaves = new HashSet<IEnumerator>();

        // Public
        public const string subWavesPortName = "SubWaves";

        [Input(ShowBackingValue.Never)]
        public WaveNode In;

        [Output]
        public WaveNode Out;

        [Output]
        public WaveSubNode SubWaves;

        public bool advanceWaveCounter = true;

        [DisplayCondition("advanceWaveCounter", true)]
        public WaveCounterMode waveCounterMode = WaveCounterMode.OnWaveStart;

        public WaveContinueMode continueMode = WaveContinueMode.WhenAllDead;

        [Tooltip("When enabled, the spawn controller will wait for the item spawn request to be completed before continuing. This make take some time as the target spawn point may be occupied")]
        public bool waitForItemSpawn = true;

        // Properties
        public override string NodeDisplayName
        {
            get { return "Wave"; }
        }

        // Methods
        public override bool CanConnectTo(NodePort from, NodePort to)
        {
            if(from.fieldName == subWavesPortName)
            {
                if(to.ValueType == typeof(WaveSubNode))
                {
                    return true;
                }
                return false;
            }

            return base.CanConnectTo(from, to);
        }

        public override bool CanHaveMultipleConnections(NodePort from)
        {
            if(from.fieldName == subWavesPortName)
            {
                return true;
            }

            return base.CanHaveMultipleConnections(from);
        }

        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            // Get the previous wave state
            WaveState previousState = controller.CurrentState;

            // Enter wave node
            controller.CurrentNode = this;
            controller.CurrentNodeType = WaveSpawnController.WaveNodeType.Wave;

            // Try to find the spawner
            Spawner targetSpawner = ResolveTargetSpawner(controller);

            // Try to find the spawnable item
            SpawnableItemRef targetItem = ResolveTargetSpawnableItem(controller);

            if (targetItem != null)
                UltimateSpawning.Log("using spawnable: " + targetItem.Name);

            // Get spawn count
            targetSpawnCount = GetInputValue(spawnCountField, spawnCount);

            // Check for multiplier mode
            if (waveMode == WaveSetupMode.Multiplier && previousState != null)
                targetSpawnCount = (int)(previousState.WaveSpawnCount * spawnCountMultiplier);

            targetSpawnFrequency = GetInputValue(spawnFrequencyField, spawnFrequency);
            targetSpawnRandomness = GetInputValue(spawnRandomnessField, spawnRandomness);


            // Move to the next wave state
            WaveState nextWave = WaveState.AdvanceWave(controller.CurrentState,
                name,
                targetSpawnCount,
                targetSpawnFrequency,
                targetSpawnRandomness,
                targetSpawner,
                targetItem);

            // Advance the wave counter
            if ((advanceWaveCounter == true && waveCounterMode == WaveCounterMode.OnWaveStart) || nextWave.WaveNumber == 0)
                nextWave.AdvanceWaveCounter();

            // Infom the controller of the wave change
            controller.SetNextWaveState(nextWave);


            // Invoke start event
            controller.OnWaveStarted.Invoke();


            // Start all subroutines
            //foreach(WaveSubNode subNode in GetConnectedSubWaves())
            //{
            //    // Get the coroutine enumerator
            //    IEnumerator routine = subNode.Evaluate(controller);

            //    // Run the routine until completion - dont yield because we need to run spawning logic in this routine at the same time
            //    EvaluateSubRoutine(controller, routine);
            //}


            List<SpawnedItem> spawnedItems = new List<SpawnedItem>();

            for (int i = 0; i < targetSpawnCount; i++)
            {
                // Wait a random amount of time
                float delay = targetSpawnFrequency + Random.Range(0, targetSpawnRandomness);

                // Wait for time to pass
                yield return WaitForSecondsNonAlloc.WaitFor(delay);
                                


                // Try to spawn and item
                IEnumerator itemSpawnRoutine = controller.ItemSpawnRoutine(targetSpawner, targetItem, spawnedItems);

                if (waitForItemSpawn == true)
                {
                    // Wait for spawn to complete
                    yield return controller.StartCoroutine(itemSpawnRoutine);
                }
                else
                {
                    // Fire and forget
                    controller.StartCoroutine(itemSpawnRoutine);
                }

                // Always wait a frame
                yield return null;
            }


            // Make sure all sub waves have ended before exiting the master wave
            while (runningSubWaves.Count > 0)
                yield return null;

            // Wait for all dead
            switch(continueMode)
            {
                case WaveContinueMode.WhenAllSpawned:
                    {
                        // Wait for all items to be fully spawned
                        while (controller.IsTryingToSpawn == true)
                            yield return null;

                        break;
                    }

                case WaveContinueMode.WhenAllDead:
                    {
                        // Wait forever
                        while (true)
                        {
                            int deadCount = 0;

                            // Check for any alive items
                            for (int i = 0; i < spawnedItems.Count; i++)
                                if (spawnedItems[i].IsAlive() == false)
                                    deadCount++;

                            // Check for all dead items
                            if (deadCount >= targetSpawnCount)
                                break;

                            // Wait a frame
                            yield return null;
                        }
                        break;
                    }
            }


            // Invoke end event
            controller.OnWaveEnded.Invoke();

            // Advance the wave counter
            if (advanceWaveCounter == true && waveCounterMode == WaveCounterMode.OnWaveEnd)
                nextWave.AdvanceWaveCounter();

            // Do not continue
            if (continueMode == WaveContinueMode.Never)
                yield break;

            if (IsConnectedOutNode() == true)
            {
                // Evaluate all connected nodes
                yield return controller.StartCoroutine(EvaluateConnectedOutNode(controller));
            }
        }

        private IEnumerator EvaluateSubRoutine(WaveSpawnController controller, IEnumerator routine)
        {
            // Register the routine
            runningSubWaves.Add(routine);

            // Wait for completion
            yield return controller.StartCoroutine(routine);

            // Unregister the routine
            runningSubWaves.Remove(routine);
        }

        private WaveSubNode[] GetConnectedSubWaves()
        {
            // Get the output port
            NodePort port = GetOutputPort(subWavesPortName);

            // Check for error
            if (port == null)
                return new WaveSubNode[0];

            // Check for connection
            if (port.IsConnected == false)
                return new WaveSubNode[0];

            List<WaveSubNode> connectedSubNodes = new List<WaveSubNode>();

            // Get all connections
            for(int i = 0; i < port.ConnectionCount; i++)
            {
                // Get the connected port
                NodePort connected = port.GetConnection(i);

                // Check for error
                if (connected == null)
                    continue;

                // Get the node
                WaveSubNode node = connected.node as WaveSubNode;

                // Register the node
                if(node != null)
                    connectedSubNodes.Add(node);
            }

            // Get the result array
            return connectedSubNodes.ToArray();
        }
    }
}
