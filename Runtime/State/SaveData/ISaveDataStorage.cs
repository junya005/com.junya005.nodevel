using System.Threading;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// セーブデータの永続化（ローカルファイル、クラウドセーブ、暗号化ストレージ等）を抽象化するインターフェース。
    /// </summary>
    public interface ISaveDataStorage
    {
        Awaitable SaveAsync(string slotKey, byte[] data, CancellationToken ct);
        Awaitable<byte[]> LoadAsync(string slotKey, CancellationToken ct);
        bool Exists(string slotKey);
        void Delete(string slotKey);
    }
}

