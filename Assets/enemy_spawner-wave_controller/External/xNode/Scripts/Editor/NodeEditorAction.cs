﻿using System;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    public partial class NodeEditorWindow {

        public static bool isPanning { get; private set; }
        public static Vector2 dragOffset;

        private bool IsDraggingNode { get { return draggedNode != null; } }
        private bool IsDraggingPort { get { return draggedOutput != null; } }
        private bool IsHoveringPort { get { return hoveredPort != null; } }
        private bool IsHoveringNode { get { return hoveredNode != null; } }
        private bool HasSelectedNode { get { return selectedNode != null; } }

        private XNode.Node hoveredNode = null;

        [NonSerialized] private XNode.Node selectedNode = null;
        [NonSerialized] private XNode.Node draggedNode = null;
        [NonSerialized] private XNode.NodePort hoveredPort = null;
        [NonSerialized] private XNode.NodePort draggedOutput = null;
        [NonSerialized] private XNode.NodePort draggedOutputTarget = null;

        //private Rect nodeRects;

        public void Controls() {
            wantsMouseMove = true;
            Event e = Event.current;
            switch (e.type) {
                case EventType.MouseMove:
                    break;
                case EventType.ScrollWheel:
                    if (e.delta.y > 0) zoom += 0.1f * zoom;
                    else zoom -= 0.1f * zoom;
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0) {
                        if (IsDraggingPort) {
                            if (IsHoveringPort && hoveredPort.IsInput) {
                                if (!draggedOutput.IsConnectedTo(hoveredPort)) {
                                    draggedOutputTarget = hoveredPort;
                                }
                            } else {
                                draggedOutputTarget = null;
                            }
                            Repaint();
                        } else if (IsDraggingNode) {
                            draggedNode.position = WindowToGridPosition(e.mousePosition) + dragOffset;
                            if (NodeEditorPreferences.gridSnap) {
                                draggedNode.position.x = (Mathf.Round((draggedNode.position.x + 8) / 16) * 16) - 8;
                                draggedNode.position.y = (Mathf.Round((draggedNode.position.y + 8) / 16) * 16) - 8;
                            }
                            Repaint();
                        }
                    } else if (e.button == 2) {
                        panOffset += e.delta * zoom;
                        isPanning = true;
                    }
                    break;
                case EventType.MouseDown:
                    Repaint();
                    if (e.button == 0) {
                        SelectNode(hoveredNode);
                        if (IsHoveringPort) {
                            if (hoveredPort.IsOutput) {
                                draggedOutput = hoveredPort;
                            } else {
                                hoveredPort.VerifyConnections();
                                if (hoveredPort.IsConnected) {
                                    XNode.Node node = hoveredPort.node;
                                    XNode.NodePort output = hoveredPort.Connection;
                                    hoveredPort.Disconnect(output);
                                    draggedOutput = output;
                                    draggedOutputTarget = hoveredPort;
                                    if (NodeEditor.onUpdateNode != null) NodeEditor.onUpdateNode(node);
                                }
                            }
                        } else if (IsHoveringNode && IsHoveringTitle(hoveredNode)) {
                            draggedNode = hoveredNode;
                            dragOffset = hoveredNode.position - WindowToGridPosition(e.mousePosition);
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        //Port drag release
                        if (IsDraggingPort)
                        {
                            //If connection is valid, save it
                            if (draggedOutputTarget != null)
                            {
                                XNode.Node node = draggedOutputTarget.node;
                                if (graph.nodes.Count != 0) draggedOutput.Connect(draggedOutputTarget);
                                if (NodeEditor.onUpdateNode != null) NodeEditor.onUpdateNode(node);
                                EditorUtility.SetDirty(graph);
                            }
                            //Release dragged connection
                            draggedOutput = null;
                            draggedOutputTarget = null;
                            EditorUtility.SetDirty(graph);
                            Repaint();
                            AssetDatabase.SaveAssets();
                        }
                        else if (IsDraggingNode)
                        {
                            draggedNode = null;
                            AssetDatabase.SaveAssets();
                        }
                        else if (GUIUtility.hotControl != 0)
                        {
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else if (e.button == 1)
                    {
                        if (!isPanning)
                        {
                            if (IsHoveringNode && IsHoveringTitle(hoveredNode))
                            {
                                ShowNodeContextMenu(hoveredNode);
                            }
                            else if (!IsHoveringNode)
                            {
                                ShowGraphContextMenu();
                            }
                        }
                        isPanning = false;
                    }
                    else if (e.button == 2)
                        isPanning = false;
                    break;
            }
        }

        /// <summary> Puts all nodes in focus. If no nodes are present, resets view to  </summary>
        public void Home() {
            zoom = 2;
            panOffset = Vector2.zero;
        }

        public void CreateNode(Type type, Vector2 position) {
            XNode.Node node = graph.AddNode(type);
            node.position = position;
            Repaint();
        }

        /// <summary> Draw a connection as we are dragging it </summary>
        public void DrawDraggedConnection() {
            if (IsDraggingPort) {
                if (!_portConnectionPoints.ContainsKey(draggedOutput)) return;
                Vector2 from = _portConnectionPoints[draggedOutput].center;
                Vector2 to = draggedOutputTarget != null ? portConnectionPoints[draggedOutputTarget].center : WindowToGridPosition(Event.current.mousePosition);
                Color col = NodeEditorPreferences.GetTypeColor(draggedOutput.ValueType);
                col.a = 0.6f;
                DrawConnection(from, to, col);
            }
        }

        bool IsHoveringTitle(XNode.Node node) {
            Vector2 mousePos = Event.current.mousePosition;
            //Get node position
            Vector2 nodePos = GridToWindowPosition(node.position);
            float width = 200;
            if (nodeWidths.ContainsKey(node)) width = nodeWidths[node];
            Rect windowRect = new Rect(nodePos, new Vector2(width / zoom, 30 / zoom));
            return windowRect.Contains(mousePos);
        }
    }
}