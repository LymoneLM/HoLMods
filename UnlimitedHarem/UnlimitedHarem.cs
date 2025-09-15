using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace UnlimitedHarem
{
    [BepInPlugin("cc.lymone.HoL.UnlimitedHarem", "UnlimitedHarem", "1.0.0")]
    public class UnlimitedHarem : BaseUnityPlugin
    {
        private static ConfigEntry<bool> OtherFamilyLimit;
        private void Start() {
            OtherFamilyLimit = Config.Bind<bool>("配置 Config", "其他家族也无限结婚 Is remove other clans marry limit", false,
                "启用后其他家族成员也能无限结婚。Once it's enabled, other clans members can also get married as many times as they want.");
            Harmony.CreateAndPatchAll(typeof(UnlimitedHarem));
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FormulaData),"isCanNaQie")]
        public static bool unlimited_NaQie_Prefix(ref int ShijiaIndex) {
            if (ShijiaIndex == -1) {
                ShijiaIndex = -2;
            }
            if (OtherFamilyLimit.Value && ShijiaIndex >= 0) {
                ShijiaIndex = -2;
            }
            return true;
        }
    }
}
