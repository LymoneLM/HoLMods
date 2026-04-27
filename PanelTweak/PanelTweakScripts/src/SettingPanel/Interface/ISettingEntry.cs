#nullable enable
using System;

namespace PanelTweak;

public interface ISettingEntry
{
    string Id { get; }
    TextRef DisplayName { get; }
    TextRef Description { get; }
    Type ValueType { get; }
    object BoxedValue { get; set; }
    object BoxedDefaultValue { get; }
    bool IsDefault { get; }
    SettingUiType UiType { get; }
    ISettingConstraint? Constraint { get; }
    void ResetToDefault();
    bool TrySetValue(object value, out string error);
}

public interface ISettingGroup
{
    string Id { get; }
    TextRef DisplayName { get; }
}

public interface ISettingTab
{
    string Id { get; }
    TextRef DisplayName { get; }
}