using System;

namespace PanelTweak;

/// <summary>
/// 可本地化的文本引用。如果没有 LocalizationKey 则直接使用 Literal，否则通过本地化系统查找
/// </summary>
public readonly struct TextRef
{
    public string Literal { get; }
    public string LocalizationKey { get; }

    private TextRef(string literal, string localizationKey)
    {
        Literal = literal;
        LocalizationKey = localizationKey;
    }

    public static implicit operator TextRef(string text)
        => new(text ?? throw new ArgumentNullException(nameof(text)), null);

    public static TextRef Key(string key, string fallback = null)
        => new(fallback, key ?? throw new ArgumentNullException(nameof(key)));

    public string Resolve(ITextResolver resolver)
        => resolver.Resolve(this);
}