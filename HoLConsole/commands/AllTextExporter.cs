using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HoLConsole;

public static class AllTextExporter
{
    // ── 与 Localization 模块保持一致 ────────────────────────────────────────────
    private static readonly List<string> Locales = ["zh-CN", "en-US"];
    private const string VanillaNamespace = "Vanilla";

    public static void Register()
    {
        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "export-AllText",
            description: "[调试指令]将游戏内置的 AllText 原版文本导出为符合YuanAPI格式的嵌套 JSON 文件",
            usage: "export-AllText",
            handler: ExportAllTextCommand
        ));
    }

    private static string ExportAllTextCommand(CommandContext ctx, ParsedArgs args)
    {
        try
        {
            // 未传路径时默认导出到程序集所在目录
            var outputRoot = Path.Combine("Output");

            ctx.Print($"开始导出，目标目录：{outputRoot}", ConsoleLevel.Info);

            Export(outputRoot, ctx);

            return "导出完成";
        }
        catch (Exception ex)
        {
            ctx.Logger.LogError($"Error: {ex.Message}");
            return $"Error：{ex.Message}";
        }
    }

    /// <summary>
    /// 将 AllText 中所有原版文本导出为嵌套 JSON 文件。
    /// <br/>输出结构：{outputRoot}/locales/{locale}/Vanilla.json
    /// </summary>
    private static void Export(string outputRoot, CommandContext ctx)
    {
        // ── 按语言准备根对象 ────────────────────────────────────────────────────
        var roots = new JObject[Locales.Count];
        for (var i = 0; i < Locales.Count; i++)
            roots[i] = new JObject();

        // ── 1. 普通字段：List<List<string>> ─────────────────────────────────────
        var vanillaFields = typeof(AllText)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(List<List<string>>))
            .ToList();

        if (vanillaFields.Count == 0)
            ctx.Logger.LogWarning("未能找到任何 List<List<string>> 字段，AllText 可能尚未初始化");

        foreach (var field in vanillaFields)
        {
            var fieldName = field.Name;
            var items = (List<List<string>>)field.GetValue(null);

            var fieldObjs = new JObject[Locales.Count];
            for (var i = 0; i < Locales.Count; i++)
                fieldObjs[i] = new JObject();

            for (var index = 0; index < items.Count; index++)
            {
                var item = items[index];
                for (var langIdx = 0; langIdx < Locales.Count; langIdx++)
                {
                    var text = (item != null && langIdx < item.Count) ? (item[langIdx] ?? "") : "";
                    fieldObjs[langIdx][index.ToString()] = text;
                }
            }

            for (var langIdx = 0; langIdx < Locales.Count; langIdx++)
                roots[langIdx][fieldName] = fieldObjs[langIdx];
        }

        ctx.Print($"已处理 {vanillaFields.Count} 个普通字段", ConsoleLevel.Info);

        // ── 2. 特殊字段：Text_AllShenFen（List<List<List<string>>>）──────────────
        try
        {
            var allShenFen = AllText.Text_AllShenFen;

            var shenFenObjs = new JObject[Locales.Count];
            for (var i = 0; i < Locales.Count; i++)
                shenFenObjs[i] = new JObject();

            for (var gIndex = 0; gIndex < allShenFen.Count; gIndex++)
            {
                var group = allShenFen[gIndex];

                var groupObjs = new JObject[Locales.Count];
                for (var i = 0; i < Locales.Count; i++)
                    groupObjs[i] = new JObject();

                for (var iIndex = 0; iIndex < group.Count; iIndex++)
                {
                    var item = group[iIndex];
                    for (var langIdx = 0; langIdx < Locales.Count; langIdx++)
                    {
                        var text = (item != null && langIdx < item.Count) ? (item[langIdx] ?? "") : "";
                        groupObjs[langIdx][iIndex.ToString()] = text;
                    }
                }

                for (var langIdx = 0; langIdx < Locales.Count; langIdx++)
                    shenFenObjs[langIdx][gIndex.ToString()] = groupObjs[langIdx];
            }

            for (var langIdx = 0; langIdx < Locales.Count; langIdx++)
                roots[langIdx]["Text_AllShenFen"] = shenFenObjs[langIdx];

            ctx.Print("已处理特殊字段 Text_AllShenFen", ConsoleLevel.Info);
        }
        catch (Exception ex)
        {
            ctx.Logger.LogWarning($"Text_AllShenFen 处理失败：{ex.Message}");
        }

        // ── 3. 写文件 ─────────────────────────────────────────────────────────
        for (var langIdx = 0; langIdx < Locales.Count; langIdx++)
        {
            var locale = Locales[langIdx];
            var dir = Path.Combine(outputRoot, "locales", locale);
            Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, $"{VanillaNamespace}.json");

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            using var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            using var jw = new JsonTextWriter(sw) { Formatting = Formatting.Indented, Indentation = 2 };

            roots[langIdx].WriteTo(jw);

            ctx.Print($"已写出：{filePath}", ConsoleLevel.Info);
        }
    }
}
