using System;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// ノードグラフ上に配置される全ノードの抽象基底クラス。
    /// </summary>
    [Serializable]
    public abstract class NodeBase : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _title;
        [SerializeField] private Vector2 _position;

        public string Id
        {
            get => _id;
            set => _id = value;
        }

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        /// <summary>
        /// 出力ポートの総数。
        /// </summary>
        public virtual int OutputPortCount => 1;
    }
}

