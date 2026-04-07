using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace MoreTimeSpeed;

[BepInPlugin(MODGUID, MODNAME, VERSION)]
public class MoreTimeSpeed : BaseUnityPlugin
{
    public const string MODNAME = "MoreTimeSpeed";
    public const string MODGUID = "cc.lymone.HoL." + MODNAME;
    public const string VERSION = "1.0.0";

    internal static readonly Harmony Harmony = new(MODGUID);

    public static ConfigEntry<int> SpeedFor3X = null!;
    public static ConfigEntry<int> SpeedFor5X = null!;
    public static ConfigEntry<int> SpeedFor10X = null!;

    private static AcceptableValueList<int> _acceptableValueList =
        new (1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 16, 20, 24, 30, 40, 48, 60, 80, 120, 240);

    private void Awake()
    {
        SpeedFor3X = Config.Bind("Speed", "Speed For 03x", 3,
            new ConfigDescription("Actual speed used when selecting 3x", _acceptableValueList)
            );

        SpeedFor5X = Config.Bind("Speed", "Speed For 05x", 5,
            new ConfigDescription("Actual speed used when selecting 5x", _acceptableValueList)
            );

        SpeedFor10X = Config.Bind("Speed", "Speed For 10x", 10,
            new ConfigDescription("Actual speed used when selecting 10x", _acceptableValueList)
            );

        Harmony.PatchAll();
    }
}