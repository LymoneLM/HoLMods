using UnityEngine;
using UnityEngine.UI;

namespace MemorialBiography {
    public class EventPanel : MonoBehaviour {
        public new int name = -1;
        public void Start() {
            if (Mainload.SetData[4] == 0) {
                base.transform.Find("TipA").GetComponent<Text>().fontSize = 18;
            } else {
                base.transform.Find("TipA").GetComponent<Text>().fontSize = 16;
            }
            base.transform.Find("TipA").GetComponent<Text>().text =
                AllText.Text_UIA[1133][Mainload.SetData[4]];

            this.transform.Find("CloseBT").GetComponent<Button>().onClick.AddListener(CloseBT);
        }
        public void OnEnable() {
            OnEnableShow_JiShi();
        }
        public void OnEnableShow_JiShi() {
            string[] array = new string[0];
            if (this.name != -1 && Mainload.Member_Ci[this.name].Count > 3) {
                array = Mainload.Member_Ci[this.name][3].Split('|');
            } else {
                var old = Mainload.Member_Ci[this.name][0].Split('|')[3].Split('@')[0].Split('~')[3];
                array = new string[] {
                     old+"@-1@null@null"
                };
            }
            string text = "null";
            for (int i = 0; i < array.Length; i++) {
                string text2;
                if (int.Parse(array[i].Split(new char[] { '@' })[1]) >= 0) {
                    text2 = AllText.Text_UIA[1222][Mainload.SetData[4]].Replace("@", array[i].Split(new char[] { '@' })[0]).Replace("$", AllText.Text_AllMemberEvent[int.Parse(array[i].Split(new char[] { '@' })[1])][Mainload.SetData[4]].Split(new char[] { '|' })[0].Replace("@", array[i].Split(new char[] { '@' })[2]).Replace("$", array[i].Split(new char[] { '@' })[3]));
                } else {
                    text2 = AllText.Text_UIA[1223][Mainload.SetData[4]].Replace("@", array[i].Split(new char[] { '@' })[0]);
                }
                if (text == "null") {
                    text = text2;
                } else {
                    text = text + "\n" + text2;
                }
            }
            if (text == "null") {
                base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                    .Find("InfoShow")
                    .GetComponent<Text>()
                    .text = " ";
            } else {
                base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                    .Find("InfoShow")
                    .GetComponent<Text>()
                    .text = text;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                .Find("InfoShow")
                .GetComponent<RectTransform>());
            base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                .GetComponent<RectTransform>()
                .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50f + base.transform.Find("AllCanSelect").Find("Viewport").Find("Content")
                    .Find("InfoShow")
                    .GetComponent<RectTransform>()
                    .sizeDelta.y);
        }

        public void CloseBT() {
            this.name = -1;
            this.gameObject.SetActive(false);
        }
    }
}
