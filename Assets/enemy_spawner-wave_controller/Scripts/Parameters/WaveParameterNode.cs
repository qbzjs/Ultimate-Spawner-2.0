using System;
using System.Collections;

namespace UltimateSpawner.Waves.Parameters
{
    [Serializable]
    [NodeTint(250, 250, 250)]
    [CreateNodeMenu(null)]      // Hide in context menu
    public class WaveParameterNode : WaveNode
    {
        // Private
        [NonSerialized]
        private WaveParameterSet runtimeParameters = null;

        // Public
        public WaveParameterSet parameters = new WaveParameterSet();

        // Properties
        public override int NodeWidth
        {
            get { return 400; }
        }

        public override string NodeDisplayName
        {
            get { return "Parameters"; }
        }

        // Methods
        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            throw new NotImplementedException();
        }

        public WaveParameterSet GetRuntimeParameters()
        {
            if(runtimeParameters == null)
            {
                // Clone parameters to avoid modifying asset
                runtimeParameters = parameters.Clone();
            }
            return runtimeParameters;
        }
    }
}
