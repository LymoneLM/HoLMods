using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PanelTweak;

public class SelectSavePanel : MonoBehaviour
{
    internal static SelectSavePanel Instance { get; private set; }
    internal static bool LegacyMode = false;
        
    private GameObject _loadPanel;
    private GameObject _startGameUI;
    private GameObject _initGameUI;

    private Text _title;
    private GameObject _delCunDPanel;

    private Transform _saveContext;
    private List<SaveBTEx> _saveList = new List<SaveBTEx>();
    private SimplePool<SaveBTEx> _savePool;
        
    private void Awake()
    {
        Instance = this;
        _loadPanel = transform.parent.Find("LoadPanel").gameObject;
        _startGameUI = transform.parent.Find("StartGameUI").gameObject;
        _initGameUI = transform.parent.Find("InitGameUI").gameObject;

        _title = transform.Find("Title").GetComponent<Text>();
        _delCunDPanel = transform.Find("DelCunDPanel").gameObject;
            
        var prefab = Resources.Load<GameObject>("PanelTweak/SaveCell").GetComponent<SaveBTEx>();
        _savePool = new SimplePool<SaveBTEx>(prefab);
            
        _saveContext = transform.Find("ScrollView/ViewPort/Content");
        for(var i = _saveContext.childCount; i > 0; --i)
            Destroy(_saveContext.GetChild(i-1).gameObject);
            
        transform.Find("CloseBT").GetComponent<Button>().onClick.AddListener(CloseBT);
        transform.Find("Back").GetComponent<Button>().onClick.AddListener(CloseBT);
    }

    private void OnEnable()
    {
        ReloadSaveList();
        _delCunDPanel.SetActive(false);
            
        _title.text = AllText.Text_UIA[27][Mainload.SetData[4]];
        transform.localPosition = new Vector3(0f, 500f, 0f);
        transform.DOLocalMoveY(0f, 0.3f).SetEase(Ease.OutBack, 1f);
    }

    private void ReloadSaveList()
    {
        var count = _saveList.Count;
        for (var i = count; i > 0; --i)
        {
            _savePool.Return(_saveList[i-1]);
            _saveList.RemoveAt(i-1);
        }
            
        if (LegacyMode)
        {
            for (var i = 0; i < 6; i++)
            {
                var saveBT = _savePool.Get();
                saveBT.transform.SetParent(_saveContext);
                saveBT.SetSave($"FW/{i}");
                _saveList.Add(saveBT);
            }
        }
        else
        {
            var saves = ES3.GetDirectories("FW/");
            Array.Sort(saves);
            foreach (var savePath in saves)
            {
                var saveBT = _savePool.Get();
                saveBT.transform.SetParent(_saveContext);
                saveBT.SetSave($"FW/{savePath}");
                _saveList.Add(saveBT);
            }
        }
    }

    internal void OpenSave(string savePath, bool haveSave)
    {
        Mainload.CunDangIndex_now = savePath;
        if (haveSave)
        {
            Mainload.Guide_order = 10000;

            SaveData.ReadGameData();
            InitRunData();
            _loadPanel.GetComponent<LoadPanel>().ShowID = 0;
            _loadPanel.SetActive(true);
            gameObject.SetActive(false);
            return;
        }

        Mainload.Guide_order = Mainload.Guide_order == 10000 ? 10000 : 0;

        if (Mainload.isFirstGame)
        {
            Mainload.SceneID = "M|0";
            _loadPanel.GetComponent<LoadPanel>().ShowID = 1;
            _loadPanel.SetActive(true);
            DefaultSaveData.Load(this);
            _loadPanel.GetComponent<LoadPanel>().SwitchScene();
        }
        else
        {
            _initGameUI.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    internal void RemoveSave(string savePath)
    {
        Mainload.CunDangIndex_now = savePath;
        _delCunDPanel.SetActive(true);
    }
        
    private static void InitRunData()
    {
        for (var i = 0; i < Mainload.NongZ_now.Count; i++)
        {
            for (var j = 0; j < Mainload.NongZ_now[i].Count; j++)
            {
                if (Mainload.NongZ_now[i][j][0] == "-1")
                {
                    Mainload.NongzHaveData.Add(i + "|" + j);
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(Mainload.FastKey[0]) && !_loadPanel.activeSelf)
        {
            CloseBT();
        }
    }

    private void CloseBT()
    {
        _startGameUI.SetActive(true);
        gameObject.SetActive(false);
    }
}