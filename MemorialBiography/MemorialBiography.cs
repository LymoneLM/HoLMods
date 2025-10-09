using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace MemorialBiography {
    [BepInPlugin("cc.lymone.HoL.MemorialBiography", "MemorialBiography", "1.0.0")]
    public class MemorialBiography : BaseUnityPlugin {
        private void Awake() {
            Harmony.CreateAndPatchAll(typeof(CiTangPanelPatch));
        }
    }
}
