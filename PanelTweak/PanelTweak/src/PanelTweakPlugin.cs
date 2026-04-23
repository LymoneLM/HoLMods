using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace PanelTweak;

[BepInPlugin(MODGUID, MODNAME, VERSION)]
public class PanelTweakPlugin : BaseUnityPlugin
{
    public const string MODNAME = "PanelTweak";
    public const string MODGUID = "cc.lymone.HoL." + MODNAME;
    public const string VERSION = "1.0.0";

    internal static Harmony Harmony = new(MODGUID);
    internal new static ManualLogSource Logger;
    internal static AssetBundle AssetBundle;

    private void Awake()
    {
        Logger = base.Logger;
        var asm = Assembly.GetExecutingAssembly();
        Harmony.PatchAll(asm);
        
        var path = Path.GetDirectoryName(asm.Location);
        AssetBundle = AssetBundle.LoadFromFile(path + "/paneltweak");
    }
}