using System;

namespace UltimateSpawner.Waves.Parameters
{
    [Serializable]
    public class WaveParameter
    {
        // Types
        public enum WaveParameterType
        {
            Int,
            Float,
            Bool,
        }

        // Public
        public string parameterName;
        public WaveParameterType parameterType;
        public int intValue;
        public float floatValue;
        public bool boolValue;
    }
}
