using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MemorialBiography {
    [BepInPlugin("cc.lymone.HoL.MemorialBiography", "MemorialBiography", "1.0.0")]
    public class MemorialBiography : BaseUnityPlugin {
        private void Start() {
            Harmony.CreateAndPatchAll(typeof(MemorialBiography));
        }

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
        [HarmonyPatch(typeof(H_CiTang_Panel),"OnEnableShow")]
        public static void Panel_Init_Patch(H_CiTang_Panel __instance) {
            if(__instance.transform.Find("EventPanel"))
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
            if (__state.number +1 != Mainload.Member_Ci.Count) { 
                return;
            }
            Mainload.Member_Ci[Mainload.Member_Ci.Count-1].Add(__state.text);
        }

    }

    public class EventPanel : MonoBehaviour {
        public new int name = -1;
        public void Start() {
            if (Mainload.SetData[4] == 0) {
                base.transform.Find("TipA").GetComponent<Text>().fontSize = 18;
            } else {
                base.transform.Find("TipA").GetComponent<Text>().fontSize = 16;
            }
            base.transform.Find("TipA").GetComponent<Text>().text =
                AllText.Text_UIA[1133][Mainload.SetData[4]];

            this.transform.Find("CloseBT").GetComponent<Button>().onClick.AddListener(CloseBT);
        }
        public void OnEnable() {
            OnEnableShow_JiShi();
        }
        public void OnEnableShow_JiShi() {
            string[] array = new string[0];
            if (this.name != -1 && Mainload.Member_Ci[this.name].Count > 3) {
                array = Mainload.Member_Ci[this.name][3].Split('|');
            } else {
                var old = Mainload.Member_Ci[this.name][0].Split('|')[3].Split('@')[0].Split('~')[3];
                array = new string[] {
                     old+"@-1@null@null"
                };
            }
            string text = "null";
            for (int i = 0; i < array.Length; i++) {
                string text2;
                if (int.Parse(array[i].Split(new char[] { '@' })[1]) >= 0) {
                    text2 = AllText.Text_UIA[1222][Mainload.SetData[4]].Replace("@", array[i].Split(new char[] { '@' })[0]).Replace("$", AllText.Text_AllMemberEvent[int.Parse(array[i].Split(new char[] { '@' })[1])][Mainload.SetData[4]].Split(new char[] { '|' })[0].Replace("@", array[i].Split(new char[] { '@' })[2]).Replace("$", array[i].Split(new char[] { '@' })[3]));
                } else {
                    text2 = AllText.Text_UIA[1223][Mainload.SetData[4]].Replace("@", array[i].Split(new char[] { '@' })[0]);
                }
                if (text == "null") {
                    text = text2;
                } else {
                    text = text + "\n" + text2;
                }
            }
            if (text == "null") {
                base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                    .Find("InfoShow")
                    .GetComponent<Text>()
                    .text = " ";
            } else {
                base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                    .Find("InfoShow")
                    .GetComponent<Text>()
                    .text = text;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                .Find("InfoShow")
                .GetComponent<RectTransform>());
            base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                .GetComponent<RectTransform>()
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50f + base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                    .Find("InfoShow")
                    .GetComponent<RectTransform>()
                    .sizeDelta.y);
        }

        public void CloseBT() {
            this.name = -1;
            this.gameObject.SetActive(false);
        }
    }
}
