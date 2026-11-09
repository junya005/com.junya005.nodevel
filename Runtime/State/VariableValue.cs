using System;
using System.Globalization;

namespace Junya005.Nodevel
{
    /// <summary>
    /// ボクシングを避けて変数の値を保持する軽量構造体。
    /// </summary>
    [Serializable]
    public struct VariableValue : IEquatable<VariableValue>
    {
        public VariableType Type;
        public long IntValue;
        public double FloatValue;
        public string StringValue;

        public static VariableValue Create(bool value)
        {
            return new VariableValue
            {
                Type = VariableType.Bool,
                IntValue = value ? 1 : 0
            };
        }

        public static VariableValue Create(int value)
        {
            return new VariableValue
            {
                Type = VariableType.Int,
                IntValue = value,
                FloatValue = value
            };
        }

        public static VariableValue Create(float value)
        {
            return new VariableValue
            {
                Type = VariableType.Float,
                FloatValue = value,
                IntValue = (long)value
            };
        }

        public static VariableValue Create(string value)
        {
            return new VariableValue
            {
                Type = VariableType.String,
                StringValue = value ?? string.Empty
            };
        }

        public bool AsBool()
        {
            if (Type == VariableType.Bool || Type == VariableType.Int)
            {
                return IntValue != 0;
            }
            if (Type == VariableType.String)
            {
                return bool.TryParse(StringValue, out var b) && b;
            }
            return false;
        }

        public int AsInt()
        {
            if (Type == VariableType.Int || Type == VariableType.Bool)
            {
                return (int)IntValue;
            }
            if (Type == VariableType.Float)
            {
                return (int)FloatValue;
            }
            if (Type == VariableType.String && int.TryParse(StringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
            {
                return i;
            }
            return 0;
        }

        public float AsFloat()
        {
            if (Type == VariableType.Float || Type == VariableType.Int || Type == VariableType.Bool)
            {
                return (float)FloatValue;
            }
            if (Type == VariableType.String && float.TryParse(StringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
            {
                return f;
            }
            return 0f;
        }

        public string AsString()
        {
            switch (Type)
            {
                case VariableType.Bool:
                    return IntValue != 0 ? "true" : "false";
                case VariableType.Int:
                    return IntValue.ToString(CultureInfo.InvariantCulture);
                case VariableType.Float:
                    return FloatValue.ToString(CultureInfo.InvariantCulture);
                case VariableType.String:
                    return StringValue ?? string.Empty;
                default:
                    return string.Empty;
            }
        }

        public bool Equals(VariableValue other)
        {
            if (Type != other.Type) return false;
            switch (Type)
            {
                case VariableType.Bool:
                case VariableType.Int:
                    return IntValue == other.IntValue;
                case VariableType.Float:
                    return Math.Abs(FloatValue - other.FloatValue) < 0.000001;
                case VariableType.String:
                    return string.Equals(StringValue, other.StringValue, StringComparison.Ordinal);
                default:
                    return true;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is VariableValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Type;
                hashCode = (hashCode * 397) ^ IntValue.GetHashCode();
                hashCode = (hashCode * 397) ^ FloatValue.GetHashCode();
                hashCode = (hashCode * 397) ^ (StringValue != null ? StringValue.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}

