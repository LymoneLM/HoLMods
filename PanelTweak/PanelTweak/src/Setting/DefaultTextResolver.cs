namespace PanelTweak.Setting;

public sealed class DefaultTextResolver : ITextResolver
{
    public string Resolve(TextRef text)
    {
        return text.Kind switch
        {
            TextRefKind.Literal => text.Value,
            TextRefKind.LocalizationKey => text.Fallback ?? text.Value,
            _ => text.Value
        };
    }
}