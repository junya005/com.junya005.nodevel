using System;

namespace Junya005.Nodevel
{
    /// <summary>
    /// スクリプト内の1行を表す抽象基底クラス。
    /// </summary>
    [Serializable]
    public abstract class ScriptLine
    {
        /// <summary>
        /// 元テキストにおける行番号（1-based）。
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// パース前の生文字列。
        /// </summary>
        public string RawText { get; set; }
    }
}

