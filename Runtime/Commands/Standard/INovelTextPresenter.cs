using System.Threading;
using UnityEngine;
namespace Junya005.Nodevel
{
    /// <summary>
    /// メッセージウィンドウでのセリフ表示・タイプライター・クリック待ちを行うView層インターフェース。
    /// </summary>
    public interface INovelTextPresenter
    {
        /// <summary>
        /// セリフを表示し、クリック待ち（またはオート進行待機）完了まで非同期待機します。
        /// </summary>
        Awaitable ShowTextAsync(ScriptDialogueLine line, CancellationToken ct);

        /// <summary>
        /// メッセージウィンドウをクリアまたは非表示にします。
        /// </summary>
        Awaitable ClearAsync(CancellationToken ct);
    }
}

