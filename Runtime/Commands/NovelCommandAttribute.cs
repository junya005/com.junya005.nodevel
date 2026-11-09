using System;
using UnityEngine.Scripting;

namespace Junya005.Nodevel
{
    /// <summary>
    /// スクリプト内のコマンド（@command）として呼び出されるメソッドに付与する属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    [Preserve]
    public class NovelCommandAttribute : PreserveAttribute
    {
        /// <summary>
        /// スクリプト内で指定されるコマンド名。
        /// </summary>
        public string CommandName { get; }

        public NovelCommandAttribute(string commandName)
        {
            CommandName = commandName ?? throw new ArgumentNullException(nameof(commandName));
        }
    }
}

