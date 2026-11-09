using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// 選択肢一覧を表示し、プレイヤーの選択結果を受け付けるView層インターフェース。
    /// </summary>
    public interface INovelChoicePresenter
    {
        /// <summary>
        /// 選択肢リストを表示し、プレイヤーが選択したインデックス（0-based）を返します。
        /// </summary>
        Awaitable<int> ShowChoicesAsync(IReadOnlyList<string> choiceTexts, CancellationToken ct);
    }
}

