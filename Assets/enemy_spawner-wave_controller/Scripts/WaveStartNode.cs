using System;
using System.Collections;
using UnityEngine;
using XNode;

namespace UltimateSpawner.Waves
{
    [Serializable]
    [NodeTint(180, 255, 180)]
    [CreateNodeMenu(null)]              // Hide in context menu
    public sealed class WaveStartNode : WaveNode
    {
        // Public
        [Output(ShowBackingValue.Never)]
        public WaveNode Out;

        // Properties
        public override int NodeWidth
        {
            get { return 140; }
        }

        public override string NodeDisplayName
        {
            get { return "Wave Start"; }
        }

        // Constructor
        private WaveStartNode() { }

        // Methods
        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            // Enter start node
            controller.CurrentNode = this;
            controller.CurrentNodeType = WaveSpawnController.WaveNodeType.Start;

            if (IsConnectedOutNode() == true)
            {
                // Evaluate all connected nodes
                yield return controller.StartCoroutine(EvaluateConnectedOutNode(controller));
            }
            else
                Debug.LogWarningFormat("The wave controller '{0}' has an empty wave configuration. The controller will do nothing", controller);
        }
    }
}
