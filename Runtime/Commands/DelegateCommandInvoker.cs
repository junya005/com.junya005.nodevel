using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// デリゲート関数から直接生成可能なコマンドインボーカー。
    /// </summary>
    public class DelegateCommandInvoker : ICommandInvoker
    {
        private readonly Func<IReadOnlyList<string>, IReadOnlyDictionary<string, string>, CancellationToken, Awaitable> _handler;

        public DelegateCommandInvoker(Func<IReadOnlyList<string>, IReadOnlyDictionary<string, string>, CancellationToken, Awaitable> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Awaitable InvokeAsync(IReadOnlyList<string> positionalArgs, IReadOnlyDictionary<string, string> namedArgs, CancellationToken ct)
        {
            return _handler(positionalArgs, namedArgs, ct);
        }
    }
}

