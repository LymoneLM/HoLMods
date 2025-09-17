using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MoreResolution
{
    [BepInPlugin("cc.lymone.HoL.MoreResolution", "MoreResolution","1.0.0")]
    public class MoreResolution : BaseUnityPlugin
    {
        private static void Start() {
            Mainload.AllFenBData.Add(new List<int> { 
                1600,
                900
            });
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SetPanel),"Awake")]]
        public static void CreateFenB(SetPanel __instance) {
            __instance.transform.Find("FenB").Find("AllClass").GetComponent<Dropdown>().options = new List<Dropdown.OptionData>();
            for (int i = 0; i < Mainload.AllFenBData.Count; i++) {
                Dropdown.OptionData optionData = new Dropdown.OptionData();
                optionData.text = 
                __instance.transform.Find("FenB").Find("AllClass").GetComponent<Dropdown>().options.Add(optionData);
                //if (__instance.isChangeLanguage) {
                //    __instance.transform.Find("LoadSpeed").Find("AllClass").GetComponent<Dropdown>().value = -1;
                //}
            }
        }

    }
}
