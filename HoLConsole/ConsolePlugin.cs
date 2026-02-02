#nullable enable
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YuanAPI;

namespace HoLConsole;

[BepInDependency(YuanAPIPlugin.MODGUID, YuanAPIPlugin.VERSION)]
[BepInPlugin(MODGUID, MODNAME, VERSION)]
public class ConsolePlugin : BaseUnityPlugin, IConsoleHost
{
    public const string MODNAME = "HoLConsole";
    public const string MODGUID = "cc.lymone.HoL." + MODNAME;
    public const string VERSION = "1.0.0";
    
    internal new static ManualLogSource Logger = null!;

    private ConfigEntry<KeyCode> _toggleKey = null!;
    private ConfigEntry<int> _maxOutputLines = null!;
    private ConfigEntry<int> _maxInputHistory = null!;
    private ConfigEntry<bool> _startOpen = null!;

    private bool _visible;
    private Rect _windowRect = new Rect(40, 40, 760, 420);
    private float _uiScale = 1f;
    private const float BASE_WIDTH = 1920f;
    private const float BASE_HEIGHT = 1080f;

    private Vector2 _scrollPos;
    private bool _shouldScrollToBottom;
    private float _lastScrollViewHeight;

    private string _input = "";
    private int _focusInputCounter;

    // Output
    private readonly List<ConsoleLine> _outputLines = new();
    private readonly ConcurrentQueue<ConsoleLine> _pendingLines = new();

    // Input history
    private readonly List<string> _inputHistory = new();
    private int _historyIndex = -1;
    private string _historyStash = "";

    // Autocomplete
    private string[] _candidates = Array.Empty<string>();
    private int _candidateIndex = -1;
    private string _sessionKey = "";

    private const string INPUT_CONTROL_NAME = "HoLConsole_Input";

    // Cached styles
    private GUIStyle _styleInfo = null!;
    private GUIStyle _styleWarn = null!;
    private GUIStyle _styleError = null!;
    private GUIStyle _styleWindow = null!;
    private GUIStyle _styleTextField = null!;
    private GUIStyle _stylePrompt = null!;
    
    // Textures
    private Texture2D _bgTexture = null!;
    private Texture2D _inputBgTexture = null!;

    private void Awake()
    {
        Logger = base.Logger;

        _toggleKey = Config.Bind("按键绑定 Bind Key", "命令行热键 Toggle Key",
            KeyCode.BackQuote,
            "唤醒命令行窗口\nKey to toggle the console window.");
        _maxOutputLines = Config.Bind("配置 Config", "最大输出行数 Max Output Lines",
            600);
        _maxInputHistory = Config.Bind("配置 Config", "最大输入历史 Max Input History",
            200);
        _startOpen = Config.Bind("配置 Config", "启动时开启命令行 Start Open",
            false);

        _visible = _startOpen.Value;

        ConsoleCore.Initialize(this, Logger);
        RegisterBuiltinCommands();
        
        ConsoleCore.Print("______________________________________________________");
        ConsoleCore.Print($"{MODNAME} v{VERSION}");
        ConsoleCore.Print("Copyright (c) 2026 LymoneLM | MIT Licensed Open Source");
        ConsoleCore.Print("GitHub Repository: https://github.com/LymoneLM/HoLMods");
        ConsoleCore.Print("______________________________________________________");
        
        Logger.LogInfo($"{MODNAME} ready. Press [{_toggleKey.Value}] to open.");
    }

    private void Update()
    {
        // 计算UI缩放比例
        float scaleX = Screen.width / BASE_WIDTH;
        float scaleY = Screen.height / BASE_HEIGHT;
        _uiScale = Mathf.Min(scaleX, scaleY);
        _uiScale = Mathf.Clamp(_uiScale, 0.5f, 2f); // 限制缩放范围

        // 按键
        if (Input.GetKeyDown(_toggleKey.Value))
        {
            _visible = !_visible;
            if (_visible) 
            {
                _focusInputCounter = 2;
            }
        }

        if (_visible && Input.GetKeyDown(KeyCode.Escape))
            _visible = false;

        // Drain pending output
        int drained = 0;
        while (_pendingLines.TryDequeue(out var line))
        {
            _outputLines.Add(line);
            drained++;
        }

        // Trim output
        if (_outputLines.Count > _maxOutputLines.Value)
        {
            int remove = _outputLines.Count - _maxOutputLines.Value;
            _outputLines.RemoveRange(0, remove);
        }

        // If new output arrived and user was at bottom, scroll down
        if (drained > 0 && IsNearBottom())
            _shouldScrollToBottom = true;
    }

