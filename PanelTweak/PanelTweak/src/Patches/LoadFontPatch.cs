using HarmonyLib;
using UnityEngine.UI;

namespace PanelTweak;

[HarmonyPatch]
public class LoadFontPatch {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StartGameUI), "Start")]
    private static void LoadFont(StartGameUI __instance)
    {
        var ts = __instance.transform.parent.Find("CunDangUI").Find("DelCunDPanel");
        var font = ts.Find("TipA").GetComponent<Text>().font;
        FontManager.BoldFont = font;
        font = ts.Find("CancelBT").Find("Text").GetComponent<Text>().font;
        FontManager.MediumFont = font;
    }
}