using System;
using System.Collections.Generic;

namespace PanelTweak;

public interface ISettingConstraint
{
    Type ValueType { get; }
    object Clamp(object value);
    bool IsValid(object value);
}

public sealed class RangeConstraint : ISettingConstraint
{
    public Type ValueType { get; }
    public float Min { get; }
    public float Max { get; }
    public float Step { get; }

    public RangeConstraint(float min, float max, float step, Type valueType)
    {
        if (min > max)
            throw new ArgumentException($"Min ({min}) cannot be greater than Max ({max})");
        Min = min;
        Max = max;
        Step = step;
        ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
    }

    public bool IsValid(object value)
    {
        var f = Convert.ToSingle(value);
        return f >= Min && f <= Max;
    }

    public object Clamp(object value)
    {
        var f = Convert.ToSingle(value);
        f = Math.Max(Min, Math.Min(Max, f));
        if (Step > 0)
            f = (float)(Math.Round(f / Step) * Step);
        return Convert.ChangeType(f, ValueType);
    }
}

public sealed class SettingOption
{
    public object Value { get; }
    public TextRef Label { get; }

    public SettingOption(object value, TextRef label)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Label = label;
    }
}

public sealed class OptionsConstraint : ISettingConstraint
{
    public Type ValueType { get; }
    public IReadOnlyList<SettingOption> Options { get; }

    public OptionsConstraint(IReadOnlyList<SettingOption> options, Type valueType)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        if (options.Count == 0)
            throw new ArgumentException("Options cannot be empty", nameof(options));
        ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
    }

    public bool IsValid(object value)
    {
        foreach (var opt in Options)
            if (Equals(opt.Value, value))
                return true;
        return false;
    }

    public object Clamp(object value)
    {
        foreach (var opt in Options)
            if (Equals(opt.Value, value))
                return value;
        return Options[0].Value;
    }
}