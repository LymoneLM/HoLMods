using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PanelTweak;

public class SelectSavePanel : MonoBehaviour
{
    public SaveCell saveCellPrefab;
    public Transform saveCellContent;
    
    public Text title;
    public Button closeBT;
    public Button backBT;
    public RemoveSaveAlert removeSaveAlert;

    internal static SelectSavePanel Instance { get; private set; }
    internal static bool LegacyMode = true;

    private GameObject _loadPanel;
    private GameObject _startGameUI;
    private GameObject _initGameUI;

    private List<SaveCell> _saveList = [];
    private SimplePool<SaveCell> _savePool;

    private void Awake()
    {
        Instance = this;
        _loadPanel = transform.parent.Find("LoadPanel").gameObject;
        _startGameUI = transform.parent.Find("StartGameUI").gameObject;
        _initGameUI = transform.parent.Find("InitGameUI").gameObject;

        _savePool = new SimplePool<SaveCell>(saveCellPrefab);

        closeBT.onClick.AddListener(CloseBT);
        backBT.onClick.AddListener(CloseBT);

        title.font = FontManager.BoldFont;
    }

    private void OnEnable()
    {
        ReloadSaveList();
        removeSaveAlert.gameObject.SetActive(false);

        title.text = AllText.Text_UIA[27][Mainload.SetData[4]];
        transform.localPosition = new Vector3(0f, 500f, 0f);
        transform.DOLocalMoveY(0f, 0.3f).SetEase(Ease.OutBack, 1f);
    }

    private void ReloadSaveList()
    {
        var count = _saveList.Count;
        for (var i = count; i > 0; --i)
        {
            _savePool.Return(_saveList[i - 1]);
            _saveList.RemoveAt(i - 1);
        }

        if (LegacyMode)
        {
            for (var i = 0; i < 6; i++)
                LoadSaveCell($"FW/{i}");
        }
        else
        {
            var saves = ES3.GetDirectories("FW/");
            Array.Sort(saves);
            foreach (var savePath in saves)
                LoadSaveCell($"FW/{savePath}");
        }
    }

    private void LoadSaveCell(string savePath)
    {
        var saveCell = _savePool.Get();
        saveCell.transform.SetParent(saveCellContent);
        saveCell.transform.localPosition = Vector3.zero;
        saveCell.transform.localScale = Vector3.one;
        saveCell.SetSave(savePath);
        _saveList.Add(saveCell);
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
        removeSaveAlert.OnRemove = () =>
        {
            ES3.DeleteDirectory(savePath);
            ReloadSaveList();
        };
        removeSaveAlert.gameObject.SetActive(true);
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