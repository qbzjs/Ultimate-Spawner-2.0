using System;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateSpawner.Waves.Parameters
{
    [Serializable]
    public class WaveParameterSet
    {
        // Private
        [SerializeField]
        private List<WaveParameter> parameters = new List<WaveParameter>();

        // Properties
        public int ParameterCount
        {
            get { return parameters.Count; }
        }

        public IList<WaveParameter> Parameters
        {
            get { return parameters; }
        }

        // Methods
        public WaveParameterSet Clone()
        {
            List<WaveParameter> clones = new List<WaveParameter>();

            foreach(WaveParameter parameter in parameters)
            {
                clones.Add(new WaveParameter
                {
                    parameterName = parameter.parameterName,
                    parameterType = parameter.parameterType,
                    intValue = parameter.intValue,
                    floatValue = parameter.floatValue,
                    boolValue = parameter.boolValue,
                });
            }

            return new WaveParameterSet
            {
                parameters = clones,
            };
        }

        public void AddParameter(string parameterName, WaveParameter.WaveParameterType parameterType)
        {
            // Check for invalid name
            if (string.IsNullOrEmpty(parameterName) == true)
                throw new ArgumentException("parameterName cannot be null or empty");

            // Check for already exists
            if (parameters.Exists(p => p.parameterName == parameterName) == true)
                throw new InvalidOperationException("A parameter with the same name already exists");

            parameters.Add(new WaveParameter
            {
                parameterName = parameterName,
                parameterType = parameterType,
            });
        }

        public void RemoveParameter(WaveParameter parameter)
        {
            if (parameters.Contains(parameter) == true)
                parameters.Remove(parameter);
        }

        public bool HasParameterWithName(string parameterName)
        {
            return parameters.Exists(p => p.parameterName == parameterName);
        }

        public void SetInt(string parameterName, int value)
        {
            // Check for invalid name
            if (string.IsNullOrEmpty(parameterName) == true)
                throw new ArgumentException("parameterName cannot be null or empty");

            // Find the parameter
            WaveParameter parameter = parameters.Find(p => p.parameterName == parameterName && p.parameterType == WaveParameter.WaveParameterType.Int);

            if(parameter == null)
            {
                Debug.LogErrorFormat("A parameter with name '{0}' and type 'Int' does not exist!", parameterName);
                return;
            }

            // Set the value
            parameter.intValue = value;
        }

        public void SetFloat(string parameterName, float value)
        {
            // Check for invalid name
            if (string.IsNullOrEmpty(parameterName) == true)
                throw new ArgumentException("parameterName cannot be null or empty");

            // Find the parameter
            WaveParameter parameter = parameters.Find(p => p.parameterName == parameterName && p.parameterType == WaveParameter.WaveParameterType.Float);

            if (parameter == null)
            {
                Debug.LogErrorFormat("A parameter with name '{0}' and type 'Float' does not exist!", parameterName);
                return;
            }

            // Set the value
            parameter.floatValue = value;
        }

        public void SetBool(string parameterName, bool value)
        {
            // Check for invalid name
            if (string.IsNullOrEmpty(parameterName) == true)
                throw new ArgumentException("parameterName cannot be null or empty");

            // Find the parameter
            WaveParameter parameter = parameters.Find(p => p.parameterName == parameterName && p.parameterType == WaveParameter.WaveParameterType.Bool);

            if (parameter == null)
            {
                Debug.LogErrorFormat("A parameter with name '{0}' and type 'Bool' does not exist!", parameterName);
                return;
            }

            // Set the value
            parameter.boolValue = value;
        }

        public int GetInt(string parameterName)
        {
            // Check for invalid name
            if (string.IsNullOrEmpty(parameterName) == true)
                throw new ArgumentException("parameterName cannot be null or empty");

            // Find the parameter
            WaveParameter parameter = parameters.Find(p => p.parameterName == parameterName && p.parameterType == WaveParameter.WaveParameterType.Int);

            if (parameter == null)
            {
                Debug.LogErrorFormat("A parameter with name '{0}' and type 'Int' does not exist!", parameterName);
                return -1;
            }

            // Get the value
            return parameter.intValue;
        }

        public float GetFloat(string parameterName)
        {
            // Check for invalid name
            if (string.IsNullOrEmpty(parameterName) == true)
                throw new ArgumentException("parameterName cannot be null or empty");

            // Find the parameter
            WaveParameter parameter = parameters.Find(p => p.parameterName == parameterName && p.parameterType == WaveParameter.WaveParameterType.Float);

            if (parameter == null)
            {
                Debug.LogErrorFormat("A parameter with name '{0}' and type 'Float' does not exist!", parameterName);
                return -1;
            }

            // Get the value
            return parameter.floatValue;
        }

        public bool GetBool(string parameterName)
        {
            // Check for invalid name
            if (string.IsNullOrEmpty(parameterName) == true)
                throw new ArgumentException("parameterName cannot be null or empty");

            // Find the parameter
            WaveParameter parameter = parameters.Find(p => p.parameterName == parameterName && p.parameterType == WaveParameter.WaveParameterType.Bool);

            if (parameter == null)
            {
                Debug.LogErrorFormat("A parameter with name '{0}' and type 'Bool' does not exist!", parameterName);
                return false;
            }

            // Get the value
            return parameter.boolValue;
        }
    }
}
