using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Junya005.Nodevel;

namespace Junya005.Nodevel.Editor
{
    /// <summary>
    /// Nodevel のノードグラフを表示・操作する UI Toolkit GraphView。
    /// </summary>
    public class NodevelGraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        public NodevelGraph CurrentGraph { get; private set; }
        public event Action<NodevelNodeView> OnNodeSelected;

        private readonly Dictionary<string, NodevelNodeView> _nodeViews = new Dictionary<string, NodevelNodeView>();

        public NodevelGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            style.flexGrow = 1;

            graphViewChanged = OnGraphViewChanged;

            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.target == this)
                {
                    OnNodeSelected?.Invoke(null);
                }
            });
        }

        public void PopulateView(NodevelGraph graph)
        {
            CurrentGraph = graph;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            _nodeViews.Clear();
            graphViewChanged = OnGraphViewChanged;

            if (CurrentGraph == null) return;

            // ノードの生成
            foreach (var node in CurrentGraph.Nodes)
            {
                CreateNodeView(node);
            }

            // エッジの生成
            foreach (var edge in CurrentGraph.Edges)
            {
                if (_nodeViews.TryGetValue(edge.FromNodeId, out var fromView) &&
                    _nodeViews.TryGetValue(edge.ToNodeId, out var toView))
                {
                    if (edge.FromPortIndex < fromView.OutputPorts.Count && toView.InputPort != null)
                    {
                        var fromPort = fromView.OutputPorts[edge.FromPortIndex];
                        var toPort = toView.InputPort;

                        var gvEdge = fromPort.ConnectTo(toPort);
                        AddElement(gvEdge);
                    }
                }
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatible = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatible.Add(port);
                }
            });
            return compatible;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (CurrentGraph == null) return;

            var mousePos = viewTransform.matrix.inverse.MultiplyPoint3x4(evt.localMousePosition);

            evt.menu.AppendAction("Create Node/Start Node", _ => CreateNode<StartNode>(mousePos));
            evt.menu.AppendAction("Create Node/Script Node", _ => CreateNode<ScriptNode>(mousePos));
            evt.menu.AppendAction("Create Node/Choice Node", _ => CreateNode<ChoiceNode>(mousePos));
            evt.menu.AppendAction("Create Node/Condition Node", _ => CreateNode<ConditionNode>(mousePos));
            evt.menu.AppendAction("Create Node/Set Variable Node", _ => CreateNode<SetVariableNode>(mousePos));
            evt.menu.AppendAction("Create Node/SubGraph Node", _ => CreateNode<SubGraphNode>(mousePos));
            evt.menu.AppendAction("Create Node/End Node", _ => CreateNode<EndNode>(mousePos));
        }

        private void CreateNode<T>(Vector2 position) where T : NodeBase
        {
            if (CurrentGraph == null) return;

            Undo.RecordObject(CurrentGraph, "Create Nodevel Node");
            var node = CurrentGraph.CreateNode<T>();
            node.Position = position;
            AssetDatabase.AddObjectToAsset(node, CurrentGraph);
            AssetDatabase.SaveAssets();

            CreateNodeView(node);
        }

        private NodevelNodeView CreateNodeView(NodeBase node)
        {
            var view = new NodevelNodeView(node);
            view.OnViewSelected += v => OnNodeSelected?.Invoke(v);
            view.OnViewUnselected += v =>
            {
                if (selection.Count <= 1)
                {
                    OnNodeSelected?.Invoke(null);
                }
            };
            _nodeViews[node.Id] = view;
            AddElement(view);
            return view;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (CurrentGraph == null) return change;

            // 削除された要素の同期
            if (change.elementsToRemove != null)
            {
                Undo.RecordObject(CurrentGraph, "Delete Nodevel Elements");

                foreach (var element in change.elementsToRemove)
                {
                    if (element is NodevelNodeView nodeView)
                    {
                        CurrentGraph.RemoveNode(nodeView.TargetNode);
                        _nodeViews.Remove(nodeView.TargetNode.Id);
                        Undo.DestroyObjectImmediate(nodeView.TargetNode);
                    }
                    else if (element is Edge edge)
                    {
                        if (edge.output?.node is NodevelNodeView fromView)
                        {
                            var portIndex = fromView.OutputPorts.IndexOf(edge.output);
                            if (portIndex >= 0)
                            {
                                CurrentGraph.Disconnect(fromView.TargetNode.Id, portIndex);
                            }
                        }
                    }
                }
                EditorUtility.SetDirty(CurrentGraph);
            }

            // 新規接続（エッジ）の同期
            if (change.edgesToCreate != null)
            {
                Undo.RecordObject(CurrentGraph, "Connect Nodevel Nodes");

                foreach (var edge in change.edgesToCreate)
                {
                    if (edge.output?.node is NodevelNodeView fromView &&
                        edge.input?.node is NodevelNodeView toView)
                    {
                        var fromPortIndex = fromView.OutputPorts.IndexOf(edge.output);
                        var toPortIndex = 0;

                        if (fromPortIndex >= 0)
                        {
                            CurrentGraph.Connect(fromView.TargetNode.Id, fromPortIndex, toView.TargetNode.Id, toPortIndex);
                        }
                    }
                }
                EditorUtility.SetDirty(CurrentGraph);
            }

            // ノード移動の同期
            if (change.movedElements != null)
            {
                Undo.RecordObject(CurrentGraph, "Move Nodevel Nodes");
                foreach (var element in change.movedElements)
                {
                    if (element is NodevelNodeView nodeView)
                    {
                        nodeView.TargetNode.Position = nodeView.GetPosition().position;
                        EditorUtility.SetDirty(nodeView.TargetNode);
                    }
                }
            }

            return change;
        }

        public void HighlightNode(string nodeId, bool highlight)
        {
            if (_nodeViews.TryGetValue(nodeId, out var view))
            {
                view.SetHighlight(highlight);
            }
        }
    }
}

