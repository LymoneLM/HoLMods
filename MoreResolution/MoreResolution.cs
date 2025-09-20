using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace MoreResolution {
    [BepInPlugin("cc.lymone.HoL.MoreResolution", "MoreResolution", "1.1.0")]
    public class MoreResolution : BaseUnityPlugin {
        private static readonly List<List<int>> resolutions = new List<List<int>> {
            new List<int> { 640, 480 },     // 4:3
            new List<int> { 800, 600 },     // 4:3
            new List<int> { 1280, 720 },    // 16:9
            new List<int> { 1280, 800 },    // 16:10
            new List<int> { 1280, 960 },    // 4:3
            new List<int> { 1400, 1050 },   // 4:3
            new List<int> { 1440, 900 },    // 16:10
            new List<int> { 1600, 900 },    // 16:9
            new List<int> { 1600, 1200 },   // 4:3
            new List<int> { 1680, 1050 },   // 16:10
            new List<int> { 1920, 1080 },   // 16:9 (Full HD)
            new List<int> { 1920, 1200 },   // 16:10
            new List<int> { 2560, 1440 },   // 16:9 (QHD)
            new List<int> { 2560, 1600 },   // 16:10 (WQXGA)
            new List<int> { 2880, 1800 },   // 16:10
            new List<int> { 3200, 1800 },   // 16:9
            new List<int> { 3440, 1440 },   // 21:9 (超宽屏)
            new List<int> { 3840, 2160 },   // 16:9 (4K UHD)
            new List<int> { 3840, 2400 },   // 16:10 (WQUXGA)
            new List<int> { 5120, 2880 },   // 16:9 (5K)
            new List<int> { 5120, 3200 },   // 16:10
            new List<int> { 7680, 4320 }    // 16:9 (8K UHD)
        };

        private static readonly List<string> VSyncText = new List<string> {
            "垂直同步",
            "VSync"
        };

        private static ConfigEntry<int> customResolution_width;
        private static ConfigEntry<int> customResolution_height;
        private static ConfigEntry<bool> onVSync;

        private static bool custom_flag = false;
        private void Start() {
            customResolution_width = Config.Bind<int>("自定义分辨率 Custom Resolution", "宽度 width", 0, "自定义分辨率的宽度，0为取消");
            customResolution_height = Config.Bind<int>("自定义分辨率 Custom Resolution", "高度 height", 0, "自定义分辨率的高度，0为取消");
            onVSync = Config.Bind<bool>("配置 Config", "启用垂直同步 VSync On", true, "启用垂直同步，游戏本体默认开启");

            Mainload.AllFenBData = resolutions;
            if (customResolution_width.Value != 0 && customResolution_height.Value != 0) {
                custom_flag = true;
                Mainload.AllFenBData.Insert(0, new List<int> { customResolution_width.Value, customResolution_height.Value });
            }

            Harmony.CreateAndPatchAll(typeof(MoreResolution));
            AllText.Text_UIA[11] = new List<string>
            {
                "全屏",
                "Fullscreen"
            };
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SetPanel), "Awake")]
        public static void CreateFenB(SetPanel __instance) {
            var options = new List<Dropdown.OptionData>();
            if (custom_flag) {
                Dropdown.OptionData optionData = new Dropdown.OptionData {
                    text = string.Concat(new List<string> {
                        "* ",
                        customResolution_width.Value.ToString(),
                        "x",
                        customResolution_height.Value.ToString()
                    })
                };
            }
            for (int i = custom_flag ? 1 : 0; i < Mainload.AllFenBData.Count; i++) {
                if (Mainload.AllFenBData[i][0] > Screen.currentResolution.width) {
                    break;
                } else if (Mainload.AllFenBData[i][0] == Screen.currentResolution.width && Mainload.AllFenBData[i][1] > Screen.currentResolution.height) {
                    break;
                }
                Dropdown.OptionData optionData = new Dropdown.OptionData {
                    text = string.Join("x", Mainload.AllFenBData[i])
                };
                options.Add(optionData);
            }
            __instance.transform.Find("FenB").Find("AllClass").GetComponent<Dropdown>().options = options;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mainload),"ReadSetData")]
        public static bool LoadVSyncConfig() {
            VSyncSet(onVSync.Value);
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveData), "ReadSetData")]
        public static bool ProtectFenBConfigPrefix(ref int __state) {
            try {
                var SetData = ES3.Load<List<int>>("SetData", "FW/SetData.es3", Mainload.SetData);
                __state = SetData[2];
            } catch (FormatException) {
                __state = -1;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveData), "ReadSetData")]
        public static void ProtectFenBConfigPostfix(int __state) {
            if (__state == -1) {
                return;
            }
            Mainload.SetData[2] = __state;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SetPanel), "Start")]
        public static void CallCreateVSyncTogglePatch() {
            MoreResolution.CreateVSyncToggle();
        }

        public static void CreateVSyncToggle() {
            GameObject SetPanel = GameObject.Find("SetPanel");
            GameObject QuanP = SetPanel.transform.Find("QuanP").gameObject;
            if (SetPanel != null && QuanP != null) {
                GameObject VSync = Instantiate(QuanP, SetPanel.transform);

                VSync.name = "VSync";
                VSync.transform.SetSiblingIndex(QuanP.transform.GetSiblingIndex() + 1);
                VSync.transform.Find("Tip").GetComponent<Text>().text = MoreResolution.VSyncText[Mainload.SetData[4]];
                QuanP.transform.Translate(new Vector3(0, 10, 0));
                VSync.transform.Translate(new Vector3(0, -50, 0));

                var toggle = VSync.transform.Find("Toggle").GetComponent<Toggle>();
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(MoreResolution.VSyncSet);
                toggle.isOn = onVSync.Value;
            }
        }
        public static void VSyncSet(bool isOn) {
            onVSync.Value = isOn;
            if (isOn) {
                QualitySettings.vSyncCount = 1;
            } else {
                QualitySettings.vSyncCount = 0;
            }
        }
    }

}
