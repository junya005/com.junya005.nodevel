using System;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// 別グラフアセットをサブルーチンとして実行するノード。
    /// </summary>
    [Serializable]
    public class SubGraphNode : NodeBase
    {
        [SerializeField] private NodevelGraph _subGraph;

        public NodevelGraph SubGraph
        {
            get => _subGraph;
            set => _subGraph = value;
        }

        public override int OutputPortCount => 1;
    }
}

