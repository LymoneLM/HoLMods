using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace PanelTweak.Settings;

public static class BepInExSettingBridge
{
    /// <summary>
    /// 将一个 BepInEx ConfigEntry 注册为游戏设置系统内的设置项，
    /// 并保持双向同步。
    /// </summary>
    public static SettingEntryHandle<T> RegisterConfigEntry<T>(
        this SettingRegistry registry, string ownerId, ConfigEntry<T> configEntry,
        string tabId = null, string groupId = null,
        TextRef? displayName = null, TextRef? description = null) where T : IEquatable<T>
    {
        // 从 BepInEx AcceptableValues 推导约束提示
        IHint hint = null;

        if (configEntry.Description.AcceptableValues is AcceptableValueRange<float> floatRange)
        {
            hint = new RangeHint<float>(floatRange.MinValue, floatRange.MaxValue);
        }
        else if (configEntry.Description.AcceptableValues is AcceptableValueRange<int> intRange)
        {
            hint = new RangeHint<int>(intRange.MinValue, intRange.MaxValue);
        }
        else if (configEntry.Description.AcceptableValues is AcceptableValueList<T> list)
        {
            hint = new OptionsHint<T>(list.AcceptableValues!);
        }
        // bool 和 enum 无需 hint，Register<T> 自动推断

        var dispName = displayName ?? configEntry.Definition.Key;
        var desc = description ?? configEntry.Description.Description ?? "";

        // 使用 {Section}.{Key} 作为复合 key，保留三段式 ID：{ownerId}.{Section}.{Key}
        var compositeKey = $"{configEntry.Definition.Section}.{configEntry.Definition.Key}";

        var handle = registry.Register(
            ownerId, compositeKey, configEntry.Value,
            hint, tabId, groupId, dispName, desc);

        // 双向同步
        configEntry.SettingChanged += (_, _) =>
        {
            if (!EqualityComparer<T>.Default.Equals(handle.Value, configEntry.Value))
                handle.Value = configEntry.Value;
        };
        handle.ValueChanged += newVal =>
        {
            if (!EqualityComparer<T>.Default.Equals(configEntry.Value, newVal))
                configEntry.Value = newVal;
        };

        return handle;
    }
}
