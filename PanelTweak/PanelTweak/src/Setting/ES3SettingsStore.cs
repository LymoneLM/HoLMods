using System;
using System.Collections.Generic;
using UnityEngine;

namespace PanelTweak.Setting;

public interface ISettingsStore
{
    void Load(SettingsRegistry registry);
    void Save(SettingsRegistry registry);
    void MarkDirty();
}

public sealed class ES3SettingsStore : ISettingsStore
{
    private const string FilePath = "FW/Settings.es3";
    private const string Key = "settings_data";
    private bool _dirty;

    public void Load(SettingsRegistry registry)
    {
        if (!ES3.FileExists(FilePath))
            return;

        var dict = ES3.Load<Dictionary<string, string>>(Key, FilePath, new Dictionary<string, string>());
        foreach (var entry in registry.GetAllEntries())
        {
            if (dict.TryGetValue(entry.Id, out var stringValue))
            {
                try
                {
                    var typedValue = ParseValue(stringValue, entry.ValueType);
                    entry.TrySetValue(typedValue, out _);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Settings] Failed to parse persisted value for '{entry.Id}': {e.Message}");
                }
            }
        }

        // 保存所有未知键（卸载 Mod 后配置不丢失）
        var currentIds = new HashSet<string>();
        foreach (var e in registry.GetAllEntries()) currentIds.Add(e.Id);
        foreach (var kv in dict)
            if (!currentIds.Contains(kv.Key))
                cacheUnknownEntry(kv.Key, kv.Value);

        _dirty = false;
    }

    public void Save(SettingsRegistry registry)
    {
        var dict = new Dictionary<string, string>();
        foreach (var entry in registry.GetAllEntries())
        {
            dict[entry.Id] = StringifyValue(entry.BoxedValue);
        }
        // 附加未知条目
        foreach (var unknown in unknownEntries)
            dict[unknown.Key] = unknown.Value;

        ES3.Save<Dictionary<string, string>>(Key, dict, FilePath);
        _dirty = false;
    }

    public void MarkDirty() => _dirty = true;
    public bool IsDirty => _dirty;

    private readonly Dictionary<string, string> unknownEntries = new();

    private void cacheUnknownEntry(string id, string value)
    {
        if (!unknownEntries.ContainsKey(id))
            unknownEntries[id] = value;
    }

    private static string StringifyValue(object? value)
    {
        if (value == null) return "";
        return value is IConvertible ? Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) : value.ToString()!;
    }

    private static object ParseValue(string raw, Type type)
    {
        if (type == typeof(bool)) return bool.Parse(raw);
        if (type == typeof(int)) return int.Parse(raw);
        if (type == typeof(float)) return float.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);
        if (type == typeof(string)) return raw;
        if (type.IsEnum) return Enum.Parse(type, raw);
        if (type == typeof(KeyCode)) return (KeyCode)Enum.Parse(typeof(KeyCode), raw);
        return raw; // fallback
    }
}