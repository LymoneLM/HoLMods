using HarmonyLib;

namespace MemorialBiography {
    internal class FormulaDataPatch {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FormulaData), "PaiNumForCiTang")]
        public static void MultiOfCapacity(ref int __result) {
            __result *= MemorialBiography.CiTangCapacityMultiplier.Value;
        }
    }
}
