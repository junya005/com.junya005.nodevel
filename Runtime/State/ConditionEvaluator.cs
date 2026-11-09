using System;
using System.Globalization;
namespace Junya005.Nodevel
{
    /// <summary>
    /// 変数とリテラル値、または変数同士の条件式を評価するヘルパークラス。
    /// </summary>
    public static class ConditionEvaluator
    {
        /// <summary>
        /// 変数名、演算子、右辺リテラル（または変数）を評価し、真偽値を返します。
        /// </summary>
        public static bool Evaluate(VariableStore store, string leftVarName, string op, string rightLiteral)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (string.IsNullOrEmpty(leftVarName)) throw new ArgumentException("左辺の変数名が空です。", nameof(leftVarName));

            var leftRaw = store.GetRaw(leftVarName);

            switch (leftRaw.Type)
            {
                case VariableType.Int:
                case VariableType.Float:
                    return EvaluateNumeric(leftRaw.AsFloat(), op, rightLiteral, store);

                case VariableType.Bool:
                    return EvaluateBool(leftRaw.AsBool(), op, rightLiteral);

                case VariableType.String:
                    return EvaluateString(leftRaw.AsString(), op, rightLiteral, store);

                default:
                    throw new NovelScriptException($"サポートされていない変数型 '{leftRaw.Type}' の条件評価です。");
            }
        }

        private static bool EvaluateNumeric(float leftVal, string op, string rightLiteral, VariableStore store)
        {
            float rightVal;
            if (store.Contains(rightLiteral))
            {
                rightVal = store.GetFloat(rightLiteral);
            }
            else if (!float.TryParse(rightLiteral, NumberStyles.Float, CultureInfo.InvariantCulture, out rightVal))
            {
                throw new NovelScriptException($"数値比較の右辺 '{rightLiteral}' を数値としてパースできませんでした。");
            }

            switch (op)
            {
                case "==":
                case "=":
                    return Math.Abs(leftVal - rightVal) < 0.00001f;
                case "!=":
                case "<>":
                    return Math.Abs(leftVal - rightVal) >= 0.00001f;
                case ">":
                    return leftVal > rightVal;
                case "<":
                    return leftVal < rightVal;
                case ">=":
                    return leftVal >= rightVal;
                case "<=":
                    return leftVal <= rightVal;
                default:
                    throw new NovelScriptException($"未知の比較演算子 '{op}' です。");
            }
        }

        private static bool EvaluateBool(bool leftVal, string op, string rightLiteral)
        {
            bool rightVal;
            if (string.Equals(rightLiteral, "true", StringComparison.OrdinalIgnoreCase) || rightLiteral == "1")
            {
                rightVal = true;
            }
            else if (string.Equals(rightLiteral, "false", StringComparison.OrdinalIgnoreCase) || rightLiteral == "0")
            {
                rightVal = false;
            }
            else
            {
                throw new NovelScriptException($"真偽値比較の右辺 '{rightLiteral}' が不正です。'true' または 'false' を指定してください。");
            }

            switch (op)
            {
                case "==":
                case "=":
                    return leftVal == rightVal;
                case "!=":
                case "<>":
                    return leftVal != rightVal;
                default:
                    throw new NovelScriptException($"真偽値に対して演算子 '{op}' は使用できません。'==' または '!=' を使用してください。");
            }
        }

        private static bool EvaluateString(string leftVal, string op, string rightLiteral, VariableStore store)
        {
            var rightVal = store.Contains(rightLiteral) ? store.GetString(rightLiteral) : rightLiteral;

            switch (op)
            {
                case "==":
                case "=":
                    return string.Equals(leftVal, rightVal, StringComparison.Ordinal);
                case "!=":
                case "<>":
                    return !string.Equals(leftVal, rightVal, StringComparison.Ordinal);
                default:
                    throw new NovelScriptException($"文字列に対して演算子 '{op}' は使用できません。'==' または '!=' を使用してください。");
            }
        }
    }
}

