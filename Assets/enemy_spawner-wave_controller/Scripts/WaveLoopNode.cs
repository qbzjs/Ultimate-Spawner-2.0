using System;
using System.Collections;
using UnityEngine;
using XNode;

namespace UltimateSpawner.Waves
{
    public enum WaveLoopMode
    {
        Fixed,
        Infinite,
        Random,
    }

    [Serializable]
    [NodeTint(240, 140, 255)]
    [CreateNodeMenu("Waves/Wave Loop")]
    public class WaveLoopNode : WaveNode
    {
        // Internal
        internal const string infiniteLoopField = "infiniteLoop";
        internal const string loopCountField = "loopCount";

        // Private
        private int targetLoopCount = 0;
        private int loopCounter = 0;

        // Public
        public const string loopChainPortName = "LoopChain";

        [Input(ShowBackingValue.Never)]
        public WaveNode In;

        [Output(ShowBackingValue.Never)]
        public WaveNode Out;

        [Tooltip("The wave node chain that should be run at every loop cycle")]
        [Output]
        public WaveNode LoopChain;

        public bool infiniteLoop = false;
        
        [Input]
        [Tooltip("The amount of times the loop should repeat")]
        [DisplayCondition(infiniteLoopField, false)]
        public int loopCount = 3;

        // Properties
        public override int NodeLabelWidth
        {
            get { return 90; }
        }

        public override string NodeDisplayName
        {
            get { return "Wave Loop"; }
        }

        public bool ConditionMet
        {
            get
            {
                // Condition is never met
                if (infiniteLoop == true)
                    return false;

                // Check for loop count
                return loopCounter >= targetLoopCount;
            }
        }

        // Methods
        public override void OnGenerateWaveSession()
        {
            // Check for non-infinite loop
            if (infiniteLoop == false)
            {
                targetLoopCount = GetInputValue<int>(loopCountField, loopCount);
            }
        }

        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            // Enter loop node
            controller.CurrentNode = this;
            controller.CurrentNodeType = WaveSpawnController.WaveNodeType.Loop;

            if (IsConnectedNode(loopChainPortName) == true)
            {
                // Create the loop
                for (loopCounter = 0; ConditionMet == false; loopCounter++)
                {
                    // Re-enter loop node after chain cycle has finished
                    controller.CurrentNode = this;
                    controller.CurrentNodeType = WaveSpawnController.WaveNodeType.Loop;

                    // Evaluate the loop chain until the end
                    yield return controller.StartCoroutine(EvaluateConnectedNode(controller, loopChainPortName));
                }
            }

            if (IsConnectedOutNode() == true)
            {
                // Evaluate all connected nodes
                yield return controller.StartCoroutine(EvaluateConnectedOutNode(controller));
            }
        }
    }
}
