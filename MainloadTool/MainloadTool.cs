using System;
using BepInEx;
using BepInEx.Logging;
using YuanAPI;
using UnityEngine;

namespace MainloadTool;

[BepInPlugin(MODGUID,MODNAME,VERSION)]
[BepInDependency(YuanAPIPlugin.MODGUID)]
public class MainloadTool : BaseUnityPlugin{
    public const string MODNAME = "MainloadTool";
    public const string MODGUID = "cc.lymone.HoL." + MODNAME;
    public const string VERSION = "1.1.0";

    internal new static ManualLogSource Logger;
    internal static string GameVersion;

    private void Awake() {
        Logger = base.Logger;
        GameVersion = "_v" + Mainload.Vision_now.Substring(2);
        
        Localization.Initialize();
    }

    private void Start()
    {
        DumpAllText._languages = Localization.GetAllLocales();
    }
    
    #region UI

    internal bool ShowMenu { get; private set; }
    
    private Rect _windowRect = new Rect(150, 100, 800f, 600f);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home))
        {
            ShowMenu = !ShowMenu;

            if (ShowMenu)
            {
                // 关闭地图面板，避免误操作
                Mainload.isMapPanelOpen = false;
            }
            
            Logger.LogInfo($"MainloadTool窗口已{(ShowMenu?"打开":"关闭")}" );
        }
        
        if (!ShowMenu) 
            return;
        
        if (Input.mouseScrollDelta.y != 0 || // 阻止鼠标滚轮
            Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2) && // 阻止鼠标点击
            Input.anyKeyDown && !Input.GetKeyDown(KeyCode.F2)) // 阻止键盘输入（保留F2键用于关闭窗口）
        {
            Input.ResetInputAxes();
        }
    }
    private void OnGUI()
    {
        if (!ShowMenu) 
            return;
        
        // 保存窗口背景色并设置为半透明
        var originalBackgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 0.95f);

        {
            // 背景遮罩
            GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
            GUI.color = new Color(0, 0, 0, 0.1f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.EndGroup();
            
            
            // 窗体内边距
            GUI.skin.window.padding = new RectOffset(
                Mathf.RoundToInt(20),
                Mathf.RoundToInt(20),
                Mathf.RoundToInt(10),
                Mathf.RoundToInt(10)
            );
            
            // 字体字号
            var fontSize = Mathf.RoundToInt(18);
            GUI.skin.textField.fontSize = fontSize;
            GUI.skin.window.fontSize = fontSize;
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.button.fontSize = fontSize;
            GUI.skin.button.alignment = TextAnchor.MiddleCenter; 

            // 创建窗口
            _windowRect = GUI.Window(0, _windowRect, DrawWindow, "", GUI.skin.window);
        }
        
        // 恢复原始背景色
        GUI.backgroundColor = originalBackgroundColor;
    }

    private void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        // 窗口最上方标题文本
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        GUILayout.FlexibleSpace();
        var titleStyle = new GUIStyle(GUI.skin.label);
        GUILayout.Label($"Mainload Tool v{VERSION} for Game {GameVersion.Substring(1)}",
            titleStyle, GUILayout.ExpandWidth(false));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(20f);
        
        
        {
            GUILayout.BeginHorizontal(); // 两个分区水平排列

            // 左分区
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(0), GUILayout.ExpandWidth(true));
            GUILayout.Label("Dump AllText", GUILayout.Width(350f));
            GUILayout.Space(5);
            {
                if (GUILayout.Button("Text_AllProp", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    DumpAllText.Text_AllProp();
                }
                
                GUILayout.Space(5);
                
                if (GUILayout.Button("Text_AllBuild", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    DumpAllText.Text_AllBuild();
                }
                
                GUILayout.Space(5);
                
                if (GUILayout.Button("Text_AllPropClass", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    DumpAllText.Text_AllPropClass();
                }
                
                GUILayout.Space(5);
                
                if (GUILayout.Button("Text_TipShow", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    DumpAllText.Text_TipShow();
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(10);

            // 右分区
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(0), GUILayout.ExpandWidth(true));
            GUILayout.Label("Dump Mainload", GUILayout.Width(350f));
            GUILayout.Space(5);
            {
                if (GUILayout.Button("AllPropData", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    DumpMainload.AllPropdata();
                }
                
                GUILayout.Space(5);
                
                if (GUILayout.Button("AllBuilddata", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                {
                    DumpMainload.AllBuilddata();
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }
        
        
        GUILayout.EndVertical();
        
        // 允许拖动窗口
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, _windowRect.height));
    }
    
    #endregion
}
