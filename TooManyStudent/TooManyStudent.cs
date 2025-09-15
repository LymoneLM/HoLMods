using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;

namespace TooManyStudent
{
    [BepInPlugin("cc.lymone.HoL.TooManyStudent", "TooManyStudent", "1.2.0")]
    public class TooManyStudent : BaseUnityPlugin
    {
        private static ConfigEntry<int> TeacherConfig;
        private static ConfigEntry<int> StudentConfig;

        private void Start()
        {
            TeacherConfig = Config.Bind<int>("配置 Config", "教师数量乘数 Teacher Multiplier", 2,
                "对两个学校都适用 Applicable to both schools");
            StudentConfig = Config.Bind<int>("配置 Config", "学生容量乘数 Student Multiplier", 2,
                "对两个学校都适用 Applicable to both schools");
            Harmony.CreateAndPatchAll(typeof(TooManyStudent));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FormulaData), "SiShuTSnum")]
        public static void Multi_TSnum(ref List<int> __result)
        {
            __result[0] *= TeacherConfig.Value;
            __result[1] *= StudentConfig.Value;
        }
    }
}
