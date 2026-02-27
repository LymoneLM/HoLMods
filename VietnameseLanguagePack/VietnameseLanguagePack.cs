using System.IO;
using System.Reflection;
using YuanAPI;
using BepInEx;

namespace VietnameseLanguagePack;

[BepInDependency(YuanAPIPlugin.MODGUID, YuanAPIPlugin.VERSION)]
[BepInPlugin(MODGUID, MODNAME, VERSION)]
public class VietnameseLanguagePackPlugin : BaseUnityPlugin
{
    private const string MODNAME = "VietnameseLanguagePack";
    private const string MODGUID = "cc.lymone.HoL." + MODNAME;
    private const string VERSION = "1.0.0";

    private void Awake()
    {
        Localization.RegisterLocale("vi-VN", "Tiếng Việt", ["en-US"]);
        
        var executingAssembly = Assembly.GetExecutingAssembly();
        var modPath = Path.GetDirectoryName(executingAssembly.Location);
        Localization.LoadFromPath(modPath);
    }
}
