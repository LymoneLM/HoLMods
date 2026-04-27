using System;
using System.Collections.Generic;
using System.Linq;

namespace PanelTweak.Settings;

public interface IHint { }

public class RangeHint<T> : IHint
{
    public T Min { get; }
    public T Max { get; }
    public T Step { get; }

    public RangeHint(T min, T max, T step = default)
    {
        Min = min;
        Max = max;
        Step = step;
    }
}

public class OptionsHint<T> : IHint
{
    public IReadOnlyList<T> Values { get; }

    public OptionsHint(IReadOnlyList<T> values)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
        if (values.Count == 0)
            throw new ArgumentException("Options cannot be empty", nameof(values));
    }
}

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

    public static SettingEntryHandle<T> Register<T>(
        string ownerId, string key, T defaultValue,
        IHint hint = null,
        string tabId = null, string groupId = null,
        TextRef? displayName = null, TextRef? description = null)
        => Registry.Register(ownerId, key, defaultValue, hint,
            tabId, groupId, displayName, description);

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

        public SettingEntryHandle<T> Add<T>(
            string key, T defaultValue,
            IHint hint = null,
            TextRef? displayName = null, TextRef? description = null)
            => _reg.Register(_ownerId, key, defaultValue, hint,
                _tabId, _groupId, displayName, description);
    }
}
