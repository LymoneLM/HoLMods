using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace BetterLibrary {
    [BepInPlugin("cc.lymone.HoL.BetterLibrary", "BetterLibrary", "1.0.0")]
    public class BetterLibrary : BaseUnityPlugin {
        private static ConfigEntry<bool> is_Book_Unbreakable;
        private static ConfigEntry<int> multi_Book_Storage;
        private void Start() {
            is_Book_Unbreakable = Config.Bind<bool>("配置 Config", "书本不损坏 Is book unbreakable", true, "");
            multi_Book_Storage = Config.Bind<int>("配置 Config", "藏书阁空间倍率 Library capacity multiplier", 3, "");
            Harmony.CreateAndPatchAll(typeof(BetterLibrary));
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FormulaData), "BackBook_MemberNow")]
        public static bool Unbreakable_Book_Prefix(string BookID, int FudiIndex, string BuildID, int memberIndex, bool isDaiZou) {
            if (BuildID != "null") {
                int num = 1;
                if (isDaiZou) {
                    num = 0;
                } else if (!is_Book_Unbreakable.Value && TrueRandom.GetRanom(100) < 20) {
                    Mainload.Event_Tip.Add(new List<string>
                    {
                    "0",
                    "null",
                    AllText.Text_UIA[1312][Mainload.SetData[4]].Replace("@", Mainload.Member_now[memberIndex][4].Split(new char[]
                    {
                        '|'
                    })[0]).Replace("$", AllText.Text_AllProp[int.Parse(BookID)][Mainload.SetData[4]]),
                    "0"
                });
                    num = 0;
                }
                int num2 = FormulaData.BooksClass(int.Parse(BookID));
                for (int i = 0; i < Mainload.CangShuGeData_Now[FudiIndex].Count; i++) {
                    if (Mainload.CangShuGeData_Now[FudiIndex][i].Split(new char[]
                    {
                    '|'
                    })[0] == BuildID) {
                        string[] array = Mainload.CangShuGeData_Now[FudiIndex][i].Split(new char[]
                        {
                        '|'
                        });
                        string[] array2 = array[num2].Split(new char[]
                        {
                        '~'
                        });
                        string text = "null";
                        for (int j = 0; j < array2.Length; j++) {
                            if (array2[j].Split(new char[]
                            {
                            '@'
                            })[0] == BookID) {
                                int num3 = int.Parse(array2[j].Split(new char[]
                                {
                                '@'
                                })[1]) + num;
                                int num4 = int.Parse(array2[j].Split(new char[]
                                {
                                '@'
                                })[2]) - 1;
                                if (num3 + num4 > 0) {
                                    if (text == "null") {
                                        text = string.Concat(new string[]
                                        {
                                        BookID,
                                        "@",
                                        num3.ToString(),
                                        "@",
                                        num4.ToString()
                                        });
                                    } else {
                                        text = string.Concat(new string[]
                                        {
                                        text,
                                        "~",
                                        BookID,
                                        "@",
                                        num3.ToString(),
                                        "@",
                                        num4.ToString()
                                        });
                                    }
                                }
                            } else if (text == "null") {
                                text = array2[j];
                            } else {
                                text = text + "~" + array2[j];
                            }
                        }
                        string text2 = "null";
                        for (int k = 0; k < array.Length; k++) {
                            if (num2 == k) {
                                if (text2 == "null") {
                                    text2 = text;
                                } else {
                                    text2 = text2 + "|" + text;
                                }
                            } else if (text2 == "null") {
                                text2 = array[k];
                            } else {
                                text2 = text2 + "|" + array[k];
                            }
                        }
                        Mainload.CangShuGeData_Now[FudiIndex][i] = text2;
                        break;
                    }
                }
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FormulaData), "BookSNumMax_CangshuGe")]
        public static bool Multi_Capacity_Prefix(ref int Lv) {
            Lv *= multi_Book_Storage.Value;
            return true;
        }
    }
}
