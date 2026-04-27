using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace PanelTweak.Settings;

public static class Setting
{
    internal static void Initialize()
    {
        Registry = new SettingRegistry();
    }

    internal static SettingRegistry Registry
    {
        get => field ??
               throw new InvalidOperationException("Settings system not initialized.");
        set;
    }

    public static SettingBuilder For(string @namespace) => new(Registry, @namespace);

    public static SettingEntry<T> Register<T>(
        string @namespace, string key, T defaultValue,
        IHint hint = null,
        string tabId = null, string groupId = null,
        TextRef? displayName = null, TextRef? description = null)
        => Registry.Register(@namespace, key, defaultValue, hint,
            tabId, groupId, displayName, description);

    public class SettingBuilder
    {
        private readonly SettingRegistry _reg;
        private readonly string _namespace;
        private string _tabId = null!;
        private string _groupId = null!;

        internal SettingBuilder(SettingRegistry reg, string @namespace)
        {
            _reg = reg;
            _namespace = @namespace;
        }

        public SettingBuilder Tab(string tabId, TextRef? displayName = null)
        {
            if (_reg.AllTabs.All(t => t.Id != tabId))
                _reg.RegisterTab(tabId, displayName ?? tabId);
            _tabId = tabId;
            return this;
        }

        public SettingBuilder Group(string groupId, TextRef? displayName = null)
        {
            if (_reg.AllGroups.All(g => g.Id != groupId))
                _reg.RegisterGroup(groupId, displayName ?? groupId);
            _groupId = groupId;
            return this;
        }

        public SettingEntry<T> Add<T>(
            string key, T defaultValue, IHint hint = null,
            TextRef? displayName = null, TextRef? description = null)
            => _reg.Register(_namespace, key, defaultValue, hint,
                _tabId, _groupId, displayName, description);

        public SettingEntry<T> Add<T>(ConfigEntry<T> configEntry,
            TextRef? displayName = null, TextRef? description = null) where T : IEquatable<T>
            => RegisterConfigEntry(_namespace, configEntry,
                _tabId, _groupId, displayName, description);
    }
    
    /// <summary>
    /// 将一个 BepInEx ConfigEntry 注册为游戏设置系统内的设置项，
    /// 并保持双向同步。
    /// </summary>
    public static SettingEntry<T> RegisterConfigEntry<T>(
        string @namespace, ConfigEntry<T> configEntry,
        string tabId = null, string groupId = null,
        TextRef? displayName = null, TextRef? description = null) where T : IEquatable<T>
    {
        IHint hint = configEntry.Description.AcceptableValues switch
        {
            AcceptableValueRange<float> floatRange => new RangeHint<float>(floatRange.MinValue, floatRange.MaxValue),
            AcceptableValueRange<int> intRange => new RangeHint<int>(intRange.MinValue, intRange.MaxValue),
            AcceptableValueList<T> list => new OptionsHint<T>(list.AcceptableValues!),
            _ => null
        };

        var entry = Registry.Register(
            @namespace, $"{configEntry.Definition.Section}.{configEntry.Definition.Key}", 
            configEntry.Value, hint, tabId, groupId, 
            displayName ?? configEntry.Definition.Key, 
            description ?? configEntry.Description.Description ?? "");

        configEntry.SettingChanged += (_, _) =>
        {
            if (!EqualityComparer<T>.Default.Equals(entry.Value, configEntry.Value))
                entry.Value = configEntry.Value;
        };
        
        entry.ValueChanged += newVal =>
        {
            if (!EqualityComparer<T>.Default.Equals(configEntry.Value, newVal))
                configEntry.Value = newVal;
        };

        return entry;
    }
}
