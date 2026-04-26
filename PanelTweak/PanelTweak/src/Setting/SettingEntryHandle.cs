using System;

namespace PanelTweak.Setting;

/// <summary>
/// 类型安全的设置项句柄，提供给模组开发者使用。
/// 通过 Value 属性读写当前值，支持值变更事件。
/// </summary>
public sealed class SettingEntryHandle<T>
{
    private readonly SettingEntryImpl<T> _impl;

    internal SettingEntryHandle(SettingEntryImpl<T> impl) => _impl = impl;

    public string Id => _impl.Id;
    public T Value { get => _impl.Value; set => _impl.Value = value; }
    public T DefaultValue => (T)_impl.BoxedDefaultValue;
    public bool IsDefault => _impl.IsDefault;
    public void ResetToDefault() => _impl.ResetToDefault();

    /// <summary>
    /// 值发生变化时触发（传入新值）。
    /// </summary>
    public event Action<T>? ValueChanged;

    internal void FireValueChanged(T newValue) => ValueChanged?.Invoke(newValue);
}