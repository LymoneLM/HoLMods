using BepInEx;
using BepInEx.Logging;

namespace MainloadTool {
    [BepInPlugin(MODGUID,MODNAME,VERSION)]
    public class MainloadTool : BaseUnityPlugin{
        private const string MODNAME = "MainloadTool";
        private const string MODGUID = "cc.lymone.HoL." + MODNAME;
        private const string VERSION = "1.0.0";

        internal new static ManualLogSource Logger;
        internal static string GameVersion;

        private void Awake() {
            Logger = base.Logger;

            GameVersion = "_v" + Mainload.Vision_now.Substring(2);
        }

        private void Start() {
            DumpMainload.AllPropdata();
            DumpAllText.Text_AllProp();

            DumpMainload.AllBuilddata();
            DumpAllText.Text_AllBuild();

            DumpAllText.Text_AllPropClass();
        }
    }
}
