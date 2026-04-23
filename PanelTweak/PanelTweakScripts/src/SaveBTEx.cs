using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PanelTweak;

public class SaveBTEx : MonoBehaviour
{
    private Transform _iconRoot;
        
    private Text _name;
    private Text _level;
    private Text _date;
    private Text _tip;
    private bool _haveSave;
    private string _savePath;
        
    private List<string> _familyData = new List<string>();
    private List<string> _memberFirst = new List<string>();
    private List<int> _saveDate = new List<int>();

    protected virtual void Awake()
    {
        var dataShow = transform.Find("DataShow");
        _iconRoot = dataShow.Find("IconShow");
        _name = dataShow.Find("Name").GetComponent<Text>();
        _level = dataShow.Find("Lv").GetComponent<Text>();
        _date = dataShow.Find("Time").GetComponent<Text>();
        _tip = transform.Find("NoDataTip").Find("Tip").GetComponent<Text>();
            
        transform.Find("BackBT").GetComponent<Button>().onClick.AddListener(ClickBT);
        transform.Find("RemoveBT").GetComponent<Button>().onClick.AddListener(RemoveBT);
    }

    private void OnEnable()
    {
        UpdateFontSize();
        UpdateShow();
    }
        
    private void UpdateFontSize()
    {
        if (Mainload.SetData[4] == 0)
        {
            _name.fontSize = 36;
            _level.fontSize = 26;
            _date.fontSize = 26;
        }
        else
        {
            _name.fontSize = 34;
            _level.fontSize = 24;
            _date.fontSize = 24;
        }
    }

    public void SetSave(string path)
    {
        _savePath = path;
        _haveSave = true;
        try
        {
            _familyData = ES3.Load<List<string>>("FamilyData", path + "/GameData.es3");
            _saveDate = ES3.Load<List<int>>("Time_now", path + "/GameData.es3");
            _memberFirst = ES3.Load<List<string>>("Member_First", path + "/GameData.es3");
        }
        catch (Exception)
        {
            _haveSave = false;
        }
        UpdateShow();
    }

    private void UpdateShow()
    {
        _tip.text = AllText.Text_UIA[31][Mainload.SetData[4]];
            
        if (_haveSave)
        {
            ShowMemberImage();
            var addressParts = _familyData[0].Split('|');
            _name.text = AllText.Text_UIA[29][Mainload.SetData[4]]
                .Replace("@", AllText.Text_City[int.Parse(addressParts[0])][Mainload.SetData[4]]
                    .Split('~')[1].Split('|')[int.Parse(addressParts[1])])
                .Replace("$", _familyData[1]);
            _level.text = AllText.Text_UIA[28][Mainload.SetData[4]]
                .Replace("@", _familyData[2]);
            _date.text = AllText.Text_UIA[975][Mainload.SetData[4]]
                .Replace("@", _saveDate[0].ToString())
                .Replace("$", AllText.Text_Months[_saveDate[1]][Mainload.SetData[4]])
                .Replace("~", _saveDate[2].ToString());
        }
            
        transform.Find("DataShow").gameObject.SetActive(_haveSave);
        transform.Find("NoDataTip").gameObject.SetActive(!_haveSave);
    }

    private void RemoveBT()
        => SelectSavePanel.Instance.RemoveSave(_savePath);
        
    private void ClickBT() 
        => SelectSavePanel.Instance.OpenSave(_savePath, _haveSave);

    private void ShowMemberImage()
    {
        var count = _iconRoot.childCount;
        for (var i = 0; i < count; i++)
            Destroy(_iconRoot.GetChild(i).gameObject);

        var genderStr = _memberFirst[5];
        var old = int.Parse(_memberFirst[6]);
        var imageParts = _memberFirst[2].Split('|');
        var clothParts = _memberFirst[4].Split('|')[0].Split('@');
        var bodyStr = ((clothParts[0] != "5")
            ? "null"
            : ((int.Parse(imageParts[1]) % 2 != 0)
                ? ("B/" + clothParts[1])
                : ("A/" + clothParts[1])));
            
        var stage = old < Mainload.OldFenjie[0] ? 0 :
            old < Mainload.OldFenjie[1] ? 1 :
            old < Mainload.OldFenjie[2] ? 2 : 3;

        var stageStr = stage.ToString();
        var midStageStr = Mathf.Min(stage, 2).ToString();

        string[] iconPath =
        {
            "AllLooks/Member_B/" + genderStr + "/" + stageStr + "/houfa/" + imageParts[0],
            ((bodyStr == "null")
                ? ("AllLooks/Member_B/" + genderStr + "/" + midStageStr + "/shen/" + imageParts[1])
                : ("AllLooks/Member_B/" + genderStr + "/5/" + bodyStr)),
            "AllLooks/Member_B/" + genderStr + "/" + midStageStr + "/tou/" + imageParts[2],
            "AllLooks/Member_B/" + genderStr + "/" + midStageStr + "/PX/" + _memberFirst[3],
            "AllLooks/Member_B/" + genderStr + "/" + stageStr + "/qianfa/" + imageParts[3]
        };
            
        foreach (var path in iconPath)
        {
            var obj = Instantiate(Resources.Load<GameObject>(path), _iconRoot);
            obj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            obj.transform.localPosition = new Vector3(0f, 0f, 0f);
        }
    }
}