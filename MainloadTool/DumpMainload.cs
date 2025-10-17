using System.Collections.Generic;

namespace MainloadTool {

    internal static class DumpMainload {

        public static void AllPropdata() {
            var csv = new CsvWriter("AllPropdata"+MainloadTool.GameVersion);

            csv.WriteLine(new List<string>{
                "序号","价格","分类",
                "文","武","商","艺",
                "健康","心情","魅力","幸运","延年益寿",
                "计谋","巫","医","相","卜","魅","工"
            });

            var count = Mainload.AllPropdata.Count;
            for (var i = 0; i < count; i++) {
                csv.WriteField(i.ToString());

                csv.WriteField(Mainload.AllPropdata[i][0]);
                csv.WriteField(Mainload.AllPropdata[i][1]);

                csv.WriteField(Mainload.AllPropdata[i][2].Split('|'));

                csv.EndRow();
            }

            csv.Save();
        }

        public static void AllBuilddata() {
            var csv = new CsvWriter("AllBuilddata" + MainloadTool.GameVersion);
            var csv7 = new CsvWriter("AllBuilddata[7]" + MainloadTool.GameVersion);

            csv.WriteLine(new List<string> {
                "序号","null_1","价格","升级基准","列3",
                "解锁等级","分类","null_2","属性",
                "生活品质","列9"
            });

            var title7 = new List<string>{ "序号" };
            var dic7 = new Dictionary<int, string> {
                {0, "文"},
                {1, "武"},
                {2, "商"},
                {3, "艺"},
                {4, "长寿"},
                {5, "爱情"},
                {6, "幸运"},
                {7, "多子"},
                {8, "魅力"},
                {10, "繁荣"},
                {11, "声望"},
                {12, "环境"},
                {13, "安全"},
                {14, "便捷"},
                {16, "教化"},
                {17, "厢房"},
                {18, "农舍"},
                {19, "仓库"},
                {20, "马厩"}
            };
            var dic9 = new Dictionary<int,string> {
                {1, "刚正"},
                {3, "善良"},
                {4, "真诚"},
                {6, "高洁"}
            };
            var len7 = Mainload.AllBuilddata[0][7].Split('|').Length;
            for (var x = 0; x < len7; x++) {
                if (x != 9) {
                    title7.Add(dic7.TryGetValue(x,out var t) ? t : $"{x}");
                }else {
                    var len79 = Mainload.AllBuilddata[0][7].Split('|')[9].Split('@').Length;
                    for (var y = 0; y < len79; y++) {
                        title7.Add(dic9.TryGetValue(y,out var t) ? t : $"{x}_{y}");
                    }
                }
            }
            csv7.WriteLine(title7);

            var count = Mainload.AllBuilddata.Count;
            for (var i = 0; i < count; i++) {
                csv.WriteField(i.ToString());
                csv7.WriteField(i.ToString());

                if (i != 161) {
                    csv.WriteField(Mainload.AllBuilddata[i]);

                    var array = Mainload.AllBuilddata[i][7].Split('|');
                    for (var x = 0; x < len7; x++) {
                        if (x != 9) {
                            csv7.WriteField(array[x]);
                        } else {
                            csv7.WriteField(array[9].Split('@'));
                        }
                    }
                }

                csv.EndRow();
                csv7.EndRow();
            }

            csv.Save();
            csv7.Save();
        }
    }
}
