using System;
using System.Collections.Generic;
using System.Linq;

namespace PanelTweak.Setting;

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
    private readonly IValueConstraint<T>? _valueConstraint;

    // 包装句柄
    internal SettingEntryHandle<T> Handle { get; }

    public event Action<ISettingEntry>? Changed;
    internal event Action<T>? ValueChanged;

    public object BoxedValue
    {
        get => _value!;
        set => TrySetValue((T)value, out _);
    }

    public object BoxedDefaultValue => _defaultValue!;

    public bool IsDefault => EqualityComparer<T>.Default.Equals(_value, _defaultValue);

    public SettingEntryImpl(string id, T defaultValue, TextRef displayName, TextRef description,
        SettingUiType uiType, ISettingConstraint? constraint, IValueConstraint<T>? valueConstraint)
    {
        Id = id;
        _defaultValue = defaultValue;
        _value = defaultValue;
        DisplayName = displayName;
        Description = description;
        UiType = uiType;
        Constraint = constraint ?? new NullConstraint();
        _valueConstraint = valueConstraint;
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
            var clamped = _valueConstraint != null ? _valueConstraint.Clamp(value) : value;
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
            error = $"Expected type {typeof(T)}, got {value?.GetType() ?? null}";
            return false;
        }

        if (_valueConstraint != null && !_valueConstraint.IsValid(typedValue, out error))
        {
            // 根据约束强制修正
            Value = typedValue;
            return true; // 已自动 Clamp，视为成功
        }

        Value = typedValue;
        return true;
    }

    public override string ToString() => $"[{Id}] = {_value}";

    // 空约束
    private sealed class NullConstraint : ISettingConstraint
    {
        public SettingConstraintType Type => SettingConstraintType.None;
    }
}

internal interface IValueConstraint<T>
{
    bool IsValid(T value, out string error);
    T Clamp(T value);
}

internal sealed class RangeValueConstraint : IValueConstraint<float>
{
    private readonly float _min, _max, _step;
    public RangeValueConstraint(float min, float max, float step)
    {
        _min = min; _max = max; _step = step;
    }
    public bool IsValid(float value, out string error)
    {
        if (value < _min || value > _max)
        {
            error = $"Value {value} out of range [{_min}, {_max}]";
            return false;
        }
        error = string.Empty;
        return true;
    }
    public float Clamp(float value)
    {
        var result = Math.Max(_min, Math.Min(_max, value));
        if (_step > 0)
            result = (float)(Math.Round(result / _step) * _step);
        return result;
    }
}

internal sealed class IntRangeValueConstraint : IValueConstraint<int>
{
    private readonly int _min, _max, _step;
    public IntRangeValueConstraint(int min, int max, int step = 1)
    {
        _min = min; _max = max; _step = step;
    }
    public bool IsValid(int value, out string error)
    {
        if (value < _min || value > _max)
        {
            error = $"Value {value} out of range [{_min}, {_max}]";
            return false;
        }
        error = string.Empty;
        return true;
    }
    public int Clamp(int value)
    {
        var result = Math.Max(_min, Math.Min(_max, value));
        if (_step > 0)
            result = (int)(Math.Round((double)result / _step) * _step);
        return result;
    }
}

internal sealed class OptionsValueConstraint<T> : IValueConstraint<T>
{
    private readonly HashSet<T> _allowedValues;
    public OptionsValueConstraint(IEnumerable<T> options)
    {
        _allowedValues = new HashSet<T>(options);
    }
    public bool IsValid(T value, out string error)
    {
        if (!_allowedValues.Contains(value))
        {
            error = $"Value {value} is not in allowed options";
            return false;
        }
        error = string.Empty;
        return true;
    }
    public T Clamp(T value) => _allowedValues.Contains(value) ? value : _allowedValues.FirstOrDefault()!;
}