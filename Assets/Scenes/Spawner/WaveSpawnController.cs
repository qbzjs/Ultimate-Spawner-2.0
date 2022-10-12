using System;
using System.Collections;
using UnityEngine;
using UltimateSpawner.Waves;
using UnityEngine.Events;
using UltimateSpawner.Spawning;
using UltimateSpawner.Waves.Parameters;

namespace UltimateSpawner
{
    [Serializable]
    public class UnityStringEvent : UnityEvent<string> { }

    public class WaveSpawnController : SpawnController
    {
        // Types
        public enum WaveNodeType
        {
            None = 0,
            Start,
            Condition,
            Delay,
            Event,
            Loop,
            Wave,
            SubWave,
        }

        // Private
        private WaveNode currentNode = null;
        private int currentWaveSpawnedItemCount = 0;
        private int currentWaveDestroyedItemCount = 0;
        private WaveNodeType currentNodeType = WaveNodeType.None;

        // Protected
        protected WaveState currentWaveState = null;

        // Public
        [Tooltip("The wave configuration asset to use for this wave spawn controller", order = 0)]
        public WaveConfiguration waveConfig;

        // Events
        [HideInInspector]
        public UnityEvent OnWaveStarted;

        [HideInInspector]
        public UnityEvent OnWaveEnded;

        [HideInInspector]
        public UnityStringEvent OnWaveCustomEvent;
        
        // Properties
        public WaveState CurrentState
        {
            get { return currentWaveState; }
        }

        public int CurrentWave
        {
            get 
            {
                if (currentWaveState == null)
                    return 0;

                return currentWaveState.WaveNumber; 
            }
        }

        public int TotalWavesCount
        {
            get
            {
                if (waveConfig == null)
                    return -1;

                return waveConfig.GetConnectedNodeCountOfType<WaveMasterNode>();
            }
        }

        public int CurrentWaveSpawnedItemCount
        {
            get { return currentWaveSpawnedItemCount; }
        }

        public int CurrentWaveDestroyedItemCount
        {
            get { return currentWaveDestroyedItemCount; }
        }

        public WaveNode CurrentNode
        {
            get { return currentNode; }
            internal set { currentNode = value; }
        }

        public WaveNodeType CurrentNodeType
        {
            get { return currentNodeType; }
            internal set { currentNodeType = value; }
        }

        public WaveParameterSet Parameters
        {
            get
            {
                // Check for no config
                if (waveConfig == null)
                    return null;

                // Try to get parameter node
                WaveParameterNode parameterNode = waveConfig.GetParameterNode();

                // Check for error
                if (parameterNode == null)
                    return null;

                // Get parmaeters
                return parameterNode.GetRuntimeParameters();
            }
        }

        // Constructor
        public WaveSpawnController()
        {
            // Create the wave state
            this.currentWaveState = new WaveState(this);
        }
        
        // Methods
        public void Log(string msg)
        {
            Debug.Log("Controller Log: " + msg);
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public override void Awake()
        {
            // Be sure to call the base method
            base.Awake();

            // Make sure we have a wave configuration
            if(waveConfig == null)
            {
                enabled = false;
                throw new MissingReferenceException(string.Format("Spawn controller '{0}' requires a 'WaveConfigiration' to be assigned", GetType().Name));
            }

            // Generate the config graph
            RegenerateWaveGraph();
        }

        public void RegenerateWaveGraph()
        {
            foreach(WaveNode node in waveConfig.nodes)
            {
                node.OnGenerateWaveSession();
            }
        }

        public void StartWave(int waveIndex)
        {
            // Get the starting node
            currentNode = waveConfig.GetStartNode();

            int waveCounter = 0;

            while(currentNode != null)
            {
                // Check for special nodes
                if(currentNode is WaveMasterNode)
                {
                    // Check for alternate end condition
                    if (waveCounter == waveIndex)
                        break;

                    // Increment index
                    waveCounter++;
                }

                // Move to the next node
                currentNode = currentNode.GetConnectedOutNode();
            }

            // Check for valid node
            if (currentNode != null)
                StartWaveNode(currentNode);
        }

        public void EndWave() { }

        public void RestartWave() { }

        public void NextWave() { }

        public void PreviousWave() { }

        

        public override IEnumerator SpawnRoutine()
        {
            // Make sure a valid config is assgined
            if (waveConfig == null)
            {
                // Dont continue
                StopSpawning();
                yield break;
            }


            // Setup starting node
            if(currentNode == null)
            {
                // Get the starting node
                WaveStartNode startNode = waveConfig.GetStartNode();

                // Check for error
                if(startNode == null)
                {
                    Debug.LogErrorFormat("Wave config '{0}' does not have a start node", waveConfig);

                    // Dont continue
                    StopSpawning();
                    yield break;
                }

                // get the first connected node
                currentNode = startNode;
                currentNodeType = WaveNodeType.Start;

                // Check for no connection
                if (currentNode == null)
                    yield break;
            }

            // Evaluate the selected node
            yield return StartCoroutine(currentNode.Evaluate(this));

            // Trigger controller end
            OnEnd.Invoke();

            //// Get the starting node
            //WaveStartNode startNode = waveConfig.GetStartNode();

            //// Get the connected node
            //WaveNode current = startNode;

            //while (current.GetConnectedNode() != null)
            //{
            //    // Get next node - The first node is a start node and has no implementation
            //    current = current.GetConnectedNode();
                
            //    // Get the routine
            //    yield return StartCoroutine(current.Evaluate(this));

            //    // Wait 1 frame
            //    yield return null;
            //}

            //do
            //{
            //    // Run the routine
            //    yield return StartCoroutine(currentNode.Evaluate(this));
            //}
            //while ((currentNode = currentNode.GetConnectedOutNodes()) != null);
        }

        public override void ResetState()
        {
            // Call base method
            base.ResetState();

            currentWaveSpawnedItemCount = 0;
            currentWaveDestroyedItemCount = 0;
        }

        public virtual WaveState OnAdvanceWave(WaveState lastWave, WaveState newWave)
        {
            // Advance to the next wave
            return newWave;
        }

        internal void SetNextWaveState(WaveState state)
        {
            // Call the virtual method
            currentWaveState = OnAdvanceWave(currentWaveState, state);

            // Trigger reset
            OnWillEnterNewWave();
        }

        private void StartWaveNode(WaveNode node)
        {
            // Check for null
            if (node == null)
                return;

            // Assign the node
            currentNode = node;

            // Start spawning
            if(IsSpawning == false)
            {
                // Start spawning
                StartSpawning();
            }
        }

        private void OnWillEnterNewWave()
        {
            currentWaveSpawnedItemCount = 0;
            currentWaveDestroyedItemCount = 0;
            //currentNode = null;
            //currentWaveState = null;
        }

        protected override void OnControllerSpawnedItem(Transform item)
        {
            currentWaveSpawnedItemCount++;
        }

        protected override void OnControllerDestroyedItem(Transform item)
        {
            currentWaveDestroyedItemCount++;
        }
    }
}
