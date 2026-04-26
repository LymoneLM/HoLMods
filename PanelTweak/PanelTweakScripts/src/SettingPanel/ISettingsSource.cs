using System.Collections.Generic;

namespace PanelTweak;

public interface ISettingsSource
{
    IReadOnlyList<ISettingTab> Tabs { get; }
    IReadOnlyList<ISettingGroup> AllGroups { get; }
    IReadOnlyList<ISettingEntry> GetAllSettings();
    ISettingEntry? GetSetting(string id);
}