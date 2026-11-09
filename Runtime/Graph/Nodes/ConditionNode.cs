using System;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// 変数の条件式を評価し、TrueまたはFalseの出力ポートへ分岐するノード。
    /// </summary>
    [Serializable]
    public class ConditionNode : NodeBase
    {
        [SerializeField] private string _variableName;
        [SerializeField] private string _operator = "==";
        [SerializeField] private string _rightValue;

        public string VariableName
        {
            get => _variableName;
            set => _variableName = value;
        }

        public string Operator
        {
            get => _operator;
            set => _operator = value;
        }

        public string RightValue
        {
            get => _rightValue;
            set => _rightValue = value;
        }

        /// <summary>
        /// Port 0: True, Port 1: False
        /// </summary>
        public override int OutputPortCount => 2;
    }
}

