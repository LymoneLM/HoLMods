using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PanelTweak.Settings;

public sealed class SettingRegistry : ISettingsSource
{
    private readonly Dictionary<string, SettingTabImpl> _tabs = new();
    private readonly Dictionary<string, SettingGroupImpl> _groups = new();
    private readonly Dictionary<string, ISettingEntry> _entries = new();

    private readonly List<SettingTabImpl> _tabList = new();
    private readonly List<SettingGroupImpl> _groupList = new();
    private readonly List<ISettingEntry> _entryList = new();

    // 默认 Tab ID
    public const string ModsTabId = "mods";
    public const string ControlsTabId = "controls";

    public ISettingsStore Store { get; set; }

    internal SettingRegistry()
    {
        RegisterTabInternal(ModsTabId, TextRef.Key("settings.tab.mods", "Mod Settings"));
        RegisterTabInternal(ControlsTabId, TextRef.Key("settings.tab.controls", "Controls"));
    }

    // ---------- 公开接口（供面板或其他内部消费者） ----------
    public IReadOnlyList<ISettingTab> Tabs => _tabList.AsReadOnly();
    public IReadOnlyList<ISettingGroup> AllGroups => _groupList.AsReadOnly();
    public IReadOnlyList<ISettingEntry> GetAllSettings() => _entryList.AsReadOnly();
    public ISettingEntry GetSetting(string id) => _entries.TryGetValue(id, out var e) ? e : null;

    // ---------- 注册 Tab / Group（支持显式注册） ----------
    public void RegisterTab(string tabId, TextRef displayName)
    {
        EnsureNotSealed();
        if (_tabs.ContainsKey(tabId))
            throw new InvalidOperationException($"Tab '{tabId}' already registered.");
        RegisterTabInternal(tabId, displayName);
    }

    public void RegisterGroup(string groupId, TextRef displayName)
    {
        EnsureNotSealed();
        if (_groups.ContainsKey(groupId))
            throw new InvalidOperationException($"Group '{groupId}' already registered.");
        RegisterGroupInternal(groupId, displayName);
    }

    private void RegisterTabInternal(string tabId, TextRef displayName)
    {
        var tab = new SettingTabImpl(tabId, displayName);
        _tabs[tabId] = tab;
        _tabList.Add(tab);
    }

    private void RegisterGroupInternal(string groupId, TextRef displayName)
    {
        var group = new SettingGroupImpl(groupId, displayName);
        _groups[groupId] = group;
        _groupList.Add(group);
    }

    // ---------- 注册设置 ----------
    internal void Register(ISettingEntry entry)
    {
        EnsureNotSealed();
        if (_entries.ContainsKey(entry.Id))
            throw new InvalidOperationException($"Duplicate setting id: {entry.Id}");
        _entries[entry.Id] = entry;
        _entryList.Add(entry);
    }
    
    internal SettingEntryHandle<bool> RegisterBool(
        string ownerId, string key, bool defaultValue,
        TextRef? displayName = null, TextRef? description = null,
        string tabId = null, string groupId = null)
    {
        return Register(ownerId, key, defaultValue, displayName, description,
            tabId, groupId, SettingUiType.Toggle, constraint: null);
    }

    internal SettingEntryHandle<float> RegisterFloat(
        string ownerId, string key, float defaultValue,
        TextRef? displayName = null, TextRef? description = null,
        string tabId = null, string groupId = null,
        float min = float.MinValue, float max = float.MaxValue, float step = 0f)
    {
        var range = new RangeConstraint(min, max, step, typeof(float));
        return Register(ownerId, key, defaultValue, displayName, description,
            tabId, groupId, SettingUiType.Slider, range);
    }

    internal SettingEntryHandle<int> RegisterInt(
        string ownerId, string key, int defaultValue,
        TextRef? displayName = null, TextRef? description = null,
        string tabId = null, string groupId = null,
        int min = int.MinValue, int max = int.MaxValue, int step = 1)
    {
        var range = new RangeConstraint(min, max, step, typeof(int));
        return Register(ownerId, key, defaultValue, displayName, description,
            tabId, groupId, SettingUiType.Slider, range);
    }

    internal SettingEntryHandle<T> RegisterEnum<T>(
        string ownerId, string key, T defaultValue,
        TextRef? displayName = null, TextRef? description = null,
        string tabId = null, string groupId = null) where T : Enum
    {
        var options = Enum.GetValues(typeof(T)).Cast<T>()
            .Select(v => new SettingOption(v, v.ToString())).ToList();
        var constraint = new OptionsConstraint(options, typeof(T));
        return Register(ownerId, key, defaultValue, displayName, description,
            tabId, groupId, SettingUiType.Dropdown, constraint);
    }

    internal SettingEntryHandle<KeyCode> RegisterKeybind(
        string ownerId, string key, KeyCode defaultValue,
        TextRef? displayName = null, TextRef? description = null,
        string tabId = null, string groupId = null)
    {
        return Register(ownerId, key, defaultValue, displayName, description,
            tabId ?? ControlsTabId, groupId, SettingUiType.Keybind, null);
    }

    // 核心泛型注册方法
    private SettingEntryHandle<T> Register<T>(
        string ownerId, string key, T defaultValue,
        TextRef? displayName, TextRef? description,
        string tabId, string groupId,
        SettingUiType uiType, ISettingConstraint? constraint)
    {
        EnsureNotSealed();
        if (string.IsNullOrEmpty(ownerId))
            throw new ArgumentNullException(nameof(ownerId));
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var id = $"{ownerId}.{key}";

        // 自动推导 Tab
        if (string.IsNullOrEmpty(tabId))
        {
            tabId = uiType == SettingUiType.Keybind ? ControlsTabId : ModsTabId;
        }
        else
        {
            if (!_tabs.ContainsKey(tabId))
                RegisterTabInternal(tabId, tabId); // 如果未注册则自动创建，降级 display name
        }

        // 自动 Group
        if (string.IsNullOrEmpty(groupId))
            groupId = "general";
        if (!_groups.ContainsKey(groupId))
            RegisterGroupInternal(groupId, groupId);

        var impl = new SettingEntryImpl<T>(
            id, defaultValue,
            displayName ?? key, description ?? "",
            uiType, constraint);
        Register(impl);
        return impl.Handle;
    }

    // ---------- 注册状态控制 ----------
    internal bool IsSealed { get; private set; }
    
    internal void Seal()
    {
        IsSealed = true;
        // TODO: 持久化数据引入逻辑
        Store?.Load(this);
    }

    private void EnsureNotSealed()
    {
        if (IsSealed)
            throw new InvalidOperationException("Settings registry is sealed. Cannot register new entries after Start.");
    }

    // ---------- 内部数据对象 ----------
    internal IEnumerable<ISettingEntry> GetAllEntries() => _entryList;

    // 内部 Tab/Group 实现
    private sealed class SettingTabImpl(string id, TextRef displayName) : ISettingTab
    {
        public string Id { get; } = id;
        public TextRef DisplayName { get; } = displayName;
    }

    private sealed class SettingGroupImpl(string id, TextRef displayName) : ISettingGroup
    {
        public string Id { get; } = id;
        public TextRef DisplayName { get; } = displayName;
    }
}