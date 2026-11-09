using System;
using System.Collections.Generic;
using UnityEngine;
namespace Junya005.Nodevel
{
    /// <summary>
    /// ゲーム内の変数やフラグを管理するストア。
    /// </summary>
    public class VariableStore
    {
        private readonly Dictionary<string, VariableValue> _variables = new Dictionary<string, VariableValue>(StringComparer.Ordinal);

        [Serializable]
        private class SerializedEntry
        {
            public string Key;
            public VariableType Type;
            public long IntValue;
            public double FloatValue;
            public string StringValue;
        }

        [Serializable]
        private class SerializedData
        {
            public List<SerializedEntry> Entries = new List<SerializedEntry>();
        }

        public void Set(string name, bool value)
        {
            _variables[name] = VariableValue.Create(value);
        }

        public void Set(string name, int value)
        {
            _variables[name] = VariableValue.Create(value);
        }

        public void Set(string name, float value)
        {
            _variables[name] = VariableValue.Create(value);
        }

        public void Set(string name, string value)
        {
            _variables[name] = VariableValue.Create(value);
        }

        public void Set(string name, VariableValue value)
        {
            _variables[name] = value;
        }

        public bool GetBool(string name)
        {
            return GetRaw(name).AsBool();
        }

        public int GetInt(string name)
        {
            return GetRaw(name).AsInt();
        }

        public float GetFloat(string name)
        {
            return GetRaw(name).AsFloat();
        }

        public string GetString(string name)
        {
            return GetRaw(name).AsString();
        }

        public VariableValue GetRaw(string name)
        {
            if (_variables.TryGetValue(name, out var val))
            {
                return val;
            }
            throw new NovelScriptException($"未定義の変数 '{name}' が参照されました。");
        }

        public VariableValue Get(string name) => GetRaw(name);

        public bool Has(string name) => !string.IsNullOrEmpty(name) && _variables.ContainsKey(name);

        public bool TryGet(string name, out int value)
        {
            if (_variables.TryGetValue(name, out var v))
            {
                value = v.AsInt();
                return true;
            }
            value = 0;
            return false;
        }

        public bool TryGet(string name, out bool value)
        {
            if (_variables.TryGetValue(name, out var v))
            {
                value = v.AsBool();
                return true;
            }
            value = false;
            return false;
        }

        public bool TryGet(string name, out float value)
        {
            if (_variables.TryGetValue(name, out var v))
            {
                value = v.AsFloat();
                return true;
            }
            value = 0f;
            return false;
        }

        public bool TryGet(string name, out string value)
        {
            if (_variables.TryGetValue(name, out var v))
            {
                value = v.AsString();
                return true;
            }
            value = null;
            return false;
        }

        public void Add(string name, int delta)
        {
            var current = TryGet(name, out int val) ? val : 0;
            Set(name, current + delta);
        }

        public void Add(string name, float delta)
        {
            var current = TryGet(name, out float val) ? val : 0f;
            Set(name, current + delta);
        }

        public void Toggle(string name)
        {
            var current = TryGet(name, out bool val) && val;
            Set(name, !current);
        }

        public bool Contains(string name)
        {
            return _variables.ContainsKey(name);
        }

        public void Clear()
        {
            _variables.Clear();
        }

        public IReadOnlyDictionary<string, VariableValue> GetAll() => _variables;

        public string ToJson()
        {
            var data = new SerializedData();
            foreach (var kvp in _variables)
            {
                data.Entries.Add(new SerializedEntry
                {
                    Key = kvp.Key,
                    Type = kvp.Value.Type,
                    IntValue = kvp.Value.IntValue,
                    FloatValue = kvp.Value.FloatValue,
                    StringValue = kvp.Value.StringValue
                });
            }
            return JsonUtility.ToJson(data);
        }

        public void FromJson(string json)
        {
            _variables.Clear();
            if (string.IsNullOrEmpty(json)) return;

            var data = JsonUtility.FromJson<SerializedData>(json);
            if (data?.Entries == null) return;

            foreach (var entry in data.Entries)
            {
                _variables[entry.Key] = new VariableValue
                {
                    Type = entry.Type,
                    IntValue = entry.IntValue,
                    FloatValue = entry.FloatValue,
                    StringValue = entry.StringValue
                };
            }
        }
    }
}

