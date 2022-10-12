using System;
using System.Collections;
using UnityEngine;

namespace UltimateSpawner.Waves
{
    public enum WaveConditionType
    {
        ItemsSpawnedTotal,
        ItemsSpawned,
        ItemsAlive,
        ItemsDestroyed,
        ItemsDestroyedTotal,
        CurrentWaveNumber,
        IntParameter,
        FloatParameter,
        BoolParameter,
    }

    public enum WaveConditionOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Modulus,
        InverseModulus,
    }

    [Serializable]
    [CreateNodeMenu("Waves/Wave Condition")]
    public class WaveConditionNode : WaveNode
    {
        // Public
        public const string truePortName = "True";
        public const string falsePortName = "False";
        public const string valuePortName = "value";

        [Input(ShowBackingValue.Never)]
        public WaveNode In;

        [Output(ShowBackingValue.Never)]
        public WaveNode True;

        [Output(ShowBackingValue.Never)]
        public WaveNode False;

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

        // Properties
        public override int NodeWidth
        {
            get { return base.NodeWidth; }
        }

        public override int NodeLabelWidth
        {
            get { return base.NodeLabelWidth - 10; }
        }

        public override string NodeDisplayName
        {
            get { return "Wave Condition"; }
        }

        // Methods
        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            // Get the input value from the controller
            double inputValue = GetInputValue(controller, condition, parameterName);

            // Get graph values
            int value = GetInputValue<int>(WaveConditionNode.valuePortName);
            float floatValue = GetInputValue<float>("valueFloat");
            bool boolValue = valueBool;

            double conditionValue = value;

            if (condition == WaveConditionType.FloatParameter)
            {
                conditionValue = floatValue;
            }
            else if (condition == WaveConditionType.BoolParameter)
            {
                conditionValue = (boolValue == true) ? 1 : 0;
            }

            // Evaluate all child nodes if the condition is true
            if (IsConditionMet(inputValue, conditionValue, operand, false) == true)
            {
                if (IsConnectedNode(truePortName))
                {
                    // Evaluate true branch
                    yield return controller.StartCoroutine(EvaluateConnectedNode(controller, truePortName));
                }
            }
            else
            {
                if (IsConnectedNode(falsePortName) == true)
                {
                    // Evaluate false branch
                    yield return controller.StartCoroutine(EvaluateConnectedNode(controller, falsePortName));
                }
            }
        }

        public override WaveNode GetConnectedOutNode()
        {
            // True port is always equivilent to Out
            return GetConnectedNode(truePortName);
        }
        private bool IsParameter()
        {
            return condition == WaveConditionType.IntParameter || condition == WaveConditionType.FloatParameter || condition == WaveConditionType.BoolParameter;
        }

        private bool IsNotFloatOrBoolParameter()
        {
            return condition != WaveConditionType.FloatParameter && condition != WaveConditionType.BoolParameter;
        }

        internal static double GetInputValue(WaveSpawnController controller, WaveConditionType condition, string parameterName)
        {
            switch(condition)
            {
                case WaveConditionType.ItemsSpawned:
                    {
                        // Get the number of items spawned in this wave only
                        return controller.CurrentWaveSpawnedItemCount;
                    }

                case WaveConditionType.ItemsSpawnedTotal:
                    {
                        // Get the total number of items spawned by this controller
                        return controller.TotalSpawnedItemCount;
                    }

                case WaveConditionType.ItemsAlive:
                    {
                        // Get the total number of alive items spawned by this controller
                        return controller.SpawnedItemCount;
                    }

                case WaveConditionType.ItemsDestroyed:
                    {
                        // Get the total number of items destroyed in this wave only
                        return controller.CurrentWaveDestroyedItemCount;
                    }

                case WaveConditionType.ItemsDestroyedTotal:
                    {
                        // Get the total number of items destroyed by this controller
                        return controller.TotalDestroyedItemCount;
                    }

                case WaveConditionType.CurrentWaveNumber:
                    {
                        // Get the current wave number
                        return controller.CurrentWave;
                    }

                case WaveConditionType.IntParameter:
                    {
                        // Try to get int parameter
                        return controller.Parameters.GetInt(parameterName);
                    }

                case WaveConditionType.FloatParameter:
                    {
                        // try to get float parameter
                        return controller.Parameters.GetFloat(parameterName);
                    }

                case WaveConditionType.BoolParameter:
                    {
                        // Try to get bool parameter
                        return controller.Parameters.GetBool(parameterName) == true ? 1 : 0;
                    }
            }

            return 0;
        }

        internal static bool IsConditionMet(double inputValue, double value, WaveConditionOperator operand, bool isParameter)
        {
            switch(operand)
            {
                case WaveConditionOperator.Equals:
                    {
                        // Check for equality
                        return inputValue == value;
                    }

                case WaveConditionOperator.NotEquals:
                    {
                        // Check for non-equality
                        return inputValue != value;
                    }

                case WaveConditionOperator.GreaterThan:
                    {
                        // Check for greater than
                        return inputValue > value;
                    }

                case WaveConditionOperator.GreaterThanOrEqual:
                    {
                        // Check for greather than or equal
                        return inputValue >= value;
                    }

                case WaveConditionOperator.LessThan:
                    {
                        // Check for less than
                        return inputValue < value;
                    }

                case WaveConditionOperator.LessThanOrEqual:
                    {
                        // Check for less than or equal
                        return inputValue <= value;
                    }

                case WaveConditionOperator.Modulus:
                    {
                        if(isParameter == true)
                        {
                            Debug.LogError("Conditional operand 'Modulus' is not supported for parameter conditions");
                            return false;
                        }

                        return inputValue % value == 0;
                    }

                case WaveConditionOperator.InverseModulus:
                    {
                        if (isParameter == true)
                        {
                            Debug.LogError("Conditional operand 'Inverse Modulus' is not supported for parameter conditions");
                            return false;
                        }

                        return inputValue % value != 0;
                    }
            }
            return false;
        }
    }
}
