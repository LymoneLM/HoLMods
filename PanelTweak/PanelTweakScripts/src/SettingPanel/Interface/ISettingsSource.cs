#nullable enable
using System.Collections.Generic;

namespace PanelTweak;

public interface ISettingsSource
{
    IReadOnlyList<ISettingTab> AllTabs { get; }
    IReadOnlyList<ISettingGroup> AllGroups { get; }
    IReadOnlyList<ISettingEntry> AllSettings { get; }
    ISettingEntry? GetSetting(string id);
}