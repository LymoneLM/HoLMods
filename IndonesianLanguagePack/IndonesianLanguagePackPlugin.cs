using System.IO;
using System.Reflection;
using YuanAPI;
using BepInEx;

namespace IndonesianLanguagePack;

[BepInDependency(YuanAPIPlugin.MODGUID, YuanAPIPlugin.VERSION)]
[BepInPlugin(MODGUID, MODNAME, VERSION)]
public class IndonesianLanguagePackPlugin : BaseUnityPlugin
{
    private const string MODNAME = "IndonesianLanguagePack";
    private const string MODGUID = "cc.lymone.HoL." + MODNAME;
    private const string VERSION = "1.0.0";

    private void Awake()
    {
        Localization.RegisterLocale("id-ID", "Bahasa Indonesia", ["en-US"]);
        
        var executingAssembly = Assembly.GetExecutingAssembly();
        var modPath = Path.GetDirectoryName(executingAssembly.Location);
        Localization.LoadFromPath(modPath);
    }
}
