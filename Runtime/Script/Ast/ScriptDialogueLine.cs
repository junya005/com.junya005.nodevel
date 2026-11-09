using System;
using System.Collections.Generic;

namespace Junya005.Nodevel
{
    /// <summary>
    /// セリフ行またはナレーション（地の文）を表すASTノード。
    /// </summary>
    [Serializable]
    public class ScriptDialogueLine : ScriptLine
    {
        /// <summary>
        /// 発言者名。地の文の場合は null。
        /// </summary>
        public string Speaker { get; set; }

        /// <summary>
        /// インラインタグを含む元のセリフテキスト。
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// インラインタグを除去した表示用のクリーンテキスト。
        /// </summary>
        public string CleanText { get; set; }

        /// <summary>
        /// セリフ内に埋め込まれたインラインタグのリスト。
        /// </summary>
        public List<InlineTag> InlineTags { get; } = new List<InlineTag>();

        /// <summary>
        /// 多言語化・ボイス管理用の一意なIDキー。
        /// </summary>
        public string TextId { get; set; }
    }
}

