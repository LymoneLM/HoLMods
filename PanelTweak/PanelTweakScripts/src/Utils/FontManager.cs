using UnityEngine;

namespace PanelTweak;

public static class FontManager
{
    public static Font BoldFont;
    public static Font MediumFont;

    static FontManager()
    {
        var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        BoldFont = font;
        MediumFont = font;
    }
}