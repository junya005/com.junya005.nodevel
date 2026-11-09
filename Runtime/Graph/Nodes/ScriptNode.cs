using System;
using UnityEngine;
namespace Junya005.Nodevel
{
    /// <summary>
    /// テキストスクリプト（セリフ・演出コマンド）を実行するノード。
    /// </summary>
    [Serializable]
    public class ScriptNode : NodeBase
    {
        [TextArea(5, 20)]
        [SerializeField] private string _scriptText;

        [NonSerialized]
        private ScriptAssetData _cachedData;

        public string ScriptText
        {
            get => _scriptText;
            set
            {
                _scriptText = value;
                _cachedData = null;
            }
        }

        public override int OutputPortCount => 1;

        /// <summary>
        /// パース済みのASTデータを取得します（必要に応じてオンデマンドパース）。
        /// </summary>
        public ScriptAssetData GetParsedData()
        {
            if (_cachedData == null)
            {
                _cachedData = ScriptParser.Parse(_scriptText);
            }
            return _cachedData;
        }
    }
}

