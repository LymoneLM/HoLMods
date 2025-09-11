using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;

namespace BetterStorage
{
    [BepInPlugin("cc.lymone.HoL.BetterStorage", "BetterStorage", "1.1.0")]
    public class BetterStorage : BaseUnityPlugin
    {
        private static ConfigEntry<int> KuFangConfig;
        private static ConfigEntry<int> HorseRoomConfig;

        private void Start()
        {
            KuFangConfig = Config.Bind<int>("配置 Config", "库房容量倍率 Warehouse Capacity Multiplier", 100, 
                "只会影响新建/新拆的建筑\n" +
                "Only affects newly built/delete buildings");
            HorseRoomConfig = Config.Bind<int>("配置 Config", "马厩容量倍率 Barn Capacity Multiplier", 10, 
                "只会影响新建/新拆的建筑\n" +
                "Only affects newly built/delete buildings");
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
