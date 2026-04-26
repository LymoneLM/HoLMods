using System;

namespace PanelTweak;

public static class SettingManager
{
    private static ISettingsSource? _source;
    private static ITextResolver? _textResolver;

    internal static bool IsInitialized => _source != null;

    public static ISettingsSource Source
    {
        get => _source ?? throw new InvalidOperationException("SettingManager is not initialized. Call Initialize() first.");
    }

    public static ITextResolver TextResolver
    {
        get => _textResolver ?? throw new InvalidOperationException("TextResolver is not initialized.");
    }

    public static event Action? LanguageChanged;

    internal static void Initialize(ISettingsSource source, ITextResolver resolver)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _textResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    /// <summary>
    /// 当游戏语言变化时由 Plugin 层调用，触发 UI 刷新文本。
    /// </summary>
    internal static void NotifyLanguageChanged()
    {
        LanguageChanged?.Invoke();
    }
}