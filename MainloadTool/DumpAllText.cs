using System.Collections.Generic;

namespace MainloadTool;

internal class DumpAllText {

    private static List<string> _languages = ["zh-CN","en-US"];

    public static void Text_AllProp (){
        var csv = new CsvWriter("Text_AllProp" + MainloadTool.GameVersion);

        var title = new List<string> { "序号" };
        title.AddRange(_languages);
        csv.WriteLine(title);

        var count = AllText.Text_AllProp.Count;
        for (var i = 0; i < count; i++) {
            csv.WriteField(i.ToString());
            csv.WriteField(AllText.Text_AllProp[i]);

            csv.EndRow();
        }

        csv.Save();
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
        var csv = new CsvWriter("Text_AllPropClass" + MainloadTool.GameVersion);

        var title = new List<string> { "序号" };
        title.AddRange(_languages);
        csv.WriteLine(title);

        var count = AllText.Text_AllPropClass.Count;
        for (var i = 0; i < count; i++) {
            csv.WriteField(i.ToString());
            csv.WriteField(AllText.Text_AllPropClass[i]);

            csv.EndRow();
        }

        csv.Save();
    }
}
