using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TryOptimize;

[BepInPlugin(MODGUID, MODNAME, VERSION)]
public class TryOptimize : BaseUnityPlugin
{
    public const string MODNAME = "TryOptimize";
    public const string MODGUID = "cc.lymone.HoL." +  MODNAME;
    public const string VERSION = "1.0.0";

    internal new static ManualLogSource Logger;
    
    private void Awake()
    {
        Logger = base.Logger;
        Harmony.CreateAndPatchAll(typeof(TryOptimize), MODGUID);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PerBuildShow), "UpdateShow")]
    public static bool PerBuildShowCdPatch()
    {
        return false;
    }
}
