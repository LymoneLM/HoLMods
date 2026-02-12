using UnityEngine;

namespace HoLConsole;

public static class DebugCommands
{
    public static void Register()
    {
        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "kill-TouMing",
            description: "[调试指令]删除本场景里所有建筑节点TouMing下属的对象(删除刚体，郡城无效)",
            usage: "kill-TouMing",
            handler: KillTouMingHandler
        ));
        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "kill-AllDrag",
            description: "[调试指令]删除本场景里所有建筑节点AllDrag下属的对象(删除刚体，郡城无效)",
            usage: "kill-AllDrag",
            handler: KillAllDragHandler
        ));
        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "kill-LineCollider",
            description: "[调试指令]删除本场景里所有建筑节点LineCollider下属的对象(删除刚体，郡城无效)",
            usage: "kill-LineCollider",
            handler: KillLineColliderHandler
        ));
    }

    public static string KillSomething(CommandContext ctx, string rmTarget)
    {
        // 1) 找到 AllBuild/BackMap（常驻）
        var allBuildGo = GameObject.Find("AllBuild");
        if (allBuildGo == null)
        {
            ctx.Logger.LogWarning("[ClearTouMingChildren] Cannot find GameObject: AllBuild");
            return "失败";
        }

        var backMap = allBuildGo.transform.Find("BackMap");
        if (backMap == null)
        {
            ctx.Logger.LogWarning("[ClearTouMingChildren] Cannot find Transform: AllBuild/BackMap");
            return "失败";
        }

        // 2) BackMap 下一层只有一个对象（如 25(Clone)）
        if (backMap.childCount < 1)
        {
            ctx.Logger.LogWarning("[ClearTouMingChildren] BackMap has no child.");
            return "失败";
        }
        if (backMap.childCount > 1)
            ctx.Logger.LogWarning($"[ClearTouMingChildren] BackMap has {backMap.childCount} children, expected 1. Will use the first.");

        Transform cloneRoot = backMap.GetChild(0);

        // 3) 进入 BuildShow（名字固定）
        var buildShow = cloneRoot.Find("BuildShow");
        if (buildShow == null)
        {
            ctx.Logger.LogWarning($"[ClearTouMingChildren] Cannot find Transform: {cloneRoot.name}/BuildShow");
            return "失败";
        }

        // 4) BuildShow 下一层数量不定名字不定：完全遍历
        int clearedTouMingCount = 0;
        int removedChildTotal = 0;

        for (int i = 0; i < buildShow.childCount; i++)
        {
            Transform levelA = buildShow.GetChild(i);

            // 5) 这一层里面还会套一层名字不定的对象（一定只有一个）
            if (levelA.childCount < 1)
            {
                ctx.Logger.LogWarning($"[ClearTouMingChildren] Skip '{levelA.name}' because it has no child (expected 1).");
                continue;
            }
            if (levelA.childCount > 1)
                ctx.Logger.LogWarning($"[ClearTouMingChildren] '{levelA.name}' has {levelA.childCount} children, expected 1. Will use the first.");

            Transform levelB = levelA.GetChild(0);

            // 6) 找到 目标节点（只清空其子节点）
            Transform target = levelB.Find(rmTarget);
            if (target == null)
            {
                ctx.Logger.LogInfo($"[ClearTouMingChildren] Not found UI/TouMing under: {GetHierarchyPath(levelB)}");
                continue;
            }

            int removedHere = DestroyAllChildren(target);
            if (removedHere > 0)
            {
                ctx.Logger.LogInfo($"[ClearTouMingChildren] Cleared {removedHere} child(ren) under: {GetHierarchyPath(target)}");
            }

            clearedTouMingCount++;
            removedChildTotal += removedHere;
        }

        ctx.Logger.LogInfo($"[ClearTouMingChildren] Done. TouMing found = {clearedTouMingCount}, children removed total = {removedChildTotal}");
        return $"成功移除{removedChildTotal}个对象";
    }

    public static string KillTouMingHandler(CommandContext ctx, ParsedArgs _)
    {
        return KillSomething(ctx, "UI/TouMing");
    }
    
    public static string KillAllDragHandler(CommandContext ctx, ParsedArgs _)
    {
        return KillSomething(ctx, "UI/AllDrag");
    }
    
    public static string KillLineColliderHandler(CommandContext ctx, ParsedArgs _)
    {
        return KillSomething(ctx, "LineCollider");
    }
    
    private static int DestroyAllChildren(Transform parent)
    {
        if (parent == null) return 0;

        int removed = 0;

        // 从后往前，避免 childCount/索引变化
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child == null) continue;

            Object.Destroy(child.gameObject);
            removed++;
        }

        return removed;
    }
    
    private static string GetHierarchyPath(Transform t)
    {
        if (t == null) return "(null)";
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
