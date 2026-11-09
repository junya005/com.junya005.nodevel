using System;
using System.Collections.Generic;

namespace Junya005.Nodevel
{
    /// <summary>
    /// 行頭コマンド（例: @bg 'classroom' fade=0.5）を表すASTノード。
    /// </summary>
    [Serializable]
    public class ScriptCommandLine : ScriptLine
    {
        /// <summary>
        /// 実行するコマンド名（先頭の @ を除いた名称）。
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// 位置引数のリスト。
        /// </summary>
        public List<string> PositionalArgs { get; } = new List<string>();

        /// <summary>
        /// 名前付き引数のディクショナリ（key=value）。
        /// </summary>
        public Dictionary<string, string> NamedArgs { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}

