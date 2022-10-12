using System.Collections.Generic;
using UltimateSpawner.Spawning;
using UnityEngine;

namespace UltimateSpawner.Waves
{
    public class WaveState
    {
        // Private
        private WaveSpawnController controller = null;
        private Spawner targetSpawner = null;
        private SpawnableItemRef targetSpawnable = null;

        private int waveNumber = 0;
        private string waveName = "Default Wave";
        private int waveSpawnCount = 0;
        private float waveSpawnFrequency = 0;
        private float waveSpawnRandomness = 0;

        private List<WaveState> subWaves = new List<WaveState>();

        // Properties
        public WaveSpawnController Controller
        {
            get { return controller; }
        }

        public Spawner TargetSpawner
        {
            get { return targetSpawner; }
        }

        public SpawnableItemRef TargetSpawnable
        {
            get { return targetSpawnable; }
        }

        public int WaveNumber
        {
            get { return waveNumber; }
        }

        public string WaveName
        {
            get { return waveName; }
        }

        public int WaveSpawnCount
        {
            get { return waveSpawnCount; }
        }

        public float WaveSpawnFrequency
        {
            get { return waveSpawnFrequency; }
        }

        public float WaveSpawnRandomness
        {
            get { return waveSpawnRandomness; }
        }

        public bool HasSubWaves
        {
            get { return subWaves.Count > 0; }
        }

        public WaveState[] SubWaves
        {
            get { return subWaves.ToArray(); }
        }

        // Constructor
        internal WaveState(WaveSpawnController controller)
        {
            this.controller = controller;
        }

        // Methods
        internal void AddSubWave(string waveName, int spawnAmount, float spawnFrequency, float spawnRandomness, Spawner spawner = null, SpawnableItemRef itemRef = null)
        {
            // Create the sub state
            WaveState subState = new WaveState(controller);

            // Set the wave number as the same value
            subState.waveNumber = waveNumber;

            // Setup values
            SetWaveValues(subState, waveName, spawnAmount, spawnFrequency, spawnRandomness, spawner, itemRef);

            // Register the sub state
            subWaves.Add(subState);
        }

        public void AdvanceWaveCounter()
        {
            // Increment wave counter
            waveNumber++;
        }

        internal static WaveState AdvanceWave(WaveState current, string waveName, int spawnAmount, float spawnFrequency, float spawnRandomness, Spawner spawner = null, SpawnableItemRef itemRef = null)
        {
            // Create the result
            WaveState newState = new WaveState(current.controller);

            // Keep the same wave counter
            newState.waveNumber = current.waveNumber;

            // Initialize state information
            SetWaveValues(newState, waveName, spawnAmount, spawnFrequency, spawnRandomness, spawner, itemRef);

            return newState;
        }

        private static void SetWaveValues(WaveState state, string waveName, int spawnAmount, float spawnFrequency, float spawnRandomness, Spawner spawner = null, SpawnableItemRef itemRef = null)
        {
            state.waveName = waveName;
            state.waveSpawnCount = spawnAmount;
            state.waveSpawnFrequency = spawnFrequency;
            state.waveSpawnRandomness = spawnRandomness;
            state.targetSpawner = spawner;
            state.targetSpawnable = itemRef;
        }
    }
}
