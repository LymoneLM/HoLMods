using System;
using System.Collections.Generic;

namespace PanelTweak.Settings;

internal sealed class SettingEntryImpl<T> : ISettingEntry
{
    public string Id { get; }
    public TextRef DisplayName { get; }
    public TextRef Description { get; }
    public Type ValueType => typeof(T);
    public SettingUiType UiType { get; }
    public ISettingConstraint Constraint { get; }

    private T _value;
    private readonly T _defaultValue;

    internal SettingEntryHandle<T> Handle { get; }

    public event Action<ISettingEntry> Changed;
    internal event Action<T> ValueChanged;

    public object BoxedValue
    {
        get => _value!;
        set => TrySetValue((T)value, out _);
    }

    public object BoxedDefaultValue => _defaultValue!;

    public bool IsDefault => EqualityComparer<T>.Default.Equals(_value, _defaultValue);

    public SettingEntryImpl(string id, T defaultValue, TextRef displayName, TextRef description,
        SettingUiType uiType, ISettingConstraint constraint)
    {
        Id = id;
        _defaultValue = defaultValue;
        _value = defaultValue;
        DisplayName = displayName;
        Description = description;
        UiType = uiType;
        Constraint = constraint;
        Handle = new SettingEntryHandle<T>(this);
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
            Changed?.Invoke(this);
            Handle.FireValueChanged(_value);
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

    public override string ToString() => $"[{Id}] = {_value}";
}