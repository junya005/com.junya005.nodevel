using System;
using System.Collections.Generic;

namespace Junya005.Nodevel
{
    [Serializable]
    public struct BacklogEntry
    {
        public string Speaker;
        public string Text;
        public string TextId;
        public string VoiceId;

        public BacklogEntry(string speaker, string text, string textId = null, string voiceId = null)
        {
            Speaker = speaker;
            Text = text;
            TextId = textId;
            VoiceId = voiceId;
        }
    }

    /// <summary>
    /// 会話ログ（バックログ）の履歴を管理するマネージャー。
    /// </summary>
    public class BacklogManager
    {
        private readonly int _maxEntries;
        private readonly List<BacklogEntry> _entries = new List<BacklogEntry>();

        public BacklogManager(int maxEntries = 100)
        {
            _maxEntries = Math.Max(1, maxEntries);
        }

        public void Add(string speaker, string text, string textId = null, string voiceId = null)
        {
            if (_entries.Count >= _maxEntries)
            {
                _entries.RemoveAt(0);
            }
            _entries.Add(new BacklogEntry(speaker, text, textId, voiceId));
        }

        public IReadOnlyList<BacklogEntry> GetEntries() => _entries;

        public void Clear()
        {
            _entries.Clear();
        }
    }
}

