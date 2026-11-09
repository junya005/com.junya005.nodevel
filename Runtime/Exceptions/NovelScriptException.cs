using System;

namespace Junya005.Nodevel
{
    /// <summary>
    /// Nodevel スクリプトの構文解析や実行時に発生する例外。
    /// </summary>
    public class NovelScriptException : Exception
    {
        /// <summary>
        /// 発生箇所の行番号（1-based）。不明な場合は -1。
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// 発生箇所のスクリプトテキスト。
        /// </summary>
        public string RawLine { get; }

        public NovelScriptException(string message) : base(message)
        {
            LineNumber = -1;
            RawLine = null;
        }

        public NovelScriptException(string message, int lineNumber, string rawLine)
            : base($"{message} (Line {lineNumber}: '{rawLine}')")
        {
            LineNumber = lineNumber;
            RawLine = rawLine;
        }

        public NovelScriptException(string message, Exception innerException) : base(message, innerException)
        {
            LineNumber = -1;
            RawLine = null;
        }
    }
}

