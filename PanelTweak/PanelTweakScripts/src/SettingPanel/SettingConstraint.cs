using System;
using System.Collections.Generic;

namespace PanelTweak;

public enum SettingConstraintType
{
    None,
    Range,
    Options
}

public interface ISettingConstraint
{
    SettingConstraintType Type { get; }
}

public sealed class RangeConstraint : ISettingConstraint
{
    public SettingConstraintType Type => SettingConstraintType.Range;
    public float Min { get; }
    public float Max { get; }
    public float Step { get; }

    public RangeConstraint(float min, float max, float step = 0f)
    {
        if (min > max)
            throw new ArgumentException($"Min ({min}) cannot be greater than Max ({max})");
        Min = min;
        Max = max;
        Step = step;
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
    public SettingConstraintType Type => SettingConstraintType.Options;
    public IReadOnlyList<SettingOption> Options { get; }

    public OptionsConstraint(IReadOnlyList<SettingOption> options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        if (options.Count == 0)
            throw new ArgumentException("Options cannot be empty", nameof(options));
    }
}