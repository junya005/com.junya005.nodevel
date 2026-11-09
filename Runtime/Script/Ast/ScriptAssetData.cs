using System;
using System.Collections.Generic;

namespace Junya005.Nodevel
{
    /// <summary>
    /// パース済みのスクリプト行リストを保持するコンテナ。
    /// </summary>
    [Serializable]
    public class ScriptAssetData
    {
        public List<ScriptLine> Lines { get; } = new List<ScriptLine>();
    }
}

