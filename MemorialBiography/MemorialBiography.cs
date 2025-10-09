using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace MemorialBiography {
    [BepInPlugin("cc.lymone.HoL.MemorialBiography", "MemorialBiography", "1.1.0")]
    public class MemorialBiography : BaseUnityPlugin {
        public static ConfigEntry<int> CiTangCapacityMultiplier;
        public static ConfigEntry<bool> IsMemorialTabletForever;
        private void Awake() {
            CiTangCapacityMultiplier = Config.Bind<int>("配置 Config",
                "祠堂容量倍率 Shrine Capacity Multiplier", 1);
            IsMemorialTabletForever = Config.Bind<bool>("配置 Config",
                "牌位不损坏 Is Memorial Tablet Never Broken", false);

            Harmony.CreateAndPatchAll(typeof(CiTangPanelPatch));
            if (CiTangCapacityMultiplier.Value != 1) {
                Harmony.CreateAndPatchAll(typeof(FormulaDataPatch));
            }

            if (IsMemorialTabletForever.Value) {
                Mainload.YearsMemberCiCun = Int32.MaxValue;
            }
        }
    }
}
