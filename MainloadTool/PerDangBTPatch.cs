using System;
using System.Collections.Generic;
using HarmonyLib;

namespace MainloadTool;

[HarmonyPatch]
public class PerDangBTPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PerDangBT), "OnEnableData")]
    public static bool PreventRemoveSaveWhenCheckError(PerDangBT __instance,ref bool ___isHaveData)
    {
        if (!MainloadTool.IsPreventRemoveSave.Value)
            return true;
        var flag = true;
        try
        {
            if (ES3.FileExists("FW/" + __instance.name + "/GameData.es3"))
            {
                ES3.Load<List<string>>("FamilyData", "FW/" + __instance.name + "/GameData.es3");
            }
            else
            {
                flag = false;
            }
        }
        catch (Exception e)
        {
            flag = false;
            MainloadTool.Logger.LogError($"Error when load save {__instance.name}, " +
                                         $"file remove has been prevented: {e.Message}");
        }
        ___isHaveData = flag;
        return false;
    }
}