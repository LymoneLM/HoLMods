using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;

namespace TooManyStudent
{
    [BepInPlugin("cc.lymone.HoL.TooManyStudent", "TooManyStudent", "1.0.0")]
    public class TooManyStudent : BaseUnityPlugin
    {
        private static ConfigEntry<int> TeacherConfig;
        private static ConfigEntry<int> StudentConfig;

        private void Start()
        {
            TeacherConfig = Config.Bind<int>("Config", "Teacher_Multiplier", 2, "教师数量乘数");
            StudentConfig = Config.Bind<int>("Config", "Student_Multiplier", 2, "学生容量乘数");
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
