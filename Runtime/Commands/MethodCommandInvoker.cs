using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using UnityEngine;
namespace Junya005.Nodevel
{
    /// <summary>
    /// リフレクションで発見されたメソッドのメタデータを事前キャッシュし、高速に呼び出すインボーカー。
    /// </summary>
    public class MethodCommandInvoker : ICommandInvoker
    {
        private readonly object _target;
        private readonly MethodInfo _method;
        private readonly ParameterInfo[] _parameters;
        private readonly bool _isAwaitable;

        public MethodCommandInvoker(object target, MethodInfo method)
        {
            _target = target;
            _method = method;
            _parameters = method.GetParameters();
            _isAwaitable = method.ReturnType == typeof(Awaitable);
        }

        public async Awaitable InvokeAsync(IReadOnlyList<string> positionalArgs, IReadOnlyDictionary<string, string> namedArgs, CancellationToken ct)
        {
            var callArgs = new object[_parameters.Length];
            var positionalIndex = 0;

            for (var i = 0; i < _parameters.Length; i++)
            {
                var param = _parameters[i];
                var paramType = param.ParameterType;

                // CancellationToken の自動注入
                if (paramType == typeof(CancellationToken))
                {
                    callArgs[i] = ct;
                    continue;
                }

                // 名前付き引数からの解決
                if (namedArgs != null && namedArgs.TryGetValue(param.Name, out var namedValue))
                {
                    callArgs[i] = ConvertValue(namedValue, paramType, param.Name);
                    continue;
                }

                // 位置引数からの解決
                if (positionalArgs != null && positionalIndex < positionalArgs.Count)
                {
                    var posValue = positionalArgs[positionalIndex++];
                    callArgs[i] = ConvertValue(posValue, paramType, param.Name);
                    continue;
                }

                // デフォルト値の解決
                if (param.HasDefaultValue)
                {
                    callArgs[i] = param.DefaultValue;
                    continue;
                }

                throw new NovelScriptException($"コマンド '{_method.Name}' の必須パラメータ '{param.Name}' が指定されていません。");
            }

            try
            {
                var result = _method.Invoke(_target, callArgs);
                if (_isAwaitable && result is Awaitable awaitable)
                {
                    await awaitable;
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    throw new NovelScriptException($"コマンド '{_method.Name}' の実行中に例外が発生しました: {ex.InnerException.Message}", ex.InnerException);
                }
                throw;
            }
        }

        private static object ConvertValue(string raw, Type targetType, string paramName)
        {
            if (targetType == typeof(string))
            {
                return raw;
            }
            if (targetType == typeof(int))
            {
                if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                {
                    return i;
                }
                throw new NovelScriptException($"パラメータ '{paramName}' の値 '{raw}' を int に変換できませんでした。");
            }
            if (targetType == typeof(float))
            {
                if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                {
                    return f;
                }
                throw new NovelScriptException($"パラメータ '{paramName}' の値 '{raw}' を float に変換できませんでした。");
            }
            if (targetType == typeof(bool))
            {
                if (bool.TryParse(raw, out var b))
                {
                    return b;
                }
                if (raw == "1") return true;
                if (raw == "0") return false;
                throw new NovelScriptException($"パラメータ '{paramName}' の値 '{raw}' を bool に変換できませんでした。");
            }
            if (targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, raw, true);
                }
                catch
                {
                    throw new NovelScriptException($"パラメータ '{paramName}' の値 '{raw}' は enum '{targetType.Name}' の有効な値ではありません。");
                }
            }

            throw new NovelScriptException($"未対応のパラメータ型 '{targetType.Name}' です。");
        }
    }
}

