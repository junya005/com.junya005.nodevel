using System;
using UnityEngine;

namespace Junya005.Nodevel
{
    public enum VariableOperation
    {
        Set = 0,
        Add = 1,
        Toggle = 2
    }

    /// <summary>
    /// 変数の代入・加算・トグル操作を行うノード。
    /// </summary>
    [Serializable]
    public class SetVariableNode : NodeBase
    {
        [SerializeField] private string _variableName;
        [SerializeField] private VariableOperation _operation;
        [SerializeField] private string _value;

        public string VariableName
        {
            get => _variableName;
            set => _variableName = value;
        }

        public VariableOperation Operation
        {
            get => _operation;
            set => _operation = value;
        }

        public string Value
        {
            get => _value;
            set => _value = value;
        }

        public override int OutputPortCount => 1;
    }
}

