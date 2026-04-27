using System;
using System.Collections.Generic;

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