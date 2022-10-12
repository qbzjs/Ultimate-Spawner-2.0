using System;
using System.Collections;
using UnityEngine;
using XNode;
using Random = UnityEngine.Random;

namespace UltimateSpawner.Waves
{
    [Serializable]
    [CreateNodeMenu("Waves/Wave Randomizer")]
    public class WaveRandomizer : WaveNode
    {
        // Public
        public const string intPortName = "Int";
        public const string floatPortName = "Float";

        [Output(ShowBackingValue.Never)]
        public int Int;

        [Output(ShowBackingValue.Never)]
        public float Float;

        public float minValue = 1;

        public float maxValue = 3;

        // Properties
        public override int NodeWidth
        {
            get { return base.NodeWidth - 100; }
        }

        public override int NodeLabelWidth
        {
            get { return base.NodeLabelWidth - 50; }
        }

        public override string NodeDisplayName
        {
            get { return "Wave Randomizer"; }
        }

        // Methods
        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(NodePort port)
        {
            if(port.fieldName == intPortName)
            {
                // Get a random rounded value
                return Mathf.RoundToInt(Random.Range(minValue, maxValue));
            }

            // Check for float field
            if(port.fieldName == floatPortName)
            {
                // Get a random value
                return Random.Range(minValue, maxValue);
            }

            // Default to base method
            return base.GetValue(port);
        }

        public override bool CanConnectTo(NodePort from, NodePort to)
        {
            // Require same types
            return from.ValueType == to.ValueType;
        }
    }
}
