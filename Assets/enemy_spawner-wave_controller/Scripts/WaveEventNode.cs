using System;
using System.Collections;
using UnityEngine;

namespace UltimateSpawner.Waves
{
    [Serializable]
    [CreateNodeMenu("Waves/Wave Event")]
    public class WaveEventNode : WaveNode
    {
        // Public
        [Input(ShowBackingValue.Never)]
        public WaveNode In;

        [Output(ShowBackingValue.Never)]
        public WaveNode Out;

        [Tooltip("This string value will be passed to the 'OnWaveCustomEvent' of the executing 'WaveSpawnController'")]
        public string eventArgs;

        // Properties
        public override int NodeWidth
        {
            get { return 200; }
        }

        public override int NodeLabelWidth
        {
            get { return 80; }
        }

        public override string NodeDisplayName
        {
            get { return "Wave Event"; }
        }

        // Methods
        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            // Enter event node
            controller.CurrentNode = this;
            controller.CurrentNodeType = WaveSpawnController.WaveNodeType.Event;

            try
            {
                // Call the use event
                controller.OnWaveCustomEvent.Invoke(eventArgs);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }

            if (IsConnectedOutNode() == true)
            {
                // Evaluate all connected nodes
                yield return controller.StartCoroutine(EvaluateConnectedOutNode(controller));
            }
        }
    }
}
