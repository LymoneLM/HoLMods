// Source Code is taken from Valheim-Modding - https://github.com/Valheim-Modding/CommonPackages
#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;

namespace NewtonsoftJsonPlugin;

[BepInPlugin(MODGUID, MODNAME, VERSION)]
public class NewtonsoftJsonPlugin : BaseUnityPlugin
{
    public const string MODNAME = "NewtonsoftJsonPlugin";
    public const string MODGUID = "cc.lymone.HoL." + MODNAME;
    public const string VERSION = "1.0.0";
    
    private const string AssemblyName = "Newtonsoft.Json";

    private void Awake()
    {
        if (IsAssemblyLoaded(out var assembly) && assembly != null)
        {
            Logger.LogInfo($"{AssemblyName} {assembly.GetName().Version} assembly already loaded from {RelativePath(assembly.Location)}");
            return;
        }

        var path = Path.Combine(Path.GetDirectoryName(Info.Location) ?? string.Empty, $"{AssemblyName}.dll");

        if (File.Exists(path))
        {
            Assembly.LoadFrom(path);
        }
        else
        {
            Assembly.Load(AssemblyName);
        }

        if (IsAssemblyLoaded(out assembly) && assembly != null)
        {
            Logger.LogInfo($"{AssemblyName} {assembly.GetName().Version} loaded from {RelativePath(assembly.Location)}");
        }
        else
        {
            Logger.LogError($"{AssemblyName} assembly not loaded");
        }
    }

    public static bool IsAssemblyLoaded(out Assembly? assembly)
    {
        assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == AssemblyName);
        return assembly != null;
    }

    public static string RelativePath(string path)
    {
        return path
            .Replace(Paths.BepInExRootPath, string.Empty)
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