    private void OnGUI()
    {
        if (!_visible) return;

        // 应用UI缩放
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(_uiScale, _uiScale, 1f));

        EnsureStyles();

        GUI.depth = 0;
        
        // 调整窗口大小以适应缩放
        Rect scaledRect = new Rect(
            _windowRect.x / _uiScale,
            _windowRect.y / _uiScale,
            _windowRect.width / _uiScale,
            _windowRect.height / _uiScale
        );
        
        scaledRect = GUILayout.Window(GetInstanceID(), scaledRect, DrawWindow, $"HoLConsole v{VERSION}", _styleWindow);
        
        _windowRect = new Rect(
            scaledRect.x * _uiScale,
            scaledRect.y * _uiScale,
            scaledRect.width * _uiScale,
            scaledRect.height * _uiScale
        );

        GUI.matrix = oldMatrix;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void EnsureStyles()
    {
        if (_styleInfo != null!) return;

        // 创建统一的半透明背景纹理
        _bgTexture = MakeTex(2, 2, new Color(0.08f, 0.08f, 0.08f, 0.92f));
        _inputBgTexture = MakeTex(2, 2, new Color(0.12f, 0.12f, 0.12f, 0.95f));

        // 窗口样式 - 简化设计
        _styleWindow = new GUIStyle(GUI.skin.window)
        {
            normal = { background = _bgTexture, textColor = new Color(0.85f, 0.85f, 0.85f) },
            onNormal = { background = _bgTexture, textColor = new Color(0.85f, 0.85f, 0.85f) },
            fontSize = 14,
            padding = new RectOffset(8, 8, 24, 8),
            border = new RectOffset(2, 2, 2, 2)
        };

        // 输入框样式 - 去除默认边框
        _styleTextField = new GUIStyle(GUI.skin.textField)
        {
            normal = { textColor = new Color(0.9f, 0.9f, 0.9f), background = _inputBgTexture },
            hover = { textColor = new Color(0.9f, 0.9f, 0.9f), background = _inputBgTexture },
            focused = { textColor = Color.white, background = _inputBgTexture },
            fontSize = 15,
            padding = new RectOffset(4, 4, 4, 4),
            border = new RectOffset(0, 0, 0, 0)
        };

        // 提示符样式 - 与输入框对齐
        _stylePrompt = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.5f, 0.9f, 0.5f) },
            padding = new RectOffset(0, 4, 4, 4),
            alignment = TextAnchor.MiddleCenter
        };

        // 文本样式
        _styleInfo = new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            fontSize = 15,
            normal = { textColor = new Color(0.85f, 0.85f, 0.85f) },
            padding = new RectOffset(2, 2, 1, 1),
            margin = new RectOffset(0, 0, 0, 0)
        };

        _styleWarn = new GUIStyle(_styleInfo)
        {
            normal = { textColor = new Color(1f, 0.85f, 0.3f) }
        };

        _styleError = new GUIStyle(_styleInfo)
        {
            normal = { textColor = new Color(1f, 0.4f, 0.4f) }
        };
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.BeginVertical();
        
        DrawOutputArea();
        DrawInputArea();

        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void DrawOutputArea()
    {
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));
        
        foreach (var line in _outputLines)
        {
            GUILayout.Label(line.Text, StyleFor(line.Level));
        }
        
        GUILayout.EndScrollView();
        
        if (Event.current.type == EventType.Repaint)
            _lastScrollViewHeight = GUILayoutUtility.GetLastRect().height;

        if (_shouldScrollToBottom)
        {
            _scrollPos.y = float.MaxValue;
            _shouldScrollToBottom = false;
        }
        
        GUILayout.Space(4);
    }

    private void DrawInputArea()
    {
        HandleInputEvents();
        
        GUILayout.BeginHorizontal();
        
        // 添加提示符 - 使用专门的样式对齐
        GUILayout.Label(">", _stylePrompt, GUILayout.Width(20));
        
        GUI.SetNextControlName(INPUT_CONTROL_NAME);
        _input = GUILayout.TextField(_input, _styleTextField, GUILayout.ExpandWidth(true));
        
        GUILayout.EndHorizontal();
        
        // 处理焦点设置
        if (_focusInputCounter > 0)
        {
            if (Event.current.type == EventType.Layout)
            {
                GUI.FocusControl(INPUT_CONTROL_NAME);
            }
            else if (Event.current.type == EventType.Repaint)
            {
                _focusInputCounter--;
                
                // 移动光标到末尾
                TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                if (te != null)
                {
                    te.text = _input ?? "";
                    int end = te.text.Length;
                    te.cursorIndex = end;
                    te.selectIndex = end;
                }
            }
        }
    }

    private GUIStyle StyleFor(ConsoleLevel lvl) =>
        lvl switch
        {
            ConsoleLevel.Warn => _styleWarn,
            ConsoleLevel.Error => _styleError,
            _ => _styleInfo
        };
    
    public void Enqueue(ConsoleLine line)
    {
        _pendingLines.Enqueue(line);
    }
    
    private void HandleInputEvents()
    {
        var e = Event.current;
        if (e == null) return;
        
        if(GUI.GetNameOfFocusedControl() != INPUT_CONTROL_NAME) return;
        
        if (e.type != EventType.KeyDown) return;

        // Enter: execute
        if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
        {
            e.Use();
            var line = _input.Trim();
            if (line.Length > 0)
            {
                PushInputHistory(line);
                ConsoleCore.Execute(line);

                if (IsNearBottom())
                    _shouldScrollToBottom = true;
            }
            _input = "";
            ResetHistoryNav();
            ResetAutocomplete();
            _focusInputCounter = 2;
            return;
        }

        // Up: history prev
        if (e.keyCode == KeyCode.UpArrow)
        {
            e.Use();
            if (_inputHistory.Count == 0) return;

            if (_historyIndex < 0)
            {
                _historyStash = _input;
                _historyIndex = _inputHistory.Count - 1;
            }
            else
            {
                _historyIndex = Mathf.Clamp(_historyIndex - 1, 0, _inputHistory.Count - 1);
            }

            _input = _inputHistory[_historyIndex];
            ResetAutocomplete();
            _focusInputCounter = 2;
            return;
        }

        // Down: history next / restore stash
        if (e.keyCode == KeyCode.DownArrow)
        {
            e.Use();
            if (_inputHistory.Count == 0) return;
            if (_historyIndex < 0) return;

            _historyIndex++;
            if (_historyIndex >= _inputHistory.Count)
            {
                _historyIndex = -1;
                _input = _historyStash;
            }
            else
            {
                _input = _inputHistory[_historyIndex];
            }

            ResetAutocomplete();
            _focusInputCounter = 2;
            return;
        }

        // Tab: autocomplete
        if (e.keyCode == KeyCode.Tab)
        {
            e.Use();
            bool reverse = e.shift;
            ApplyAutocomplete(reverse);
            _focusInputCounter = 2;
            return;
        }

        // Esc: close
        if (e.keyCode == KeyCode.Escape)
        {
            e.Use();
            _visible = false;
            return;
        }
        
        if (e.keyCode != KeyCode.LeftArrow && e.keyCode != KeyCode.RightArrow)
        {
            ResetAutocomplete();
        }
    }

    private void PushInputHistory(string line)
    {
        if (_inputHistory.Count == 0 || _inputHistory.Last() != line)
            _inputHistory.Add(line);

        while (_inputHistory.Count > _maxInputHistory.Value)
            _inputHistory.RemoveAt(0);
    }

    private void ResetHistoryNav()
    {
        _historyIndex = -1;
        _historyStash = "";
    }
    
    private bool IsNearBottom()
    {
        if (_scrollPos.y >= 1e7f) return true;
        if (_lastScrollViewHeight <= 0f) return true;
        return _scrollPos.y > 100000f;
    }
    
    private void ResetAutocomplete()
    {
        _candidates = Array.Empty<string>();
        _candidateIndex = -1;
        _sessionKey = "";
    }

    private void ApplyAutocomplete(bool reverse)
    {
        string current = _input;
        var split = CommandLineParser.SplitForCompletion(current);

        // Determine what token is being completed
        bool completingNewToken = split.HasTrailingSpace;
        int tokenIndex = completingNewToken ? split.Tokens.Count : Math.Max(0, split.Tokens.Count - 1);

        string cmdName = split.Tokens.Count > 0 ? split.Tokens[0] : "";
        bool completingCommandName = (split.Tokens.Count == 0) || (tokenIndex == 0);

        string seed = "";
        if (!completingNewToken && split.Tokens.Count > 0)
            seed = split.LastToken;

        // Build session key: changes when input meaningfully changes
        string newKey = $"{cmdName}|{tokenIndex}|{seed}|{(completingNewToken ? 1 : 0)}|{(completingCommandName ? 1 : 0)}";
        if (_sessionKey != newKey)
        {
            _sessionKey = newKey;
            _candidateIndex = reverse ? int.MaxValue : -1;
            _candidates = BuildCandidates(current, split, cmdName, tokenIndex, completingCommandName, seed).ToArray();
        }

        if (_candidates.Length == 0) return;

        // cycle
        if (!reverse)
            _candidateIndex = (_candidateIndex + 1) % _candidates.Length;
        else
            _candidateIndex = (_candidateIndex - 1 + _candidates.Length) % _candidates.Length;

        var picked = _candidates[_candidateIndex];
        ApplyCandidateToInput(current, split, tokenIndex, completingNewToken, picked);
    }

    private IEnumerable<string> BuildCandidates(string fullInput, CommandLineParser.CompletionSplit split, string cmdName, int tokenIndex, bool completingCommandName, string seed)
    {
        if (completingCommandName)
        {
            var all = ConsoleAPI.ListCommandNames();
            return all.Where(n => n.StartsWith(seed, StringComparison.OrdinalIgnoreCase))
                      .OrderBy(n => n);
        }

        var cmd = ConsoleAPI.TryGetCommand(cmdName);
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (cmd is ICommandCompleter completer)
        {
            var ctx = new CompletionContext
            {
                CommandName = cmdName,
                FullInput = fullInput,
                Tokens = split.Tokens.ToList(),
                Seed = seed,
                TokenIndex = tokenIndex,
                CompletingCommandName = false
            };

            return completer.Complete(ctx)
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(s => s);
        }

        return Array.Empty<string>();
    }

    private void ApplyCandidateToInput(string current, CommandLineParser.CompletionSplit split, int tokenIndex, bool completingNewToken, string picked)
    {
        if (completingNewToken)
        {
            _input = current + picked + " ";
            return;
        }
        
        if (split.Tokens.Count == 0)
        {
            _input = picked + " ";
            return;
        }
        
        var last = split.LastToken;
        if (last.Length == 0)
        {
            _input = current + picked;
            return;
        }

        var prefix = current.Substring(0, current.Length - last.Length);
        _input = prefix + picked;
        
        if (tokenIndex == 0)
        {
            _input += " ";
        }
    }
    
    private void RegisterBuiltinCommands()
    {
        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "help",
            description: "List commands or show help for a command.",
            usage: "help [command]",
            handler: (_, args) =>
            {
                if (args.Positionals.Count == 0)
                {
                    var names = ConsoleAPI.ListCommandNames().OrderBy(x => x).ToArray();
                    var sb = new StringBuilder();
                    sb.AppendLine("Commands:");
                    foreach (var n in names)
                    {
                        var c = ConsoleAPI.TryGetCommand(n);
                        if (c is { Hidden: false })
                            sb.AppendLine($"  {n} - {c.Description}");
                    }
                    sb.AppendLine("Type: help <command> for details.");
                    return sb.ToString().TrimEnd();
                }
                else
                {
                    var n = args.Positionals[0];
                    var c = ConsoleAPI.TryGetCommand(n);
                    if (c == null) return $"Unknown command: {n}";
                    return $"{c.Name}\n  {c.Description}\nUsage: {c.Usage}";
                }
            }
        ));

        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "echo",
            description: "Print text.",
            usage: "echo <text...>",
            handler: (_, args) => string.Join(" ", args.Positionals)
        ));

        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "clear",
            description: "Clear console output.",
            usage: "clear",
            handler: (_, _) =>
            {
                _outputLines.Clear();
                _shouldScrollToBottom = true;
                return "";
            }
        ));

        ConsoleAPI.RegisterAlias("cls", "clear");
        
        PropCommands.Register();
        DebugCommands.Register();
    }
}
