using System;

namespace PanelTweak;

public enum TextRefKind
{
    Literal,
    LocalizationKey
}

/// <summary>
/// 可本地化的文本引用。Literal 表示直接使用文本，Key 表示通过本地化系统查找。
/// </summary>
public readonly struct TextRef
{
    public TextRefKind Kind { get; }
    public string Value { get; }
    public string? Fallback { get; }
    public string? Scope { get; }

    private TextRef(TextRefKind kind, string value, string? fallback, string? scope)
    {
        Kind = kind;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Fallback = fallback;
        Scope = scope;
    }

    public static TextRef Literal(string text)
        => new(TextRefKind.Literal, text, null, null);

    public static TextRef Key(string key, string? fallback = null, string? scope = null)
        => new(TextRefKind.LocalizationKey, key, fallback, scope);

    public string Resolve(ITextResolver resolver)
        => resolver.Resolve(this);
}