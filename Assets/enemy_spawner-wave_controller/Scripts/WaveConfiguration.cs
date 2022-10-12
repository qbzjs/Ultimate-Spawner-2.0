using System;
using UnityEngine;
using XNode;
using System.Collections.Generic;
using UltimateSpawner.Waves.Parameters;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace UltimateSpawner.Waves
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Wave Graph", menuName = "Ultimate Spawner/Wave Graph")]
    public sealed class WaveConfiguration : NodeGraph
    {
        // Private
        private static readonly List<WaveNode> sharedNodes = new List<WaveNode>();
        
        // Constructor
        internal WaveConfiguration() {  }

        // Methods
#if UNITY_EDITOR
        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            WaveConfiguration config = EditorUtility.InstanceIDToObject(instanceID) as WaveConfiguration;

            if (config != null)
            {
                if(config.nodes.Count == 0)
                {
                    config.AddNode<WaveStartNode>();
                }

                if(config.nodes.Exists(n => n.GetType() == typeof(WaveParameterNode)) == false)
                {
                    WaveParameterNode node = config.AddNode<WaveParameterNode>();
                    node.position = new Vector2(50, 100);
                }

            }
            return false;
        }
#endif

        public WaveStartNode GetStartNode()
        {
            // Check all nodes in the grid
            foreach(Node node in nodes)
            {
                // Check for a start node
                if(node is WaveStartNode)
                {
                    // Get as start node
                    return node as WaveStartNode;
                }
            }

            // No start node found
            return null;
        }

        public WaveParameterNode GetParameterNode()
        {
            // Check all nodes in the grid
            foreach (Node node in nodes)
            {
                // Check for a parameter node
                if (node is WaveParameterNode)
                {
                    // Get as parameter node
                    return node as WaveParameterNode;
                }
            }

            // No parameter node found
            return null;
        }

        public int GetConnectedNodeCountOfType<T>() where T : WaveNode
        {
            // Reset shared list
            sharedNodes.Clear();

            // Get start node
            WaveNode start = GetStartNode();

            // Get connected count
            GetConnectedNodesOfType<T>(start, sharedNodes);

            return sharedNodes.Count;
        }

        public IEnumerable<T> GetConnectedNodesOfType<T>() where T : WaveNode
        {
            // Reset shared list
            sharedNodes.Clear();

            // Get start node
            WaveNode start = GetStartNode();

            // Get connected count
            GetConnectedNodesOfType<T>(start, sharedNodes);

            // Enumerate all
            foreach (T node in sharedNodes)
                yield return node;
        }

        public void GetConnectedNodesOfType<T>(WaveNode parent, IList<WaveNode> result) where T : WaveNode
        {
            // Get output port count
            GetConnectedNodesOfTypeForPort<T>(parent, WaveNode.outputPortName, result);

            // Get loop chain port count
            GetConnectedNodesOfTypeForPort<T>(parent, WaveLoopNode.loopChainPortName, result);


            // Conditionals
            GetConnectedNodesOfTypeForPort<T>(parent, WaveConditionNode.truePortName, result);
            GetConnectedNodesOfTypeForPort<T>(parent, WaveConditionNode.falsePortName, result);
        }

        public void GetConnectedNodesOfTypeForPort<T>(WaveNode parent, string portName, IList<WaveNode> result) where T : WaveNode
        {
            // Get the chain connection
            WaveNode connectedChain = parent.GetConnectedNode(portName);

            // Check for valid
            if (connectedChain != null)
            {
                if (connectedChain is T && result.Contains(connectedChain) == false)
                    result.Add(connectedChain);
                    

                // Process connected
                GetConnectedNodesOfType<T>(connectedChain, result);
            }
        }
    }
}
