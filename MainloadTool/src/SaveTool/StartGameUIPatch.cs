using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace MainloadTool;

[HarmonyPatch]
public class StartGameUIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartGameUI), "Start")]
    public static void AutoLoadSave(StartGameUI __instance)
    {
        var saveName = MainloadTool.AutoLoadSaveName.Value;
        if(saveName == "")
            return;
        
        MainloadTool.Logger.LogInfo($"Auto load save FW/{saveName}.");
        try
        {
            __instance.StartCoroutine(LoadSave(saveName));
        }
        catch (Exception e)
        {
            MainloadTool.Logger.LogWarning("Auto load save failed: " + e.Message);
        }
        
    }

    private static IEnumerator LoadSave(string saveName)
    {
        GameObject mainUI = GameObject.Find("MainUI");
        Transform cunDangUI = mainUI.transform.Find("CunDangUI");
        Transform saveTag = cunDangUI.Find("0");
        
        GameObject thisSaveTag = Instantiate(saveTag.gameObject, cunDangUI);
        thisSaveTag.name = saveName;
        
        cunDangUI.gameObject.SetActive(true);
        yield return null;
        thisSaveTag.transform.Find("BackBT").GetComponent<Button>().onClick.Invoke();
    }
}