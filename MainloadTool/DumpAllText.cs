using System.Collections.Generic;
using YuanAPI;

namespace MainloadTool;

internal class DumpAllText
{

    internal static List<string> _languages;

    private static void DumpText(string name, List<List<string>> texts)
    {
        var csv = new CsvWriter(name + MainloadTool.GameVersion);
        
        var title = new List<string> { "序号" };
        title.AddRange(_languages);
        csv.WriteLine(title);
        
        var count = texts.Count;
        for (var i = 0; i < count; i++) {
            csv.WriteField(i.ToString());
            csv.WriteField(texts[i]);

            csv.EndRow();
        }
        
        csv.Save();
    }

    public static void Text_AllProp (){
        DumpText("Text_AllProp", AllText.Text_AllProp);
    }

    public static void Text_AllBuild() {
        var csv = new CsvWriter("Text_AllBuild" + MainloadTool.GameVersion);

        var title = new List<string> { "序号" };
        foreach (var lang in _languages) {
            title.Add(lang + "建筑");
            title.Add(lang + "简介");
        }
        csv.WriteLine(title);

        var count = AllText.Text_AllBuild.Count;
        for (var i = 0; i < count; i++) {
            csv.WriteField(i.ToString());

            foreach (var text in AllText.Text_AllBuild[i]) {
                csv.WriteField(text.Split('|'));
            }

            csv.EndRow();
        }

        csv.Save();
    }

    public static void Text_AllPropClass() {
        DumpText("Text_AllPropClass", AllText.Text_AllPropClass);
    }

    public static void Text_TipShow()
    {
        DumpText("Text_TipShow", AllText.Text_TipShow);
    }
}
