using System;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// ノード間の接続を表すエッジデータ。
    /// </summary>
    [Serializable]
    public struct NodeEdge : IEquatable<NodeEdge>
    {
        [SerializeField] public string FromNodeId;
        [SerializeField] public int FromPortIndex;
        [SerializeField] public string ToNodeId;
        [SerializeField] public int ToPortIndex;

        public NodeEdge(string fromNodeId, int fromPortIndex, string toNodeId, int toPortIndex)
        {
            FromNodeId = fromNodeId;
            FromPortIndex = fromPortIndex;
            ToNodeId = toNodeId;
            ToPortIndex = toPortIndex;
        }

        public bool Equals(NodeEdge other)
        {
            return FromNodeId == other.FromNodeId &&
                   FromPortIndex == other.FromPortIndex &&
                   ToNodeId == other.ToNodeId &&
                   ToPortIndex == other.ToPortIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeEdge other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FromNodeId != null ? FromNodeId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ FromPortIndex;
                hashCode = (hashCode * 397) ^ (ToNodeId != null ? ToNodeId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ToPortIndex;
                return hashCode;
            }
        }
    }
}

