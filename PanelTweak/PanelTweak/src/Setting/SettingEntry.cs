using System;
using System.Collections.Generic;

namespace PanelTweak.Settings;

public sealed class SettingEntry<T> : ISettingEntry
{
    public string Id { get; }
    public TextRef DisplayName { get; }
    public TextRef Description { get; }
    public Type ValueType => typeof(T);
    public SettingUiType UiType { get; }
    public ISettingConstraint Constraint { get; }

    private T _value;
    private readonly T _defaultValue;

    public event Action<T> ValueChanged;

    public object BoxedValue
    {
        get => _value!;
        set => TrySetValue((T)value, out _);
    }

    public object BoxedDefaultValue => _defaultValue!;

    public bool IsDefault => EqualityComparer<T>.Default.Equals(_value, _defaultValue);

    internal SettingEntry(string id, T defaultValue, TextRef displayName, TextRef description,
        SettingUiType uiType, ISettingConstraint constraint)
    {
        Id = id;
        _defaultValue = defaultValue;
        _value = defaultValue;
        DisplayName = displayName;
        Description = description;
        UiType = uiType;
        Constraint = constraint;
    }

    public void ResetToDefault()
    {
        Value = _defaultValue;
    }

    public T Value
    {
        get => _value;
        set
        {
            var clamped = Constraint != null ? (T)Constraint.Clamp(value) : value;
            if (EqualityComparer<T>.Default.Equals(_value, clamped))
                return;

            _value = clamped;
            ValueChanged?.Invoke(_value);
        }
    }

    public bool TrySetValue(object value, out string error)
    {
        error = string.Empty;
        if (value is not T typedValue)
        {
            error = $"Expected type {typeof(T)}, got {value?.GetType()}";
            return false;
        }

        Value = typedValue;
        return true;
    }
}

internal sealed class SettingTab(string id, TextRef displayName) : ISettingTab
{
    public string Id { get; } = id;
    public TextRef DisplayName { get; } = displayName;
}

internal sealed class SettingGroup(string id, TextRef displayName) : ISettingGroup
{
    public string Id { get; } = id;
    public TextRef DisplayName { get; } = displayName;
}