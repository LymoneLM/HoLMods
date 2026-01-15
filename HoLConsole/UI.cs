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

namespace HoLConsole;

[BepInPlugin(MODGUID, MODNAME, VERSION)]
public class HoLConsolePlugin : BaseUnityPlugin, IConsoleHost
{
    public const string MODNAME = "HoLConsole";
    public const string MODGUID = "cc.lymone.HoL." + MODNAME;
    public const string VERSION = "1.0.0";

    internal static HoLConsolePlugin Instance = null!;
    internal static ManualLogSource Log = null!;

    private ConfigEntry<KeyCode> _toggleKey = null!;
    private ConfigEntry<int> _maxOutputLines = null!;
    private ConfigEntry<int> _maxInputHistory = null!;
    private ConfigEntry<bool> _startOpen = null!;

    private bool _visible;
    private Rect _windowRect = new Rect(40, 40, 760, 420);

    private Vector2 _scrollPos;
    private bool _shouldScrollToBottom;
    private float _lastScrollViewHeight;

    private string _input = "";
    private bool _focusInputNextFrame;

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

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        _toggleKey = Config.Bind("General", "ToggleKey", KeyCode.BackQuote, "Key to toggle the console window.");
        _maxOutputLines = Config.Bind("General", "MaxOutputLines", 600, "Maximum output lines kept in memory.");
        _maxInputHistory = Config.Bind("General", "MaxInputHistory", 200, "Maximum input history items kept in memory.");
        _startOpen = Config.Bind("General", "StartOpen", false, "Start with console visible.");

        _visible = _startOpen.Value;

        HoLConsoleAPI.Initialize(this);
        RegisterBuiltinCommands();

        HoLConsoleAPI.Print("HoLConsole ready. Press ToggleKey to open.", ConsoleLevel.Info);
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey.Value))
        {
            _visible = !_visible;
            if (_visible) _focusInputNextFrame = true;
        }

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

        EnsureStyles();

        GUI.depth = 0;
        _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, DrawWindow, "HoLConsole");
    }

    private void EnsureStyles()
    {
        if (_styleInfo != null!) return;

        _styleInfo = new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        _styleWarn = new GUIStyle(_styleInfo);
        _styleWarn.normal.textColor = new Color(1f, 0.8f, 0.2f);

        _styleError = new GUIStyle(_styleInfo);
        _styleError.normal.textColor = new Color(1f, 0.35f, 0.35f);
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.BeginVertical();

        DrawToolbar();
        DrawOutputArea();
        DrawInputArea();

        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear", GUILayout.Width(70)))
        {
            _outputLines.Clear();
            _shouldScrollToBottom = true;
        }

        if (GUILayout.Button("Help", GUILayout.Width(70)))
        {
            ExecuteLine("help");
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label($"Lines: {_outputLines.Count}/{_maxOutputLines.Value}", GUILayout.Width(180));

        GUILayout.EndHorizontal();
    }

    private void DrawOutputArea()
    {
        // We need scrollview height for bottom detection
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));
        foreach (var line in _outputLines)
        {
            GUILayout.Label(line.Text, StyleFor(line.Level));
        }
        GUILayout.EndScrollView();

        // Track visible height (approx)
        if (Event.current.type == EventType.Repaint)
            _lastScrollViewHeight = GUILayoutUtility.GetLastRect().height;

        if (_shouldScrollToBottom)
        {
            _scrollPos.y = float.MaxValue;
            _shouldScrollToBottom = false;
        }

        GUILayout.Space(6);
    }

    private void DrawInputArea()
    {
        GUI.SetNextControlName(INPUT_CONTROL_NAME);
        _input = GUILayout.TextField(_input ?? "", GUILayout.ExpandWidth(true));

        HandleInputEvents();

        if (_focusInputNextFrame)
        {
            _focusInputNextFrame = false;
            GUI.FocusControl(INPUT_CONTROL_NAME);
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
        if (e.type != EventType.KeyDown) return;

        // Enter: execute
        if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
        {
            e.Use();
            var line = (_input ?? "").Trim();
            if (line.Length > 0)
            {
                PushInputHistory(line);
                ExecuteLine(line);
            }
            _input = "";
            ResetHistoryNav();
            ResetAutocomplete();
            _focusInputNextFrame = true;
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
            _focusInputNextFrame = true;
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
            _focusInputNextFrame = true;
            return;
        }

        // Tab: autocomplete
        if (e.keyCode == KeyCode.Tab)
        {
            e.Use();
            bool reverse = e.shift;
            ApplyAutocomplete(reverse);
            _focusInputNextFrame = true;
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

    private void ExecuteLine(string line)
    {
        HoLConsoleAPI.Print($"> {line}", ConsoleLevel.Info);

        try
        {
            var result = HoLConsoleAPI.Execute(line, Log);
            if (!string.IsNullOrWhiteSpace(result))
                HoLConsoleAPI.Print(result, ConsoleLevel.Info);
        }
        catch (Exception ex)
        {
            HoLConsoleAPI.Print($"Exception: {ex.Message}", ConsoleLevel.Error);
            HoLConsoleAPI.Print(ex.StackTrace ?? "(no stack)", ConsoleLevel.Error);
            Log.LogError(ex);
        }

        // only scroll if user already near bottom
        if (IsNearBottom())
            _shouldScrollToBottom = true;
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
        string current = _input ?? "";
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
            var all = HoLConsoleAPI.ListCommandNames();
            return all.Where(n => n.StartsWith(seed, StringComparison.OrdinalIgnoreCase))
                      .OrderBy(n => n);
        }

        var cmd = HoLConsoleAPI.TryGetCommand(cmdName);
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
        HoLConsoleAPI.RegisterCommand(new CommandDef(
            name: "help",
            description: "List commands or show help for a command.",
            usage: "help [command]",
            handler: (ctx, args) =>
            {
                if (args.Positionals.Count == 0)
                {
                    var names = HoLConsoleAPI.ListCommandNames().OrderBy(x => x).ToArray();
                    var sb = new StringBuilder();
                    sb.AppendLine("Commands:");
                    foreach (var n in names)
                    {
                        var c = HoLConsoleAPI.TryGetCommand(n);
                        if (c != null && !c.Hidden)
                            sb.AppendLine($"  {n} - {c.Description}");
                    }
                    sb.AppendLine("Type: help <command> for details.");
                    return sb.ToString().TrimEnd();
                }
                else
                {
                    var n = args.Positionals[0];
                    var c = HoLConsoleAPI.TryGetCommand(n);
                    if (c == null) return $"Unknown command: {n}";
                    return $"{c.Name}\n  {c.Description}\nUsage: {c.Usage}";
                }
            }
        ));

        HoLConsoleAPI.RegisterCommand(new CommandDef(
            name: "echo",
            description: "Print text.",
            usage: "echo <text...>",
            handler: (ctx, args) => string.Join(" ", args.Positionals)
        ));

        HoLConsoleAPI.RegisterCommand(new CommandDef(
            name: "clear",
            description: "Clear console output.",
            usage: "clear",
            handler: (ctx, args) =>
            {
                _outputLines.Clear();
                _shouldScrollToBottom = true;
                return "";
            }
        ));

        HoLConsoleAPI.RegisterAlias("cls", "clear");
    }
}

