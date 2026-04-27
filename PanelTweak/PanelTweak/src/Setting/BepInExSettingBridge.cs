using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace PanelTweak.Setting;

public static class BepInExSettingBridge
{
    /// <summary>
    /// 将一个 BepInEx ConfigEntry 注册为游戏设置系统内的设置项，
    /// 并保持双向同步。
    /// </summary>
    public static SettingEntryHandle<T> RegisterConfigEntry<T>(
        this SettingsRegistry registry, string ownerId, ConfigEntry<T> configEntry,
        string tabId = null, string groupId = null,
        TextRef? displayName = null, TextRef? description = null) where T : IEquatable<T>
    {
        // 构造适当的约束和 UI 类型
        ISettingConstraint constraint = null;
        IValueConstraint<T> valueConstraint = null;
        var uiType = SettingUiType.Toggle; // 默认

        if (configEntry.Description.AcceptableValues is AcceptableValueRange<float> floatRange)
        {
            constraint = new RangeConstraint(floatRange.MinValue, floatRange.MaxValue);
            valueConstraint = new RangeValueConstraint(floatRange.MinValue, floatRange.MaxValue, 0) as IValueConstraint<T>;
            uiType = SettingUiType.Slider;
        }
        else if (configEntry.Description.AcceptableValues is AcceptableValueRange<int> intRange)
        {
            constraint = new RangeConstraint(intRange.MinValue, intRange.MaxValue);
            valueConstraint = new IntRangeValueConstraint(intRange.MinValue, intRange.MaxValue) as IValueConstraint<T>;
            uiType = SettingUiType.Slider;
        }
        else if (configEntry.Description.AcceptableValues is AcceptableValueList<T> list)
        {
            var options = list.AcceptableValues
                .Select(v => new SettingOption(v!, v!.ToString()))
                .ToList();
            constraint = new OptionsConstraint(options);
            valueConstraint = new OptionsValueConstraint<T>(list.AcceptableValues);
            uiType = SettingUiType.Dropdown;
        }
        else
        {
            if (typeof(T) == typeof(bool)) uiType = SettingUiType.Toggle;
            else if (typeof(T) == typeof(float) || typeof(T) == typeof(int)) uiType = SettingUiType.Slider;
            else if (typeof(T).IsEnum) uiType = SettingUiType.Dropdown;
            else uiType = SettingUiType.Toggle; // fallback
        }

        var dispName = displayName ?? configEntry.Definition.Key;
        var desc = description ?? configEntry.Description.Description ?? "";

        var id = $"{ownerId}.{configEntry.Definition.Section}.{configEntry.Definition.Key}";

        // 使用 Registry 核心方法注册
        // 由于 Register<bool> 等方法受泛型限制，这里需要使用一个内部通用方法
        // 我们先直接构造 SettingEntryImpl
        var impl = new SettingEntryImpl<T>(id, configEntry.Value, dispName, desc, uiType, constraint, valueConstraint);
        registry.Register(impl);

        var handle = impl.Handle;

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