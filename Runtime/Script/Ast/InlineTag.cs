using System;

namespace Junya005.Nodevel
{
    /// <summary>
    /// セリフ本文内に埋め込まれたインライン制御タグ（例: {wait 0.5}）。
    /// </summary>
    [Serializable]
    public struct InlineTag
    {
        /// <summary>
        /// タグ名（例: wait, speed, ruby）。
        /// </summary>
        public string TagName;

        /// <summary>
        /// タグの引数文字列（例: 0.5）。
        /// </summary>
        public string Argument;

        /// <summary>
        /// タグが挿入されるクリーンテキスト内の文字インデックス（0-based）。
        /// </summary>
        public int CharacterIndex;

        public InlineTag(string tagName, string argument, int characterIndex)
        {
            TagName = tagName;
            Argument = argument;
            CharacterIndex = characterIndex;
        }
    }
}

