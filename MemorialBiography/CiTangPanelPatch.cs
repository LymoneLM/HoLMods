using HarmonyLib;
using UnityEngine;
using static UnityEngine.Object;

namespace MemorialBiography {
    internal class CiTangPanelPatch {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(H_CiTang_PanelA), "Start")]
        public static void CopyMemberEventPanel() {
            GameObject AllPanel = GameObject.Find("AllUI/AllPanel");

            Transform H_CiTang_Panel = AllPanel.transform.Find("H_CiTang_Panel");
            Transform PanelB = H_CiTang_Panel.transform.Find("PanelB");
            var EventPanel = Instantiate(PanelB.gameObject, H_CiTang_Panel);
            EventPanel.name = "EventPanel";

            Transform InfoShow = AllPanel.transform.Find("ZupuPanel").Find("MemberEventPanel").Find("PanelA").Find("AllJiShi").Find("Viewport").Find("Content").Find("InfoShow");
            Transform parent = EventPanel.transform.Find("AllCanSelect").Find("Viewport").Find("Content");
            var info = Instantiate(InfoShow.gameObject, parent);
            info.name = "InfoShow";

            Destroy(EventPanel.GetComponent<H_CiTang_PanelB>());
            EventPanel.transform.SetSiblingIndex(PanelB.GetSiblingIndex());
            EventPanel.AddComponent<EventPanel>();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(H_CiTang_PanelA), "AddBT")]
        public static bool PanelA_AddBT_Patch(H_CiTang_PanelA __instance) {
            __instance.transform.parent.Find("EventPanel").gameObject.SetActive(false);
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(H_CiTang_PanelA), "CloseBT")]
        public static bool PanelA_CloseBT_Patch(H_CiTang_PanelA __instance) {
            __instance.transform.parent.Find("EventPanel").gameObject.SetActive(false);
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(H_CiTang_Panel), "OnEnableShow")]
        public static void Panel_Init_Patch(H_CiTang_Panel __instance) {
            if (__instance.transform.Find("EventPanel"))
                __instance.transform.Find("EventPanel").gameObject.SetActive(false);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerPaiWeiBT), "ClickBT")]
        public static bool PerPaiWeiBT_ClickBT_Patch(PerPaiWeiBT __instance) {
            if (__instance.ShowID == 0) {
                if (__instance.transform.parent.parent.parent.parent.parent.Find("PanelB").gameObject.activeSelf) {
                    __instance.transform.parent.parent.parent.parent.parent.Find("PanelB").GetComponent<H_CiTang_PanelB>().RemovePaiWeiBT(int.Parse(__instance.name));
                } else {
                    __instance.transform.parent.parent.parent.parent.parent.Find("EventPanel").GetComponent<EventPanel>().name = int.Parse(__instance.name);
                    if (__instance.transform.parent.parent.parent.parent.parent.Find("EventPanel").gameObject.activeSelf) {
                        __instance.transform.parent.parent.parent.parent.parent.Find("EventPanel").GetComponent<EventPanel>().OnEnableShow_JiShi();
                    } else
                        __instance.transform.parent.parent.parent.parent.parent.Find("EventPanel").gameObject.SetActive(true);
                }
            } else if (__instance.ShowID == 1) {
                __instance.transform.parent.parent.parent.parent.GetComponent<H_CiTang_PanelB>().AddPaiWeiBT(int.Parse(__instance.name));
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Event_2_PanelA), "SureBT")]
        public static bool Create_PaiWei_PrePatch(ref (int number, string text) __state, Event_2_PanelA __instance) {
            __state.number = Mainload.Member_Ci.Count;
            var index = Traverse.Create(__instance).Field("MemberIndex").GetValue<int>();
            __state.text = Mainload.Member_now[index][36];
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Event_2_PanelA), "SureBT")]
        public static void Create_PaiWei_PostPatch(ref (int number, string text) __state) {
            if (__state.number + 1 != Mainload.Member_Ci.Count) {
                return;
            }
            Mainload.Member_Ci[Mainload.Member_Ci.Count - 1].Add(__state.text);
        }

    }
}
