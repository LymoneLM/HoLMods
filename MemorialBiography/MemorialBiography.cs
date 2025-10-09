using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace MemorialBiography {
    [BepInPlugin("cc.lymone.HoL.MemorialBiography", "MemorialBiography", "1.0.0")]
    public class MemorialBiography : BaseUnityPlugin {
        public static ConfigEntry<int> CiTangCapacityMultiplier;
        private void Awake() {
            CiTangCapacityMultiplier = Config.Bind<int>("配置 Config",
                "祠堂容量倍率 Shrine Capacity Multiplier", 1);
            Harmony.CreateAndPatchAll(typeof(CiTangPanelPatch));
            if (CiTangCapacityMultiplier.Value != 1) {
                Harmony.CreateAndPatchAll(typeof(FormulaDataPatch));
            }
        }
    }
}
