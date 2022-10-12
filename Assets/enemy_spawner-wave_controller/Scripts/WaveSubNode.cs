using System;
using System.Collections;
using UltimateSpawner.Spawning;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UltimateSpawner.Waves
{
    [Serializable]
    [NodeTint(200, 180, 200)]
    [CreateNodeMenu("Waves/Sub-Wave")]
    public class WaveSubNode : WaveSpawnNode
    {
        // Public   
        [Input(ShowBackingValue.Never)]
        public WaveSubNode In;

        // Properties
        public override string NodeDisplayName
        {
            get { return "Sub-Wave"; }
        }

        // Methods
        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            // Enter sub wave node
            controller.CurrentNode = this;
            controller.CurrentNodeType = WaveSpawnController.WaveNodeType.SubWave;

            yield break;
        }
    }
}
