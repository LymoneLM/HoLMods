using System;
using System.Collections.Generic;
using System.Linq;

namespace PanelTweak.Settings;

internal sealed class SettingRegistry : ISettingsSource
{
    private readonly Dictionary<string, SettingTab> _tabs = new();
    private readonly Dictionary<string, SettingGroup> _groups = new();
    private readonly Dictionary<string, ISettingEntry> _entries = new();

    private readonly List<SettingTab> _tabList = new();
    private readonly List<SettingGroup> _groupList = new();
    private readonly List<ISettingEntry> _entryList = new();

    public const string ModsTabId = "mods";
    public const string ControlsTabId = "controls";

    public ISettingsStore Store { get; set; }

    internal SettingRegistry()
    {
        RegisterTab(ModsTabId, TextRef.Key("settings.tab.mods", "Mod Settings"));
        RegisterTab(ControlsTabId, TextRef.Key("settings.tab.controls", "Controls"));
    }

    public IReadOnlyList<ISettingTab> Tabs => _tabList.AsReadOnly();
    public IReadOnlyList<ISettingGroup> AllGroups => _groupList.AsReadOnly();
    public IReadOnlyList<ISettingEntry> GetAllSettings() => _entryList.AsReadOnly();
    public ISettingEntry GetSetting(string id) => _entries.TryGetValue(id, out var e) ? e : null;

    public void RegisterTab(string tabId, TextRef displayName)
    {
        EnsureNotSealed();
        if (_tabs.ContainsKey(tabId))
            throw new InvalidOperationException($"Tab '{tabId}' already registered.");
        
        var tab = new SettingTab(tabId, displayName);
        _tabs[tabId] = tab;
        _tabList.Add(tab);
    }

    public void RegisterGroup(string groupId, TextRef displayName)
    {
        EnsureNotSealed();
        if (_groups.ContainsKey(groupId))
            throw new InvalidOperationException($"Group '{groupId}' already registered.");
        
        var group = new SettingGroup(groupId, displayName);
        _groups[groupId] = group;
        _groupList.Add(group);
    }

    public void Register(ISettingEntry entry)
    {
        EnsureNotSealed();
        if (_entries.ContainsKey(entry.Id))
            throw new InvalidOperationException($"Duplicate setting id: {entry.Id}");
        _entries[entry.Id] = entry;
        _entryList.Add(entry);
    }

    public SettingEntry<T> Register<T>(
        string @namespace, string key, T defaultValue,
        IHint hint = null,
        string tabId = null, string groupId = null,
        TextRef? displayName = null, TextRef? description = null)
    {
        EnsureNotSealed();
        if (string.IsNullOrEmpty(@namespace))
            throw new ArgumentNullException(nameof(@namespace));
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var id = $"{@namespace}.{key}";

        // 自动推断 UI 类型和约束
        SettingUiType uiType;
        ISettingConstraint constraint = null;

        if (typeof(T) == typeof(bool))
        {
            uiType = SettingUiType.Toggle;
        }
        else if (hint is RangeHint<T> range)
        {
            var min = Convert.ToSingle(range.Min);
            var max = Convert.ToSingle(range.Max);
            var step = Convert.ToSingle(range.Step);
            constraint = new RangeConstraint(min, max, step, typeof(T));
            uiType = SettingUiType.Slider;
        }
        else if (hint is OptionsHint<T> options)
        {
            var optsList = options.Values
                .Select(v => new SettingOption(v!, v!.ToString()))
                .ToList();
            constraint = new OptionsConstraint(optsList, typeof(T));
            uiType = SettingUiType.Dropdown;
        }
        else if (typeof(T).IsEnum)
        {
            var optsList = Enum.GetValues(typeof(T)).Cast<object>()
                .Select(v => new SettingOption(v, v.ToString()))
                .ToList();
            constraint = new OptionsConstraint(optsList, typeof(T));
            uiType = SettingUiType.Dropdown;
        }
        else
        {
            throw new ArgumentException(
                $"Cannot infer UI type for '{typeof(T).Name}'. " +
                "Provide a RangeHint, OptionsHint, or use bool/enum.");
        }

        // 自动推导 Tab
        if (string.IsNullOrEmpty(tabId))
        {
            tabId = ModsTabId;
        }
        else
        {
            if (!_tabs.ContainsKey(tabId))
                RegisterTab(tabId, tabId);
        }

        // 自动 Group
        if (string.IsNullOrEmpty(groupId))
            groupId = "general";
        if (!_groups.ContainsKey(groupId))
            RegisterGroup(groupId, groupId);

        // TODO: 处理Entry与Tab和Group的绑定
        var entry = new SettingEntry<T>(
            id, defaultValue,
            displayName ?? key, description ?? "",
            uiType, constraint);
        Register(entry);
        return entry;
    }

    #region Registry State Machine

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

    #endregion
    
    internal IEnumerable<ISettingEntry> GetAllEntries() => _entryList;
    

}