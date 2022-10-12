using System;
using System.Collections;
using UnityEngine;

namespace UltimateSpawner.Waves
{
    [Serializable]
    [CreateNodeMenu("Waves/Wave Wait Condition")]
    public sealed class WaveWaitCondition : WaveNode
    {
        // Public
        [Input(ShowBackingValue.Never)]
        public WaveNode In;

        [Output(ShowBackingValue.Never)]
        public WaveNode Out;

        public WaveConditionType condition = WaveConditionType.ItemsSpawned;

        [DisplayConditionMethod("IsParameter")]
        public string parameterName;

        public WaveConditionOperator operand = WaveConditionOperator.Equals;

        [Input]
        [DisplayConditionMethod("IsNotFloatOrBoolParameter")]
        public int value = 0;

        [Input]
        [DisplayCondition("condition", WaveConditionType.FloatParameter)]
        public float valueFloat = 0;

        [DisplayCondition("condition", WaveConditionType.BoolParameter)]
        public bool valueBool = false;

        // Methods
        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            // Wait for condition
            while(true)
            {
                double inputValue = WaveConditionNode.GetInputValue(controller, condition, parameterName);

                // Get graph values
                int value = GetInputValue<int>(WaveConditionNode.valuePortName);
                float floatValue = GetInputValue<float>("valueFloat");
                bool boolValue = valueBool;

                double conditionValue = value;

                if(condition == WaveConditionType.FloatParameter)
                {
                    conditionValue = floatValue;
                }
                else if(condition == WaveConditionType.BoolParameter)
                {
                    conditionValue = (boolValue == true) ? 1 : 0;
                }
                
                // Check for condition
                if (WaveConditionNode.IsConditionMet(inputValue, conditionValue, operand, IsParameter()) == true)
                    break;

                // Wait a frame
                yield return null;
            }

            // Evaluate out
            yield return controller.StartCoroutine(EvaluateConnectedOutNode(controller));
        }

        private bool IsParameter()
        {
            return condition == WaveConditionType.IntParameter || condition == WaveConditionType.FloatParameter || condition == WaveConditionType.BoolParameter;
        }

        private bool IsNotFloatOrBoolParameter()
        {
            return condition != WaveConditionType.FloatParameter && condition != WaveConditionType.BoolParameter;
        }
    }
}
