using System;
using System.Collections;
using UltimateSpawner.Spawning;
using UnityEngine;
using XNode;

namespace UltimateSpawner.Waves
{
    public enum ReferenceMode
    {
        UseTag,
        UseName,
        UseID,
    }

    [Serializable]
    public abstract class WaveReferenceNode : WaveNode
    {
        // Public
        public ReferenceMode referenceMode = ReferenceMode.UseTag;

        [DisplayCondition("referenceMode", ReferenceMode.UseTag)]
        public string referenceTag = "Respawn";

        [DisplayCondition("referenceMode", ReferenceMode.UseName)]
        public string referenceName = "Spawn Point 1";

        [DisplayCondition("referenceMode", ReferenceMode.UseID)]
        public int referenceID = 0;

        // Properties
        public override int NodeWidth
        {
            get { return base.NodeWidth - 40; }
        }

        public override int NodeLabelWidth
        {
            get { return base.NodeLabelWidth - 20; }
        }

        // Methods
        public override IEnumerator Evaluate(WaveSpawnController controller)
        {
            // SHould not be implemented - this node has no executable behaiour
            throw new NotImplementedException();
        }

        public override bool CanHaveMultipleConnections(NodePort from)
        {
            // All reference nodes should allow multiple connections
            if (from.IsOutput == true)
                return true;

            return false;
        }
    }
}
