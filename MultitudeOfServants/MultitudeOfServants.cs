using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultitudeOfServants
{
    [BepInPlugin("cc.lymone.HoL.MultitudeOfServants", "MultitudeOfServants", "1.2.0")]
    public class MultitudeOfServants: BaseUnityPlugin
    {
        private static ConfigEntry<int> XiangFangConfig;
        private static ConfigEntry<float> GongZiConfig;
        private static ConfigEntry<int> PuRenMaxConfig;

        private void Start()
        {
            XiangFangConfig = Config.Bind("配置 Config", "厢房居住仆人倍率 Servant Quarters Capacity Multiplier", 10, "");
            GongZiConfig = Config.Bind<float>("配置 Config", "仆人工资倍率 Servant Wage Multiplier", 1.0f, "");
            PuRenMaxConfig = Config.Bind("配置 Config", "仆人上限倍率 Servant Cap Multiplier", 1, "");
            Harmony.CreateAndPatchAll(typeof(MultitudeOfServants));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FormulaData), "CostPuRen")]
        public static void Multi_PuRen_Cost(ref int __result)
        {
            __result = (int)(__result * GongZiConfig.Value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FormulaData), "PuNum_PuFang")]
        public static bool Multi_XiangFang_PuNum(ref int Lv)
        {
            Lv = Lv * XiangFangConfig.Value;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FormuAct),"ZhaoPuRen")]
        public static bool ZhaoPuRen_Multi_PuRenMax_Patch(int RenNum, ref string __result)
        {
            string result = AllText.Text_UIA[703][Mainload.SetData[4]];
            int index = int.Parse(Mainload.SceneID.Split(new char[]
            {
            '|'
            })[1]);
            if ((float)TrueRandom.GetRanom(100) <
                (float)Mathf.CeilToInt(float.Parse(Mainload.FamilyData[2]) / 5f) * 200f * (float)PuRenMaxConfig.Value -
                (float)int.Parse(Mainload.Fudi_now[index][2]))
            {
                Mainload.Fudi_now[index][2] = (int.Parse(Mainload.Fudi_now[index][2]) + RenNum).ToString();
                Mainload.Fudi_now[index][3] = (int.Parse(Mainload.Fudi_now[index][3]) - RenNum).ToString();
                Mainload.BuildInto_m[Mainload.BuildIndex_click][3] = (int.Parse(Mainload.BuildInto_m[Mainload.BuildIndex_click][3]) + RenNum).ToString();
                Mainload.PLNQCanShowNow += RenNum;
                if (Mainload.PLNQCanShowNow >= Mainload.MemberQShowNum[0])
                {
                    Mainload.PLNQCanShowNow = Mainload.MemberQShowNum[0];
                }
                result = AllText.Text_UIA[704][Mainload.SetData[4]].Replace("@", RenNum.ToString());
                FormulaData.TaskOrder_AddNum(39, RenNum);
                int num = 0;
                for (int i = 0; i < Mainload.Fudi_now.Count; i++)
                {
                    num += int.Parse(Mainload.Fudi_now[i][2]);
                }
                Mainload.AchDataUpdate.Add(new List<int>
            {
                23,
                num
            });
            }
            Mainload.isTimeGoDay = true;
            __result = result;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FastZhaoPuPanel), "OnEnableData")]
        public static IEnumerator Fast_ZhaoPuRen_Multi_PuRenMax_Patch(IEnumerator result, FastZhaoPuPanel __instance)
        {
            var SceneIndex = int.Parse(Mainload.SceneID.Split(new char[]{'|'})[1]);
            int num2;
            for (int a = 0; a < Mainload.BuildInto_m.Count; a = num2 + 1)
            {
                if (Mainload.AllBuilddata[int.Parse(Mainload.BuildInto_m[a][1])][5] == "10" && int.Parse(Mainload.BuildInto_m[a][6]) >= 0)
                {
                    int num = FormulaData.PuNum_PuFang(int.Parse(Mainload.BuildInto_m[a][1]), int.Parse(Mainload.BuildInto_m[a][2])) - int.Parse(Mainload.BuildInto_m[a][3]);
                    if ((float)Mathf.CeilToInt(float.Parse(Mainload.FamilyData[2]) / 5f) * 200f * (float)PuRenMaxConfig.Value> (float)int.Parse(Mainload.Fudi_now[SceneIndex][2]))
                    {
                        if (num > 0)
                        {
                            Mainload.Fudi_now[SceneIndex][2] = (int.Parse(Mainload.Fudi_now[SceneIndex][2]) + num).ToString();
                            Mainload.Fudi_now[SceneIndex][3] = (int.Parse(Mainload.Fudi_now[SceneIndex][3]) - num).ToString();
                            Mainload.BuildInto_m[a][3] = (int.Parse(Mainload.BuildInto_m[a][3]) + num).ToString();
                            Mainload.PLNQCanShowNow += num;
                            if (Mainload.PLNQCanShowNow >= Mainload.MemberQShowNum[0])
                            {
                                Mainload.PLNQCanShowNow = Mainload.MemberQShowNum[0];
                            }
                            FormulaData.TaskOrder_AddNum(39, num);
                        }
                        yield return null;
                    }
                    else
                    {
                        Mainload.Tip_Show.Add(new List<string>
                    {
                        "1",
                        AllText.Text_TipShow[315][Mainload.SetData[4]]
                    });
                        __instance.StopCoroutine("OnEnableData");
                        __instance.Invoke("EndShow", 0.5f);
                    }
                }
                else if ((a + 1) % 50 == 0)
                {
                    yield return null;
                }
                if (a >= Mainload.BuildInto_m.Count - 1)
                {
                    Mainload.Tip_Show.Add(new List<string>
                {
                    "0",
                    AllText.Text_TipShow[314][Mainload.SetData[4]]
                });
                    __instance.StopCoroutine("OnEnableData");
                    __instance.Invoke("EndShow", 0.5f);
                }
                if (Input.GetKeyDown(Mainload.FastKey[0]))
                {
                    __instance.StopCoroutine("OnEnableData");
                    __instance.Invoke("EndShow", 0.5f);
                }
                num2 = a;
            }
            yield break;
        }
    }
}
