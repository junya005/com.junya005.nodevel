using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
namespace Junya005.Nodevel
{
    /// <summary>
    /// コマンド名と実行インボーカーを管理し、ディスパッチを行うレジストリ。
    /// </summary>
    public class CommandRegistry
    {
        private readonly Dictionary<string, ICommandInvoker> _commands = new Dictionary<string, ICommandInvoker>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// オブジェクト内の [NovelCommand] 属性が付与されたすべてのメソッドをコマンドとして登録します。
        /// </summary>
        public void RegisterTarget(object target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var type = target.GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes<NovelCommandAttribute>(true);
                foreach (var attr in attrs)
                {
                    RegisterCommand(attr.CommandName, new MethodCommandInvoker(target, method));
                }
            }
        }

        /// <summary>
        /// コマンド名とインボーカーを直接登録します。
        /// </summary>
        public void RegisterCommand(string commandName, ICommandInvoker invoker)
        {
            if (string.IsNullOrEmpty(commandName)) throw new ArgumentException("コマンド名が空です。", nameof(commandName));
            if (invoker == null) throw new ArgumentNullException(nameof(invoker));

            _commands[commandName] = invoker;
        }

        public bool ContainsCommand(string commandName)
        {
            return _commands.ContainsKey(commandName);
        }

        /// <summary>
        /// 指定されたコマンドを非同期実行します。
        /// </summary>
        public async Awaitable DispatchAsync(string commandName, IReadOnlyList<string> positionalArgs, IReadOnlyDictionary<string, string> namedArgs, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                throw new NovelScriptException("ディスパッチ対象のコマンド名が空です。");
            }

            if (!_commands.TryGetValue(commandName, out var invoker))
            {
                throw new NovelScriptException($"未登録のコマンド '@{commandName}' が呼び出されました。");
            }

            await invoker.InvokeAsync(positionalArgs, namedArgs, ct);
        }

        public void Clear()
        {
            _commands.Clear();
        }
    }
}

