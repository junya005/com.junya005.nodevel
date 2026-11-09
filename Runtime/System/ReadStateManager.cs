using System;
using System.Collections.Generic;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// シナリオの既読状態（ノード・行単位）を管理するマネージャー。
    /// </summary>
    public class ReadStateManager
    {
        private readonly HashSet<string> _readKeys = new HashSet<string>(StringComparer.Ordinal);

        [Serializable]
        private class SerializedData
        {
            public List<string> Keys = new List<string>();
        }

        public void MarkAsRead(string graphGuid, string nodeId, int lineIndex)
        {
            var key = MakeKey(graphGuid, nodeId, lineIndex);
            _readKeys.Add(key);
        }

        public bool IsRead(string graphGuid, string nodeId, int lineIndex)
        {
            var key = MakeKey(graphGuid, nodeId, lineIndex);
            return _readKeys.Contains(key);
        }

        public void Clear()
        {
            _readKeys.Clear();
        }

        private static string MakeKey(string graphGuid, string nodeId, int lineIndex)
        {
            return $"{graphGuid ?? ""}/{nodeId ?? ""}/{lineIndex}";
        }

        public string ToJson()
        {
            var data = new SerializedData();
            data.Keys.AddRange(_readKeys);
            return JsonUtility.ToJson(data);
        }

        public void FromJson(string json)
        {
            _readKeys.Clear();
            if (string.IsNullOrEmpty(json)) return;

            var data = JsonUtility.FromJson<SerializedData>(json);
            if (data?.Keys != null)
            {
                foreach (var k in data.Keys)
                {
                    _readKeys.Add(k);
                }
            }
        }
    }
}

