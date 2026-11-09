using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// ローカルディスクへのアトミック書き込みを提供するセーブデータストレージ実装。
    /// </summary>
    public class LocalFileSaveDataStorage : ISaveDataStorage
    {
        private readonly string _baseDirectory;

        public LocalFileSaveDataStorage(string baseDirectory = null)
        {
            _baseDirectory = string.IsNullOrEmpty(baseDirectory)
                ? Path.Combine(Application.persistentDataPath, "NodevelSaves")
                : baseDirectory;

            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }
        }

        public async Awaitable SaveAsync(string slotKey, byte[] data, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(slotKey)) throw new ArgumentException("スロットキーが空です。", nameof(slotKey));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var targetPath = GetFilePath(slotKey);
            var tempPath = targetPath + ".tmp";

            // 一時ファイルに完全書き込み
            await File.WriteAllBytesAsync(tempPath, data, ct);

            // アトミック置換
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Move(tempPath, targetPath);
        }

        public async Awaitable<byte[]> LoadAsync(string slotKey, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(slotKey)) throw new ArgumentException("スロットキーが空です。", nameof(slotKey));

            var targetPath = GetFilePath(slotKey);
            if (!File.Exists(targetPath))
            {
                return null;
            }

            return await File.ReadAllBytesAsync(targetPath, ct);
        }

        public bool Exists(string slotKey)
        {
            if (string.IsNullOrEmpty(slotKey)) return false;
            return File.Exists(GetFilePath(slotKey));
        }

        public void Delete(string slotKey)
        {
            if (string.IsNullOrEmpty(slotKey)) return;
            var targetPath = GetFilePath(slotKey);
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            var tempPath = targetPath + ".tmp";
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        private string GetFilePath(string slotKey)
        {
            return Path.Combine(_baseDirectory, $"{slotKey}.sav");
        }
    }
}

