using System;

namespace PanelTweak;

public static class SettingManager
{
    public static ISettingsSource Source
    {
        get => field ?? throw new InvalidOperationException("SettingManager is not initialized.");
        set;
    }

    public static ITextResolver TextResolver
    {
        get => field ?? throw new InvalidOperationException("SettingManager is not initialized.");
        set;
    } = new DefaultTextResolver();
    
    internal static bool IsInitialized => Source != null;

    public static void Initialize(ISettingsSource source, ITextResolver resolver)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        TextResolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }
    
    public static event Action LanguageChanged;
    
    internal static void NotifyLanguageChanged()
    {
        LanguageChanged?.Invoke();
    }
}

public sealed class DefaultTextResolver : ITextResolver
{
    public string Resolve(TextRef text)
    {
        if (text.LocalizationKey != null)
            return text.Literal ?? text.LocalizationKey;
        return text.Literal ?? "";
    }
}