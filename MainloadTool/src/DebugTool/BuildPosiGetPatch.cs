using HarmonyLib;
using UnityEngine;

namespace MainloadTool;

[HarmonyPatch(typeof(BuildPosiGet))]
public class BuildPosiGetPatch
{
    private static int _counter = 0;
    private static PerBackMapScene _scene;
    
    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    public static void Postfix(BuildPosiGet __instance)
    {
        if (++_counter < 9) return;
        _counter = 0;

        var worldPosi =
            __instance.transform.InverseTransformPoint(Camera.main!.ScreenToWorldPoint(Input.mousePosition));

        if (_scene == null)
            _scene = __instance.transform.parent.GetComponent<PerBackMapScene>();
        
        MainloadTool.Logger.LogInfo($"[DebugTool] Current Position: {(Vector2)worldPosi} | ({_scene.PoisA_mouse}, {_scene.PoisB_mouse})");
    }
}