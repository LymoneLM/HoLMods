using HarmonyLib;
using UnityEngine;

namespace PanelTweak;

[HarmonyPatch]
public class PluginEntryPatch
{
    private static GameObject _selectSavePanel;
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StartGameUI), "Start")]
    private static void LoadPanels(StartGameUI __instance)
    {
        var allPanel = __instance.transform.parent;
        _selectSavePanel = Object.Instantiate(
            PanelTweakPlugin.AssetBundle.LoadAsset<GameObject>("Assets/PanelTweak/SelectSavePanel.prefab"),
            allPanel, false);
        _selectSavePanel.SetActive(false);
        
        Object.Destroy(allPanel.Find("CunDangUI").gameObject);
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StartGameUI), "StartBT")]
    private static bool StartSelectSavePanelBT(StartGameUI __instance)
    {
        _selectSavePanel.SetActive(true);
        __instance.gameObject.SetActive(false);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InitGameUI), "CloseBT")]
    private static bool CloseSelectSavePanelBT(InitGameUI __instance)
    {
        _selectSavePanel.SetActive(true);
        __instance.gameObject.SetActive(false);
        return false;
    }
}