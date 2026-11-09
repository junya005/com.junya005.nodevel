using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// パース済み引数を受け取ってコマンドを実行するインボーカーのインターフェース。
    /// </summary>
    public interface ICommandInvoker
    {
        Awaitable InvokeAsync(IReadOnlyList<string> positionalArgs, IReadOnlyDictionary<string, string> namedArgs, CancellationToken ct);
    }
}

