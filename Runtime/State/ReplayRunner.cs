using System;
using System.Threading;
using UnityEngine;
namespace Junya005.Nodevel
{
    /// <summary>
    /// セーブデータからの復元やロールバック時に、コマンドを高速再実行（リプレイ）して状態を再現するランナー。
    /// </summary>
    public class ReplayRunner
    {
        private readonly VariableStore _variables;
        private readonly CommandRegistry _commands;
        private readonly INovelTextPresenter _textPresenter;

        public ReplayRunner(VariableStore variables, CommandRegistry commands, INovelTextPresenter textPresenter = null)
        {
            _variables = variables ?? throw new ArgumentNullException(nameof(variables));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _textPresenter = textPresenter;
        }

        /// <summary>
        /// 指定されたノード内の特定行（targetLineIndex）まで高速にコマンドを実行して状態をシミュレーションします。
        /// </summary>
        public async Awaitable FastForwardToLineAsync(NodevelGraph graph, string targetNodeId, int targetLineIndex, CancellationToken ct)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var node = graph.GetNode(targetNodeId) as ScriptNode;
            if (node == null)
            {
                throw new NovelScriptException($"リプレイ対象の ScriptNode '{targetNodeId}' がグラフ内に見つかりません。");
            }

            var data = node.GetParsedData();
            if (data?.Lines == null) return;

            var limit = Math.Min(targetLineIndex, data.Lines.Count);
            for (var i = 0; i < limit; i++)
            {
                if (ct.IsCancellationRequested) break;

                var line = data.Lines[i];
                if (line is ScriptCommandLine cmdLine)
                {
                    await _commands.DispatchAsync(cmdLine.CommandName, cmdLine.PositionalArgs, cmdLine.NamedArgs, ct);
                }
            }
        }
    }
}

