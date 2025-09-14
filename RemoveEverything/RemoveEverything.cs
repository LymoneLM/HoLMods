using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RemoveEverything
{
    [BepInPlugin("cc.lymone.HoL.RemoveEverything", "RemoveEverything", "1.0.0")]
    public class RemoveEverything : BaseUnityPlugin
    {
        private static ConfigEntry<bool> isSafeRemoveStorage;
        private static ConfigEntry<bool> ChangShuGeRemoveStrategy;


        private void Start()
        {
            isSafeRemoveStorage = Config.Bind<bool>("配置 Config", "安全移除库存 Safe Remove Storage", false, 
                "是否安全移除仓储(如果为true，将确保仓储和马厩不为负)\n" +
                "开启此选项可能会导致部分建筑不会被删除\n" +
                "注意：仓储容量为负不会导致存档损坏，随后补充容量即可");
            ChangShuGeRemoveStrategy = Config.Bind<bool>("配置 Config", "是否强行保留书籍 Force keep the books", true,
                "切换藏书阁移除时，处理正在借阅书籍的策略\n" +
                "true :强行将书籍纳入库存，但借阅人正常阅读，相当于刷了一本书（微作弊）\n" +
                "false:书籍被赠与借阅者，家族损失该书籍（正常流程）\n");
            Harmony.CreateAndPatchAll(typeof(RemoveEverything));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DelBuildMPanel), "OnEnableData")]
        public static IEnumerator RemoveBuild_Patch(IEnumerator result,DelBuildMPanel __instance)
        {
            var SceneIndex = int.Parse(Mainload.SceneID.Split(new char[]{'|'})[1]);
            var School = new HashSet<string>();
            var ZhuZhai = new HashSet<string>();
            var BookStr = new List<string>();
            // 预处理藏书阁
            for (int n = 0; n < Mainload.CangShuGeData_Now.Count; n++)
            {
                BookStr.Add(Mainload.CangShuGeData_Now[SceneIndex][n]);
            }
            int num;
            for (int a = 0; a < Mainload.BuildInto_m.Count; a = num + 1)
            {
                switch(Mainload.AllBuilddata[int.Parse(Mainload.BuildInto_m[a][1])][5])
                {
                    case "0": // 祠堂                  
                    case "2": // 三种庙

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
                        FormulaData.RemoveBuildNew("M", SceneIndex, a, 0, false, false);
                        a -= 1;
                        break;

                    case "6": // 马厩
                        if (isSafeRemoveStorage.Value && int.Parse(Mainload.BuildInto_m[a][6]) >= 0 )
                        {
                            if(int.Parse(Mainload.FamilyData[6]) <=
                                FormulaData.MaNum_HorseRoom(int.Parse(Mainload.BuildInto_m[a][1]), int.Parse(Mainload.BuildInto_m[a][2])))
                            {
                                break;
                            }
                        }
                        FormulaData.RemoveBuildNew("M", SceneIndex, a, 0, false, false);
                        a -= 1;
                        break;
                    case "7": // 库房
                        if (isSafeRemoveStorage.Value && int.Parse(Mainload.BuildInto_m[a][6]) >= 0 )
                        {
                            if (int.Parse(Mainload.FamilyData[5]) + 10 <= 
                                FormulaData.CunNum_KuFang(int.Parse(Mainload.BuildInto_m[a][1]), int.Parse(Mainload.BuildInto_m[a][2])))
                            {
                                break;
                            }
                        }
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
                    for(int i = 0; i < Mainload.MenKe_Now.Count; i++)
                    {
                        if (School.Contains(Mainload.MenKe_Now[i][9].Split('|')[1]))
                        {
                            Mainload.MenKe_Now[i][9] = "0|null|" + Mainload.MenKe_Now[i][9].Split(new char[]{'|'})[2];
                            Mainload.MenKe_Now[i][10] = "0";
                        }
                    }
                    for(int i = 0; i < Mainload.Member_now.Count; i++)
                    {
                        if (School.Contains(Mainload.Member_now[i][3].Split('|')[2]))
                        {
                            Mainload.Member_now[i][3] = string.Concat(new string[]
                            {
                                Mainload.Member_now[i][3].Split(new char[]{'|'})[0],
                                "|",
                                Mainload.Member_now[i][3].Split(new char[]{'|'})[1],
                                "|null|",
                                Mainload.Member_now[i][3].Split(new char[]{'|'})[3]
                            });
                            Mainload.Member_now[i][32] = Mainload.Member_now[i][32].Split(new char[]{'|'})[0] 
                                + "|" + Mainload.Member_now[i][32].Split(new char[]{ '|'})[1] + "|0|0|0|0";
                        }
                    }
                    yield return null;

                    // 藏书阁
                    if(BookStr.Count != 0)
                    {
                        var Library = new HashSet<string>();
                        var stockBook = new Dictionary<string, int>();
                        foreach (var line in BookStr)
                        {
                            // 分割主要部分
                            List<string> Categories = line.Split('|').ToList();
                            Library.Add(Categories[0]);
                            Categories.RemoveAt(0);
                            foreach (var category in Categories)
                            {
                                //分割具体书
                                if (string.IsNullOrEmpty(category))
                                {
                                    continue;
                                }
                                List<string> books = category.Split('~').ToList();
                                foreach (var book in books)
                                {
                                    var values = book.Split('@');
                                    if (int.Parse(values[1]) > 0)
                                    {
                                        if (!stockBook.ContainsKey(values[0]))
                                            stockBook[values[0]] = 0;
                                        stockBook[values[0]] += int.Parse(values[1]);
                                    }
                                    if (int.Parse(values[2]) > 0 && ChangShuGeRemoveStrategy.Value)
                                    {
                                        if (!stockBook.ContainsKey(values[0]))
                                            stockBook[values[0]] = 0;
                                        stockBook[values[0]] += int.Parse(values[1]);
                                    }
                                }
                            }
                        }
                        // 库存入库
                        for (int i = 0; i< Mainload.Prop_have.Count; i++)
                        {
                            if (stockBook.ContainsKey(Mainload.Prop_have[i][0]))
                            {
                                Mainload.Prop_have[i][1] = (int.Parse(Mainload.Prop_have[i][1]) + stockBook[Mainload.Prop_have[i][0]]).ToString();
                                Mainload.FamilyData[5] = (int.Parse(Mainload.FamilyData[5]) - stockBook[Mainload.Prop_have[i][0]]).ToString();
                                stockBook.Remove(Mainload.Prop_have[i][0]);
                            }
                        }
                        foreach (var i in stockBook)
                        {
                            Mainload.Prop_have.Add(new List<string>
                            {
                                i.Key,
                                i.Value.ToString()
                            });
                            Mainload.FamilyData[5] = (int.Parse(Mainload.FamilyData[5]) - i.Value).ToString();
                        }
                        // 出借书处理
                        for (int i=0; i < Mainload.Member_now.Count; i++)
                        {
                            var str = Mainload.Member_now[i][19].Split('~')[0];
                            if (Library.Contains(str.Split('@')[2]))
                            {
                                string text = str[0] + "@" + str[1] + "@" + "null";
                                text = text + "~" + Mainload.Member_now[i][19].Split('~')[1];
                                Mainload.Member_now[i][19] = text;
                            }
                        }

                    }    
                    yield return null;

                    // 住宅


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
