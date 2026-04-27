using System;
using System.Linq;
using UnityEngine;

namespace PanelTweak.Settings;

public static class Setting
{
    internal static void Initialize()
    {
        Registry = new SettingRegistry();
    }

    private static SettingRegistry Registry
    {
        get => field ?? 
               throw new InvalidOperationException("Settings system not initialized. Call Settings.Initialize() first.");
        set;
    }
    
    public static SettingBuilder For(string ownerId) => new(Registry, ownerId);
    
    public static SettingEntryHandle<bool> RegisterBool(string ownerId, string key, bool defaultValue,
        string tabId = null, string groupId = null, TextRef? displayName = null, TextRef? description = null)
        => Registry.RegisterBool(ownerId, key, defaultValue, displayName, description, tabId, groupId);

    public static SettingEntryHandle<float> RegisterFloat(string ownerId, string key, float defaultValue,
        float min = float.MinValue, float max = float.MaxValue, float step = 0f,
        string tabId = null, string groupId = null, TextRef? displayName = null, TextRef? description = null)
        => Registry.RegisterFloat(ownerId, key, defaultValue, displayName, description, tabId, groupId, min, max, step);

    public static SettingEntryHandle<int> RegisterInt(string ownerId, string key, int defaultValue,
        int min = int.MinValue, int max = int.MaxValue, int step = 1,
        string tabId = null, string groupId = null, TextRef? displayName = null, TextRef? description = null)
        => Registry.RegisterInt(ownerId, key, defaultValue, displayName, description, tabId, groupId, min, max, step);

    public static SettingEntryHandle<T> RegisterEnum<T>(string ownerId, string key, T defaultValue,
        string tabId = null, string groupId = null, TextRef? displayName = null, TextRef? description = null) where T : Enum
        => Registry.RegisterEnum(ownerId, key, defaultValue, displayName, description, tabId, groupId);

    public static SettingEntryHandle<KeyCode> RegisterKeybind(string ownerId, string key, KeyCode defaultValue,
        string tabId = null, string groupId = null, TextRef? displayName = null, TextRef? description = null)
        => Registry.RegisterKeybind(ownerId, key, defaultValue, displayName, description, tabId, groupId);
    
    public class SettingBuilder
    {
        private readonly SettingRegistry _reg;
        private readonly string _ownerId;
        private string _tabId = null!;
        private string _groupId = null!;

        internal SettingBuilder(SettingRegistry reg, string ownerId)
        {
            _reg = reg;
            _ownerId = ownerId;
        }

        public SettingBuilder Tab(string tabId, TextRef? displayName = null)
        {
            if (_reg.Tabs.All(t => t.Id != tabId))
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