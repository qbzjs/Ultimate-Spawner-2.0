using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace UltimateSpawner.Waves
{
    [Serializable]
    public abstract class WaveNode : Node
    {
        // Public
        public const string inputPortName = "In";
        public const string outputPortName = "Out";

        // Properties
        public virtual int NodeWidth
        {
            get { return 280; }
        }

        public virtual int NodeLabelWidth
        {
            get { return 132; }
        }

        public virtual string NodeDisplayName
        {
            get { return GetType().Name; }
        }
        
        public bool IsInputConnected
        {
            get
            {
                // Get the input port
                NodePort port = InputPort;

                // Check for erro
                if (port == null)
                    return false;

                // Check for connected
                return port.IsConnected;
            }
        }

        public NodePort InputPort
        {
            get { return GetInputPort(inputPortName); }
        }

        public bool HasInputPort
        {
            get { return InputPort != null; }
        }

        public bool IsOutputConnected
        {
            get
            {
                // Get the output port
                NodePort port = OutputPort;

                // Check for erro
                if (port == null)
                    return false;

                // Check for connected
                return port.IsConnected;
            }
        }

        public bool HasOutputPort
        {
            get { return OutputPort != null; }
        }

        public NodePort OutputPort
        {
            get { return GetOutputPort(outputPortName); }
        }

        // Methods
        protected override void Init()
        {
            base.Init();

#if UNITY_EDITOR
            // Get the display name of the node
            name = NodeDisplayName;
#endif
        }

        public abstract IEnumerator Evaluate(WaveSpawnController controller);

        public virtual IEnumerator EvaluateConnectedOutNode(WaveSpawnController controller)
        {
            WaveNode node = GetConnectedOutNode();

            if (node == null)
                return null;

            // Evaluate the connected node
            return node.Evaluate(controller);
        }

        public virtual IEnumerator EvaluateConnectedNode(WaveSpawnController controller, string portName)
        {
            // Get the connected node
            WaveNode node = GetConnectedNode(portName);

            if (node == null)
                return null;

            // Evaluate the connected node
            return node.Evaluate(controller);
        }

        // #### Multiple connections on output nodes are not supported
        #region UnsupportedCode
        //public virtual IEnumerator EvaluateConnectedOutNodes(WaveSpawnController controller)
        //{
        //    List<YieldInstruction> branches = new List<YieldInstruction>();

        //    foreach(WaveNode node in GetConnectedOutNodes())
        //    {
        //        // Run the branch
        //        IEnumerator branch = node.Evaluate(controller);

        //        YieldInstruction task = controller.StartCoroutine(branch);

        //        // Register the yeidl
        //        branches.Add(task);
        //    }

        //    foreach (YieldInstruction branch in branches)
        //        yield return branch;
        //}

        //public virtual IEnumerator EvaluateConnectedNodes(WaveSpawnController controller, string portName)
        //{
        //    List<IEnumerator> branches = new List<IEnumerator>();
        //    List<IEnumerator> deadBranches = new List<IEnumerator>();

        //    foreach (WaveNode node in GetConnectedNodes(portName))
        //    {
        //        // Run the branch
        //        IEnumerator branch = node.Evaluate(controller);

        //        // Register the yeidl
        //        branches.Add(branch);
        //    }

        //    while (branches.Count > 0)
        //    {
        //        foreach (IEnumerator update in branches)
        //        {
        //            if (update.MoveNext() == false)
        //            {
        //                deadBranches.Add(update);
        //            }
        //        }

        //        foreach (IEnumerator dead in deadBranches)
        //            branches.Remove(dead);

        //        deadBranches.Clear();

        //        yield return null;
        //    }
        //}
        #endregion

        public virtual void OnGenerateWaveSession() { }

        public override object GetValue(NodePort port)
        {
            return null;
        }

        public bool IsConnectedOutNode()
        {
            return IsOutputConnected;
        }

        public bool IsConnectedNode(string portName)
        {
            NodePort match = null;

            foreach (NodePort port in Ports)
            {
                if (port.fieldName == portName)
                {
                    match = port;
                    break;
                }
            }

            // Check for matched port
            if (match == null)
                return false;

            // Check for connection
            return match.IsConnected;
        }

        public virtual WaveNode GetConnectedOutNode()
        {
            if (IsOutputConnected == false)
                return null;

            // Get the output node
            return OutputPort.Connection.node as WaveNode;
        }

        public virtual WaveNode GetConnectedNode(string portName)
        {
            NodePort match = null;

            foreach(NodePort port in Ports)
            {
                if(port.fieldName == portName)
                {
                    match = port;
                    break;
                }
            }

            // Check for matched port
            if (match == null || match.Connection == null)
                return null;
            
            // Get the conected node
            return match.Connection.node as WaveNode;
        }

        // #### Multiple connections on output nodes are not supported
        #region UnsupportedCode
        //public virtual IEnumerable<WaveNode> GetConnectedOutNodes()
        //{
        //    // Check for output
        //    if (IsOutputConnected == false)
        //        yield break;

        //    // Check for single connection
        //    if (OutputPort.ConnectionCount == 1)
        //    {
        //        yield return OutputPort.Connection.node as WaveNode;
        //    }
        //    else
        //    {
        //        // Get all connected nodes
        //        for (int i = 0; i < OutputPort.ConnectionCount; i++)
        //            yield return OutputPort.GetConnection(i).node as WaveNode;
        //    }
        //}

        //public virtual IEnumerable<WaveNode> GetConnectedNodes(string portName)
        //{
        //    NodePort matchedPort = null;

        //    foreach(NodePort port in Ports)
        //    {
        //        if(port.IsOutput == true && port.fieldName == portName)
        //        {
        //            matchedPort = port;
        //            break;
        //        }
        //    }

        //    if (matchedPort == null)
        //        yield break;

        //    if (matchedPort.ConnectionCount == 1)
        //    {
        //        yield return matchedPort.Connection.node as WaveNode;
        //    }
        //    else
        //    {
        //        for (int i = 0; i < matchedPort.ConnectionCount; i++)
        //            yield return matchedPort.GetConnection(i).node as WaveNode;
        //    }
        //}
        #endregion

        public virtual bool CanConnectTo(NodePort from, NodePort to)
        {
            if(from.fieldName == outputPortName)
            {
                if(to.fieldName == inputPortName)
                {
                    // Check for special case
                    if (to.ValueType == typeof(WaveSubNode))
                        return false;

                    return true;
                }
                return false;
            }

            return true;
        }

        public virtual bool CanHaveMultipleConnections(NodePort from)
        {
            // Ouptut ports can have more than one connection
            //if(from.fieldName == outputPortName)
            //    return true;

            // All other ports are singular unless overriden
            return false;
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            // Dont allow connect to self
            if(from.node == to.node || CanConnectTo(from, to) == false)
            {
                // Delete connection
                from.Disconnect(to);
            }

            // Only allow one connection
            if (from.node == this && CanHaveMultipleConnections(from) == false)
            {
                if (from.ConnectionCount > 1 && from.node is WaveNode)
                {
                    // Delete all connections and restore the connection
                    from.ClearConnections();
                    from.Connect(to);
                }
            }

            // Destroy connection if type is wrong
            if ((from.node is WaveNode) == false)
            {
                Debug.LogWarning("Only wave types such as 'WaveNode' can be assigned to an input port");
                
                // Dont allow the connection
                from.Disconnect(to);
            }
        }

        public WaveNode GetWaveNode(NodePort port)
        {
            // Check for error
            if (port == null)
                return null;

            // Get the connected node
            if (port.IsConnected == false)
                return null;

            // Get the connected node
            Node connected = port.Connection.node;

            // Check for error
            if (connected == null)
                return null;

            // Get the wave node
            return connected as WaveNode;
        }
    }
}
