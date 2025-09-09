using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace RemoveEverything
{
    [BepInPlugin("cc.lymone.HoL.RemoveEverything", "RemoveEverything", "1.0.0")]
    public class RemoveEverything : BaseUnityPlugin
    {
        private static ConfigEntry<bool> isSafeRemoveStorage;
        private static ConfigEntry<bool> switchChangShuGeRemoveStrategy;


        private void Start()
        {
            isSafeRemoveStorage = Config.Bind<bool>("Config", "is_Safe_Remove_Storage", false, 
                "是否安全移除仓储(如果为是，将确保仓储和马厩不为负)\n" +
                "开启此选项可能会导致部分建筑不会被删除\n" +
                "仓储容量为负不会导致存档损坏，随后补充容量即可");
            switchChangShuGeRemoveStrategy = Config.Bind<bool>("Config", "switch_ChangShuGe_Remove_Strategy", true, 
                "切换藏书阁移除时，处理正在借阅书籍的策略\n" +
                "true :正在被借阅的书籍将被强制索要，纳入仓库。\n" +
                "      此人的阅读进度损失，但是书籍得以保留。\n" +
                "false:正在被借阅的书籍会赠送给借读的人，并且此人的借阅策略会重置为空\n" +
                "      书籍会损失，但是此人的学习进度得以保留。\n" +
                "![注意]无论如何选择，模组都会首先尝试把在借书籍转移到其他府邸藏书阁");
            Harmony.CreateAndPatchAll(typeof(RemoveEverything));
            Logger.LogInfo("RemoveEverything loaded.");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DelBuildMPanel), "OnEnableData")]
        public static IEnumerator RemoveBuild_Patch(IEnumerator result,DelBuildMPanel __instance)
        {
            var SceneIndex = int.Parse(Mainload.SceneID.Split(new char[]{'|'})[1]);
            var School = new HashSet<string>();
            var ZhuZhai = new HashSet<string>();
            var ChangShuGe = new HashSet<string>();
            int num;
            for (int a = 0; a < Mainload.BuildInto_m.Count; a = num + 1)
            {
                switch(Mainload.AllBuilddata[int.Parse(Mainload.BuildInto_m[a][1])][5])
                {
                    case "0": // 祠堂                  
                    case "2": // 三种庙

                    case "6": // 马厩
                    case "7": // 库房

                    // 装饰类建筑
                    case "9":
                    case "21":
                    case "22":
                    case "23":
                    case "24":
                    case "29":
                    case "35":
                    case "38":
                        FormulaData.RemoveBuildNew("M", SceneIndex, a, 0, false, false);
                        a -= 1;
                        break;
                    
                    case "3": // 学校
                    case "4": // 戏台
                        School.Add(Mainload.BuildInto_m[a][0]);
                        FormulaData.RemoveBuildNew("M", SceneIndex, a, 0, false, false);
                        a -= 1;
                        break;
                    
                    case "5": // 藏书阁
                        ChangShuGe.Add(Mainload.BuildInto_m[a][0]);
                        FormulaData.RemoveBuildNew("M", SceneIndex, a, 0, false, false);
                        a -= 1;
                        break;
                                  
                    case "8": // 住宅
                        ZhuZhai.Add(Mainload.BuildInto_m[a][0]);
                        FormulaData.RemoveBuildNew("M", SceneIndex, a, 0, false, false);
                        a -= 1;
                        break;
                   
                    case "10": // 厢房
                        if (int.Parse(Mainload.BuildInto_m[a][3]) > 0)
                        {
                            __instance.transform.parent.Find("FastSanPuPanel").gameObject.SetActive(true);
                        }
                        FormulaData.RemoveBuildNew("M", SceneIndex, a, 0, false, false);
                        a -= 1;
                        break;
                }
                if ((a + 1) % 10 == 0)
                {
                    yield return null;
                }
                if (a >= Mainload.BuildInto_m.Count - 1)
                {
                    // 后处理
                    yield return null;
                    // 学校、戏台

                    yield return null;
                    // 住宅

                    yield return null;
                    // 藏书阁

                    __instance.StopCoroutine("OnEnableData");
                    __instance.Invoke("ChangeScene", 0.1f);
                }
                // 关闭反悔退出功能
                //if (Input.GetKeyDown(Mainload.FastKey[0]))
                //{
                //    __instance.StopCoroutine("OnEnableData");
                //    __instance.Invoke("ChangeScene", 0.1f);
                //}
                num = a;
            }
            yield break;
        }
    }
}
