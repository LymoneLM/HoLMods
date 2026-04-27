namespace PanelTweak.Setting;

public sealed class DefaultTextResolver : ITextResolver
{
    public string Resolve(TextRef text)
    {
        if (text.LocalizationKey != null)
            return text.Literal ?? text.LocalizationKey;
        return text.Literal ?? "";
    }
}