using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;

namespace BetterStorage
{
    [BepInPlugin("cc.lymone.HoL.BetterStorage", "BetterStorage", "1.0.0")]
    public class BetterStorage : BaseUnityPlugin
    {
        private static ConfigEntry<int> KuFangConfig;
        private static ConfigEntry<int> HorseRoomConfig;

        private void Start()
        {
            KuFangConfig = Config.Bind<int>("Config", "KuFang_Multiplier", 100, "库房库存倍率");
            HorseRoomConfig = Config.Bind<int>("Config", "HorseRoom_Multiplier", 10, "马厩大小倍率");
            Harmony.CreateAndPatchAll(typeof(BetterStorage));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FormulaData), "CunNum_KuFang")]
        public static bool multi_kufang_size(ref int Lv)
        {
            Lv = Lv * KuFangConfig.Value;
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FormulaData), "MaNum_HorseRoom")]
        public static bool multi_horseroom_size(ref int Lv)
        {
            Lv = Lv * HorseRoomConfig.Value;
            return true;
        }
    }
}
