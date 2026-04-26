using System;
using System.Collections.Generic;
using UnityEngine;

namespace PanelTweak.Setting;

/// <summary>
/// 负责在设置系统和 Mainload.SetData 数组之间同步值。
/// 需要配置 ID 到 SetData 索引的映射表。
/// </summary>
internal static class MainloadSetDataSync
{
    private static bool _isSyncing;
    private static Dictionary<string, int> _idToIndex;
    private static Dictionary<int, string> _indexToId;

    public static void Initialize(Dictionary<string, int> mapping)
    {
        _idToIndex = mapping ?? throw new ArgumentNullException(nameof(mapping));
        _indexToId = new Dictionary<int, string>();
        foreach (var kv in _idToIndex)
            _indexToId[kv.Value] = kv.Key;
    }

    // 设置变更 → SetData
    public static void PushToSetData(ISettingEntry entry)
    {
        if (_isSyncing || _idToIndex == null) return;
        if (!_idToIndex.TryGetValue(entry.Id, out var index)) return;

        _isSyncing = true;
        try
        {
            var raw = entry.BoxedValue;
            var intValue = raw switch
            {
                bool b => b ? 1 : 0,
                int i => i,
                float f => Mathf.RoundToInt(f * 100), // 假设音量等 float 是 0–1，对应 SetData 0–100
                KeyCode k => (int)k,
                _ => Convert.ToInt32(raw)
            };
            Mainload.SetData[index] = intValue;
        }
        finally
        {
            _isSyncing = false;
        }
    }

    // 从 SetData 拉取并更新设置（用于存档加载）
    public static void PullFromSetData(SettingsRegistry registry)
    {
        if (_isSyncing || _indexToId == null) return;
        _isSyncing = true;
        try
        {
            foreach (var kv in _indexToId)
            {
                var index = kv.Key;
                var id = kv.Value;
                var entry = registry.GetSetting(id);
                if (entry == null || index >= Mainload.SetData.Count) continue;

                var rawValue = Mainload.SetData[index];
                var value = rawValue switch
                {
                    _ when entry.ValueType == typeof(bool) => rawValue != 0,
                    _ when entry.ValueType == typeof(int) => rawValue,
                    _ when entry.ValueType == typeof(float) => rawValue / 100f,
                    _ when entry.ValueType == typeof(KeyCode) => (KeyCode)rawValue,
                    _ when entry.ValueType.IsEnum => Enum.ToObject(entry.ValueType, rawValue),
                    _ => rawValue
                };
                entry.TrySetValue(value, out _);
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private static object ConvertToType(string raw, Type targetType)
    {
        if (targetType == typeof(int)) return int.Parse(raw);
        if (targetType == typeof(float)) return float.Parse(raw);
        if (targetType == typeof(bool)) return bool.Parse(raw);
        if (targetType == typeof(string)) return raw;
        if (targetType.IsEnum) return Enum.Parse(targetType, raw);
        return raw;
    }
}