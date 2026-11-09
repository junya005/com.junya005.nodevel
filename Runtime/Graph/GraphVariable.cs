using System;
using System.Globalization;
using UnityEngine;
namespace Junya005.Nodevel
{
    /// <summary>
    /// Blackboard で定義されるシナリオ変数のデータモデル。
    /// </summary>
    [Serializable]
    public class GraphVariable
    {
        [SerializeField] private string _name;
        [SerializeField] private VariableType _type;
        [SerializeField] private string _initialValue;
        [SerializeField] private string _description;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public VariableType Type
        {
            get => _type;
            set => _type = value;
        }

        public string InitialValue
        {
            get => _initialValue;
            set => _initialValue = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public GraphVariable()
        {
        }

        public GraphVariable(string name, VariableType type, string initialValue = "", string description = "")
        {
            _name = name;
            _type = type;
            _initialValue = initialValue;
            _description = description;
        }

        /// <summary>
        /// 定義された型および初期値文字列から VariableValue を生成します。
        /// </summary>
        public VariableValue ToVariableValue()
        {
            switch (_type)
            {
                case VariableType.Bool:
                    if (bool.TryParse(_initialValue, out var b))
                    {
                        return VariableValue.Create(b);
                    }
                    if (int.TryParse(_initialValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bi))
                    {
                        return VariableValue.Create(bi != 0);
                    }
                    return VariableValue.Create(false);

                case VariableType.Int:
                    if (int.TryParse(_initialValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                    {
                        return VariableValue.Create(i);
                    }
                    return VariableValue.Create(0);

                case VariableType.Float:
                    if (float.TryParse(_initialValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                    {
                        return VariableValue.Create(f);
                    }
                    return VariableValue.Create(0f);

                case VariableType.String:
                default:
                    return VariableValue.Create(_initialValue ?? string.Empty);
            }
        }
    }
}

