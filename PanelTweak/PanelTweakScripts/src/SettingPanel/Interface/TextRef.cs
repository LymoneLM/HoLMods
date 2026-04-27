using System;

namespace PanelTweak;

/// <summary>
/// 可本地化的文本引用。
/// 记录直接文本 Literal 和本地化键 LocalizationKey
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
}