using HarmonyLib;

namespace MoreTimeSpeed;

[HarmonyPatch(typeof(MainUI_DownBT))]
public class MainUIDownBTPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("PlayXNumShow")]
    public static void Postfix()
    {
        Mainload.PlayTimeRun[1] = Mainload.PlayTimeRun[1] switch
        {
            3 => MoreTimeSpeed.SpeedFor3X.Value,
            5 => MoreTimeSpeed.SpeedFor5X.Value,
            10 => MoreTimeSpeed.SpeedFor10X.Value,
            _ => Mainload.PlayTimeRun[1]
        };
    }

    [HarmonyPrefix]
    [HarmonyPatch("InitShow")]
    public static void Prefix()
    {
        Mainload.PlayTimeRun[1] = 1;
    }
}