using System;
using System.Linq;
using UnityEngine;

namespace PanelTweak.Setting;

public static class Setting
{
    private static SettingsRegistry? _registry;

    internal static void Initialize(SettingsRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    private static SettingsRegistry Registry
    {
        get
        {
            if (_registry == null)
                throw new InvalidOperationException("Settings system not initialized. Call Settings.Initialize(registry) first.");
            return _registry;
        }
    }

    // 为了方便，提供 For 方法返回 Builder
    public static SettingsBuilder For(string ownerId) => new(Registry, ownerId);

    // 快捷注册方法（直接传参）
    public static SettingEntryHandle<bool> RegisterBool(string ownerId, string key, bool defaultValue,
        string? tabId = null, string? groupId = null, TextRef? displayName = null, TextRef? description = null)
        => Registry.RegisterBool(ownerId, key, defaultValue, displayName, description, tabId, groupId);

    public static SettingEntryHandle<float> RegisterFloat(string ownerId, string key, float defaultValue,
        float min = float.MinValue, float max = float.MaxValue, float step = 0f,
        string? tabId = null, string? groupId = null, TextRef? displayName = null, TextRef? description = null)
        => Registry.RegisterFloat(ownerId, key, defaultValue, displayName, description, tabId, groupId, min, max, step);

    public static SettingEntryHandle<int> RegisterInt(string ownerId, string key, int defaultValue,
        int min = int.MinValue, int max = int.MaxValue, int step = 1,
        string? tabId = null, string? groupId = null, TextRef? displayName = null, TextRef? description = null)
        => Registry.RegisterInt(ownerId, key, defaultValue, displayName, description, tabId, groupId, min, max, step);

    public static SettingEntryHandle<T> RegisterEnum<T>(string ownerId, string key, T defaultValue,
        string? tabId = null, string? groupId = null, TextRef? displayName = null, TextRef? description = null) where T : Enum
        => Registry.RegisterEnum(ownerId, key, defaultValue, displayName, description, tabId, groupId);

    public static SettingEntryHandle<KeyCode> RegisterKeybind(string ownerId, string key, KeyCode defaultValue,
        string? tabId = null, string? groupId = null, TextRef? displayName = null, TextRef? description = null)
        => Registry.RegisterKeybind(ownerId, key, defaultValue, displayName, description, tabId, groupId);

    // 批量注册便捷类
    public class SettingsBuilder
    {
        private readonly SettingsRegistry _reg;
        private readonly string _ownerId;
        private string _tabId = null!;
        private string _groupId = null!;

        internal SettingsBuilder(SettingsRegistry reg, string ownerId)
        {
            _reg = reg;
            _ownerId = ownerId;
        }

        public SettingsBuilder Tab(string tabId, TextRef? displayName = null)
        {
            if (!_reg.Tabs.Any(t => t.Id == tabId))
                _reg.RegisterTab(tabId, displayName ?? TextRef.Literal(tabId));
            _tabId = tabId;
            return this;
        }

        public SettingsBuilder Group(string groupId, TextRef? displayName = null)
        {
            if (!_reg.AllGroups.Any(g => g.Id == groupId))
                _reg.RegisterGroup(groupId, displayName ?? TextRef.Literal(groupId));
            _groupId = groupId;
            return this;
        }

        public SettingEntryHandle<bool> AddBool(string key, bool defaultValue, TextRef? displayName = null, TextRef? description = null)
            => _reg.RegisterBool(_ownerId, key, defaultValue, displayName, description, _tabId, _groupId);

        public SettingEntryHandle<float> AddFloat(string key, float defaultValue, float min = float.MinValue, float max = float.MaxValue, float step = 0f, TextRef? displayName = null, TextRef? description = null)
            => _reg.RegisterFloat(_ownerId, key, defaultValue, displayName, description, _tabId, _groupId, min, max, step);

        public SettingEntryHandle<int> AddInt(string key, int defaultValue, int min = int.MinValue, int max = int.MaxValue, int step = 1, TextRef? displayName = null, TextRef? description = null)
            => _reg.RegisterInt(_ownerId, key, defaultValue, displayName, description, _tabId, _groupId, min, max, step);

        public SettingEntryHandle<T> AddEnum<T>(string key, T defaultValue, TextRef? displayName = null, TextRef? description = null) where T : Enum
            => _reg.RegisterEnum(_ownerId, key, defaultValue, displayName, description, _tabId, _groupId);

        public SettingEntryHandle<KeyCode> AddKeybind(string key, KeyCode defaultValue, TextRef? displayName = null, TextRef? description = null)
            => _reg.RegisterKeybind(_ownerId, key, defaultValue, displayName, description, _tabId, _groupId);
    }
}