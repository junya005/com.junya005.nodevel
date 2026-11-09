using System;
using System.Collections.Generic;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// 複数の選択肢をプレイヤーに提示し、選択結果に応じて分岐するノード。
    /// </summary>
    [Serializable]
    public class ChoiceNode : NodeBase
    {
        [SerializeField] private List<string> _choices = new List<string>();

        public List<string> Choices => _choices;

        public override int OutputPortCount => _choices != null ? Math.Max(1, _choices.Count) : 1;
    }
}

