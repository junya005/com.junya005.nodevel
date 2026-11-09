using System;
using System.Collections.Generic;
using UnityEngine;
namespace Junya005.Nodevel
{
    /// <summary>
    /// ノベルのシナリオフローを定義するノードグラフアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewNodevelGraph", menuName = "Nodevel/Graph Asset")]
    public class NodevelGraph : ScriptableObject
    {
        [SerializeField] private List<NodeBase> _nodes = new List<NodeBase>();
        [SerializeField] private List<NodeEdge> _edges = new List<NodeEdge>();
        [SerializeField] private List<GraphVariable> _variables = new List<GraphVariable>();

        public IReadOnlyList<NodeBase> Nodes => _nodes;
        public IReadOnlyList<NodeEdge> Edges => _edges;
        public IReadOnlyList<GraphVariable> Variables => _variables;

        public T CreateNode<T>(string id = null) where T : NodeBase
        {
            var node = CreateInstance<T>();
            node.Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString("N") : id;
            node.Title = typeof(T).Name;
            _nodes.Add(node);
            return node;
        }

        public void AddNode(NodeBase node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (!_nodes.Contains(node))
            {
                _nodes.Add(node);
            }
        }

        public void RemoveNode(NodeBase node)
        {
            if (node == null) return;
            _nodes.Remove(node);
            _edges.RemoveAll(e => e.FromNodeId == node.Id || e.ToNodeId == node.Id);
        }

        public void Connect(string fromNodeId, int fromPortIndex, string toNodeId, int toPortIndex)
        {
            // 同一出力ポートからの既存接続を除去
            _edges.RemoveAll(e => e.FromNodeId == fromNodeId && e.FromPortIndex == fromPortIndex);
            _edges.Add(new NodeEdge(fromNodeId, fromPortIndex, toNodeId, toPortIndex));
        }

        public void Disconnect(string fromNodeId, int fromPortIndex)
        {
            _edges.RemoveAll(e => e.FromNodeId == fromNodeId && e.FromPortIndex == fromPortIndex);
        }

        public NodeBase GetNode(string id)
        {
            for (var i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].Id == id) return _nodes[i];
            }
            return null;
        }

        /// <summary>
        /// Blackboard 変数を追加します。すでに同名の変数が存在する場合は false を返します。
        /// </summary>
        public bool AddVariable(GraphVariable variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            if (string.IsNullOrEmpty(variable.Name)) return false;

            for (var i = 0; i < _variables.Count; i++)
            {
                if (string.Equals(_variables[i].Name, variable.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            _variables.Add(variable);
            return true;
        }

        /// <summary>
        /// 指定された名前の Blackboard 変数を削除します。
        /// </summary>
        public bool RemoveVariable(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            for (var i = 0; i < _variables.Count; i++)
            {
                if (string.Equals(_variables[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    _variables.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定された名前の Blackboard 変数を取得します。
        /// </summary>
        public GraphVariable GetVariable(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            for (var i = 0; i < _variables.Count; i++)
            {
                if (string.Equals(_variables[i].Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return _variables[i];
                }
            }
            return null;
        }

        public StartNode GetStartNode()
        {
            for (var i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i] is StartNode start) return start;
            }
            return null;
        }

        public NodeBase GetNextNode(string currentNodeId, int outputPortIndex)
        {
            for (var i = 0; i < _edges.Count; i++)
            {
                var edge = _edges[i];
                if (edge.FromNodeId == currentNodeId && edge.FromPortIndex == outputPortIndex)
                {
                    return GetNode(edge.ToNodeId);
                }
            }
            return null;
        }
    }
}

