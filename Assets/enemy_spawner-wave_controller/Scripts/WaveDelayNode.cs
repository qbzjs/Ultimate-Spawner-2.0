using System;
using System.Collections;
using UnityEngine;
using XNode;

namespace UltimateSpawner.Waves
{
    [Serializable]
    [NodeTint(248, 248, 100)]
    [CreateNodeMenu("Waves/Wave Delay")]
    public sealed class WaveDelayNode : WaveNode
    {
        // Public
        [Input(ShowBackingValue.Never)]
        public WaveNode In;

        [Output(ShowBackingValue.Never)]
        public WaveNode Out;

        [Tooltip("The amount of time in seconds to delay execution")]
        public float delay = 1f;

        // Properties
        public override int NodeWidth
        {
            get { return 180; }
        }

        public override int NodeLabelWidth
        {
            get { return 70; }
        }

        public override string NodeDisplayName
        {
            get { return "Wave Delay"; }
        }

        // Methods
        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            // Enter delay node
            controller.CurrentNode = this;
            controller.CurrentNodeType = WaveSpawnController.WaveNodeType.Delay;

            // Get the wait coroutine
            yield return WaitForSecondsNonAlloc.WaitFor(delay);

            if (IsConnectedOutNode() == true)
            {
                // Evaluate all connected nodes
                yield return controller.StartCoroutine(EvaluateConnectedOutNode(controller));
            }
        }
    }
}
