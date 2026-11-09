using System;
using UnityEngine;

namespace Junya005.Nodevel
{
    /// <summary>
    /// セーブデータとして永続化される状態スナップショット。
    /// </summary>
    [Serializable]
    public class SaveDataSnapshot
    {
        public int Version = 1;
        public long Timestamp;
        public string GraphGuid;
        public string NodeId;
        public int LineIndex;
        public string VariablesJson;
        public string ReadStateJson;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static SaveDataSnapshot FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<SaveDataSnapshot>(json);
        }
    }
}

