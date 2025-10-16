using System;
using System.Collections.Generic;
using System.Text;

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

            csv.WriteLine(new List<string> {
                "序号","null","价格","null","null",
                "解锁等级","分类","null","null",
                "生活品质","null"
            });

            var count = Mainload.AllBuilddata.Count;
            for (var i = 0; i < count; i++) {
                csv.WriteField(i.ToString());

                if (Mainload.AllBuilddata[i] != null) 
                    csv.WriteField(Mainload.AllBuilddata[i]);

                csv.EndRow();
            }

            csv.Save();
        }
    }
}
