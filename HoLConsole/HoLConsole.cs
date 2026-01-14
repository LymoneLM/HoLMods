#nullable enable
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HoLConsole;

[BepInPlugin(MODGUID, MODNAME, VERSION)]
public class HoLConsole : BaseUnityPlugin
{
    public const string MODNAME = "HoLConsole";
    public const string MODGUID = "cc.lymone.HoL." + MODNAME;
    public const string VERSION = "1.0.0";
    
    internal static HoLConsole Instance = null!;
    internal static ManualLogSource Log = null!;

    private ConfigEntry<KeyCode> _toggleKey = null!;
    private ConfigEntry<int> _maxOutputLines = null!;
    private ConfigEntry<int> _maxInputHistory = null!;
    private ConfigEntry<bool> _startOpen = null!;

    private bool _visible;
    private Rect _windowRect = new Rect(40, 40, 760, 420);
    private Vector2 _scrollPos;

    private string _input = "";
    private bool _focusInputNextFrame;
    private int _historyIndex = -1;

    // 输出行
    private readonly List<ConsoleLine> _outputLines = new List<ConsoleLine>();

    // 输入历史
    private readonly List<string> _inputHistory = new List<string>();

    // 输出缓冲区
    private readonly Queue<ConsoleLine> _pendingLines = new Queue<ConsoleLine>();

    private string[] _autoCompleteCandidates = Array.Empty<string>();
    private int _autoCompleteIndex = -1;
    private string _autoCompleteSeed = "";

    private const string INPUT_CONTROL_NAME = "InGameConsole_Input";

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        _toggleKey = Config.Bind("General", "ToggleKey", KeyCode.BackQuote, "Key to toggle the console window.");
        _maxOutputLines = Config.Bind("General", "MaxOutputLines", 600, "Maximum output lines kept in memory.");
        _maxInputHistory = Config.Bind("General", "MaxInputHistory", 200, "Maximum input history items kept in memory.");
        _startOpen = Config.Bind("General", "StartOpen", false, "Start with console visible.");

        _visible = _startOpen.Value;

        // 初始化核心
        HoLConsoleAPI.Initialize(new ConsoleHost(this));

        // 内置命令
        RegisterBuiltinCommands();

        HoLConsoleAPI.Print("InGameConsole ready. Press ToggleKey to open.", ConsoleLevel.Info);
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey.Value))
        {
            _visible = !_visible;
            if (_visible) _focusInputNextFrame = true;
        }

        // 缓冲区写入输出
        lock (_pendingLines)
        {
            while (_pendingLines.Count > 0)
            {
                _outputLines.Add(_pendingLines.Dequeue());
            }
        }

        // 修剪输出
        while (_outputLines.Count > _maxOutputLines.Value)
            _outputLines.RemoveAt(0);
    }

    private void OnGUI()
    {
        if (!_visible) return;

        // 简单皮肤：你也可以换成自定义 GUIStyle
        GUI.depth = 0;
        _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, DrawWindow, "Console");
    }

    private void DrawWindow(int windowId)
    {
        GUILayout.BeginVertical();

        DrawToolbar();

        // 输出区
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));
        foreach (var line in _outputLines)
        {
            var style = GetStyleForLevel(line.Level);
            GUILayout.Label(line.Text, style);
        }
        GUILayout.EndScrollView();

        GUILayout.Space(6);

        // 输入区
        GUI.SetNextControlName(INPUT_CONTROL_NAME);
        _input = GUILayout.TextField(_input ?? "", GUILayout.ExpandWidth(true));

        HandleInputEvents();

        if (_focusInputNextFrame)
        {
            _focusInputNextFrame = false;
            GUI.FocusControl(INPUT_CONTROL_NAME);
        }

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear", GUILayout.Width(70)))
        {
            _outputLines.Clear();
            _scrollPos.y = 999999;
        }

        if (GUILayout.Button("Help", GUILayout.Width(70)))
        {
            ExecuteLine("help");
        }

        GUILayout.FlexibleSpace();

        GUILayout.Label($"Lines: {_outputLines.Count}/{_maxOutputLines.Value}", GUILayout.Width(160));

        GUILayout.EndHorizontal();
    }

    private void HandleInputEvents()
    {
        var e = Event.current;
        if (e == null) return;

        // 只处理键盘事件
        if (e.type != EventType.KeyDown) return;

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
            ResetAutocomplete();
            _historyIndex = -1;
            _focusInputNextFrame = true;
            return;
        }

        // Up/Down 浏览历史
        if (e.keyCode == KeyCode.UpArrow)
        {
            e.Use();
            if (_inputHistory.Count == 0) return;
            if (_historyIndex < 0) _historyIndex = _inputHistory.Count - 1;
            else _historyIndex = Mathf.Clamp(_historyIndex - 1, 0, _inputHistory.Count - 1);
            _input = _inputHistory[_historyIndex];
            MoveCursorToEnd();
            ResetAutocomplete();
            return;
        }

        if (e.keyCode == KeyCode.DownArrow)
        {
            e.Use();
            if (_inputHistory.Count == 0) return;
            if (_historyIndex < 0) return;

            _historyIndex++;
            if (_historyIndex >= _inputHistory.Count)
            {
                _historyIndex = -1;
                _input = "";
            }
            else
            {
                _input = _inputHistory[_historyIndex];
            }
            MoveCursorToEnd();
            ResetAutocomplete();
            return;
        }

        // Tab 自动补全
        if (e.keyCode == KeyCode.Tab)
        {
            e.Use();
            bool reverse = e.shift;
            ApplyAutocomplete(reverse);
            MoveCursorToEnd();
            return;
        }

        // Esc 关闭
        if (e.keyCode == KeyCode.Escape)
        {
            e.Use();
            _visible = false;
            return;
        }
    }

    private void ExecuteLine(string line)
    {
        HoLConsoleAPI.Print($"> {line}", ConsoleLevel.Info);

        try
        {
            var result = HoLConsoleAPI.Execute(line);
            if (!string.IsNullOrWhiteSpace(result))
                HoLConsoleAPI.Print(result, ConsoleLevel.Info);
        }
        catch (Exception ex)
        {
            HoLConsoleAPI.Print($"Exception: {ex.Message}", ConsoleLevel.Error);
            HoLConsoleAPI.Print(ex.StackTrace ?? "(no stack)", ConsoleLevel.Error);
            Log.LogError(ex);
        }

        // 自动滚到底
        _scrollPos.y = 999999;
    }

    private void PushInputHistory(string line)
    {
        // 避免连续重复
        if (_inputHistory.Count == 0 || _inputHistory[_inputHistory.Count - 1] != line)
            _inputHistory.Add(line);

        // Trim
        while (_inputHistory.Count > _maxInputHistory.Value)
            _inputHistory.RemoveAt(0);
    }

    internal void EnqueueLine(ConsoleLine line)
    {
        lock (_pendingLines)
        {
            _pendingLines.Enqueue(line);
        }
    }

    private GUIStyle GetStyleForLevel(ConsoleLevel lvl)
    {
        // 你可以更精细：不同颜色、字体
        // IMGUI 颜色用 GUI.contentColor 也行，但会影响后续控件；这里用 style.normal.textColor 做简单区分
        var style = new GUIStyle(GUI.skin.label);
        switch (lvl)
        {
            case ConsoleLevel.Warn:
                style.normal.textColor = new Color(1f, 0.8f, 0.2f);
                break;
            case ConsoleLevel.Error:
                style.normal.textColor = new Color(1f, 0.35f, 0.35f);
                break;
            default:
                style.normal.textColor = Color.white;
                break;
        }
        style.wordWrap = true;
        return style;
    }

    private void MoveCursorToEnd()
    {
        // TextField 光标控制在 IMGUI 里没那么直观，这里用一个常见 trick：
        // 让下一帧继续 focus，并依赖 Unity 自动把光标放最后（多数版本可行）
        _focusInputNextFrame = true;
    }

    private void ResetAutocomplete()
    {
        _autoCompleteCandidates = Array.Empty<string>();
        _autoCompleteIndex = -1;
        _autoCompleteSeed = "";
    }

    private void ApplyAutocomplete(bool reverse)
    {
        string current = _input ?? "";
        var parse = CommandLineParser.SplitForCompletion(current);

        // 若光标不在末尾、或多行编辑，你可能要做更复杂的 token-range 替换；
        // 这里先假设始终在末尾补全（足够实用）。

        if (parse.TokenCount == 0)
        {
            // 空输入：列出所有命令循环补全
            EnsureCandidates("", onlyCommands: true);
            CycleCandidate(reverse, out var picked);
            if (picked != null) _input = picked + " ";
            return;
        }

        if (parse.TokenCount == 1 && !parse.HasTrailingSpace)
        {
            // 补全命令名
            var seed = parse.LastToken;
            EnsureCandidates(seed, onlyCommands: true);
            CycleCandidate(reverse, out var picked);
            if (picked != null) _input = picked;
            return;
        }

        // 补全参数：让命令提供 completer（可选）
        var cmdName = parse.Tokens[0];
        var cmd = HoLConsoleAPI.TryGetCommand(cmdName);
        if (cmd is ICommandCompleter completer)
        {
            var seed = parse.HasTrailingSpace ? "" : parse.LastToken;
            var suggestions = completer.Complete(new CompletionContext
            {
                CommandName = cmdName,
                FullInput = current,
                Tokens = parse.Tokens,
                Seed = seed
            }).Distinct().ToArray();

            if (suggestions.Length == 0) return;

            // cycle suggestions
            if (_autoCompleteSeed != seed || !_autoCompleteCandidates.SequenceEqual(suggestions))
            {
                _autoCompleteSeed = seed;
                _autoCompleteCandidates = suggestions;
                _autoCompleteIndex = reverse ? suggestions.Length : -1;
            }

            CycleCandidate(reverse, out var picked2);
            if (picked2 == null) return;

            // 替换最后一个 token 或追加
            if (parse.HasTrailingSpace)
            {
                _input = current + picked2 + " ";
            }
            else
            {
                // replace last token
                var prefix = current.Substring(0, current.Length - parse.LastToken.Length);
                _input = prefix + picked2;
            }
        }
    }

    private void EnsureCandidates(string seed, bool onlyCommands)
    {
        if (_autoCompleteSeed == seed && _autoCompleteCandidates.Length > 0) return;

        _autoCompleteSeed = seed;
        var cmds = HoLConsoleAPI.ListCommandNames();

        var matches = cmds
            .Where(n => n.StartsWith(seed, StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n)
            .ToArray();

        _autoCompleteCandidates = matches;
        _autoCompleteIndex = -1;
    }

    private void CycleCandidate(bool reverse, out string? picked)
    {
        picked = null;
        if (_autoCompleteCandidates.Length == 0) return;

        if (!reverse)
            _autoCompleteIndex = (_autoCompleteIndex + 1) % _autoCompleteCandidates.Length;
        else
            _autoCompleteIndex = (_autoCompleteIndex - 1 + _autoCompleteCandidates.Length) % _autoCompleteCandidates.Length;

        picked = _autoCompleteCandidates[_autoCompleteIndex];
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
                return "";
            }
        ));

        HoLConsoleAPI.RegisterAlias("cls", "clear");
    }
}

// =========================
// Public API (Other mods use this)
// =========================
public static class HoLConsoleAPI
{
    private static ConsoleHost? _host;
    private static readonly CommandRegistry _registry = new CommandRegistry();

    public static void Initialize(ConsoleHost host) => _host = host;

    public static void Print(string text, ConsoleLevel level = ConsoleLevel.Info)
    {
        if (_host == null) return;
        foreach (var line in SplitLines(text))
            _host.Enqueue(new ConsoleLine(level, line));
    }

    public static string Execute(string line)
    {
        var parsed = CommandLineParser.Parse(line);
        if (parsed.Tokens.Count == 0) return "";

        var cmdName = parsed.Tokens[0];
        if (_registry.TryResolve(cmdName, out var cmd) == false || cmd == null)
            return $"Unknown command: {cmdName} (type: help)";

        var ctx = new CommandContext
        {
            Logger = HoLConsole.Log,
            Print = Print
        };

        // build args (excluding command token)
        var args = ParsedArgs.FromTokens(parsed.Tokens.Skip(1).ToList());
        return cmd.Handler(ctx, args) ?? "";
    }

    public static void RegisterCommand(ICommand command) => _registry.Register(command);
    public static bool UnregisterCommand(string name) => _registry.Unregister(name);

    public static void RegisterAlias(string alias, string targetCommand) => _registry.RegisterAlias(alias, targetCommand);

    public static ICommand? TryGetCommand(string name) => _registry.TryGet(name);

    public static IEnumerable<string> ListCommandNames() => _registry.ListNames();

    private static IEnumerable<string> SplitLines(string text)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        var arr = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        foreach (var s in arr) yield return s;
    }
}

// =========================
// Host wrapper (main thread safe enqueue)
// =========================
public sealed class ConsoleHost
{
    private readonly HoLConsole _plugin;
    public ConsoleHost(HoLConsole plugin) => _plugin = plugin;
    public void Enqueue(ConsoleLine line) => _plugin.EnqueueLine(line);
}

public enum ConsoleLevel { Info, Warn, Error }

public readonly struct ConsoleLine
{
    public readonly ConsoleLevel Level;
    public readonly string Text;
    public ConsoleLine(ConsoleLevel level, string text)
    {
        Level = level;
        Text = text;
    }
}

// =========================
// Command system
// =========================
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    string Usage { get; }
    bool Hidden { get; }
    Func<CommandContext, ParsedArgs, string> Handler { get; }
}

public interface ICommandCompleter
{
    IEnumerable<string> Complete(CompletionContext ctx);
}

public sealed class CommandDef : ICommand
{
    public string Name { get; }
    public string Description { get; }
    public string Usage { get; }
    public bool Hidden { get; }
    public Func<CommandContext, ParsedArgs, string> Handler { get; }

    public CommandDef(string name, string description, string usage, Func<CommandContext, ParsedArgs, string> handler, bool hidden = false)
    {
        Name = name;
        Description = description;
        Usage = usage;
        Handler = handler;
        Hidden = hidden;
    }
}

public sealed class CommandContext
{
    public ManualLogSource Logger = null!;
    public Action<string, ConsoleLevel> Print = null!;
}

public sealed class CompletionContext
{
    public string CommandName = "";
    public string FullInput = "";
    public List<string> Tokens = new List<string>();
    public string Seed = "";
}

public sealed class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public void Register(ICommand cmd)
    {
        _commands[cmd.Name] = cmd;
    }

    public bool Unregister(string name)
    {
        _aliases.Remove(name);
        return _commands.Remove(name);
    }

    public void RegisterAlias(string alias, string target)
    {
        _aliases[alias] = target;
    }

    public bool TryResolve(string name, out ICommand? cmd)
    {
        cmd = null;

        if (_aliases.TryGetValue(name, out var target))
            name = target;

        return _commands.TryGetValue(name, out cmd);
    }

    public ICommand? TryGet(string name)
    {
        TryResolve(name, out var cmd);
        return cmd;
    }

    public IEnumerable<string> ListNames()
    {
        // 只列真实命令名（不含 alias）
        return _commands.Keys;
    }
}

// =========================
// Args parsing (supports quotes, flags)
// =========================
public sealed class ParsedArgs
{
    public readonly List<string> Positionals = new List<string>();
    public readonly Dictionary<string, string?> Flags = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public bool HasFlag(string key) => Flags.ContainsKey(key);

    public string? GetFlag(string key, string? defaultValue = null)
        => Flags.TryGetValue(key, out var v) ? v : defaultValue;

    public int GetInt(string key, int defaultValue = 0)
        => int.TryParse(GetFlag(key), out var i) ? i : defaultValue;

    public float GetFloat(string key, float defaultValue = 0f)
        => float.TryParse(GetFlag(key), out var f) ? f : defaultValue;

    public static ParsedArgs FromTokens(List<string> tokens)
    {
        var a = new ParsedArgs();

        for (int i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];

            // --key=value or -k=value
            if ((t.StartsWith("--") || t.StartsWith("-")) && t.Contains("="))
            {
                var idx = t.IndexOf('=');
                var k = t.Substring(0, idx).TrimStart('-');
                var v = t.Substring(idx + 1);
                a.Flags[k] = v;
                continue;
            }

            // --key value / -k value / --flag (bool)
            if (t.StartsWith("--"))
            {
                var k = t.Substring(2);
                string? v = null;
                if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("-"))
                {
                    v = tokens[i + 1];
                    i++;
                }
                a.Flags[k] = v;
                continue;
            }
            if (t.StartsWith("-") && t.Length > 1)
            {
                var k = t.Substring(1);
                string? v = null;
                if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("-"))
                {
                    v = tokens[i + 1];
                    i++;
                }
                a.Flags[k] = v;
                continue;
            }

            // positional
            a.Positionals.Add(t);
        }

        return a;
    }
}

// =========================
// Tokenizer (quotes, escapes) + completion helper
// =========================
public static class CommandLineParser
{
    public sealed class ParseResult
    {
        public readonly List<string> Tokens = new List<string>();
    }

    public sealed class CompletionSplit
    {
        public readonly List<string> Tokens = new List<string>();
        public bool HasTrailingSpace;
        public int TokenCount => Tokens.Count;
        public string LastToken => Tokens.Count == 0 ? "" : Tokens[Tokens.Count - 1];
    }

    public static ParseResult Parse(string input)
    {
        var r = new ParseResult();
        foreach (var t in Tokenize(input))
            r.Tokens.Add(t);
        return r;
    }

    // 用于补全：保留“末尾是否有空格”，便于判断是在补全下一个 token 还是替换当前 token
    public static CompletionSplit SplitForCompletion(string input)
    {
        var s = new CompletionSplit();
        s.HasTrailingSpace = input.Length > 0 && char.IsWhiteSpace(input[input.Length - 1]);

        foreach (var t in Tokenize(input))
            s.Tokens.Add(t);

        // 若末尾有空格，则认为“最后 token 已完成”，Seed 为空
        return s;
    }

    private static IEnumerable<string> Tokenize(string input)
    {
        if (string.IsNullOrEmpty(input))
            yield break;

        var sb = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '"';

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (!inQuotes && char.IsWhiteSpace(c))
            {
                if (sb.Length > 0)
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
                continue;
            }

            if (c == '\\' && i + 1 < input.Length)
            {
                // 简单转义：\" \\ \n \t
                char n = input[i + 1];
                if (n == '"' || n == '\'' || n == '\\')
                {
                    sb.Append(n);
                    i++;
                    continue;
                }
                if (n == 'n') { sb.Append('\n'); i++; continue; }
                if (n == 't') { sb.Append('\t'); i++; continue; }
            }

            if (!inQuotes && (c == '"' || c == '\''))
            {
                inQuotes = true;
                quoteChar = c;
                continue;
            }
            else if (inQuotes && c == quoteChar)
            {
                inQuotes = false;
                continue;
            }

            sb.Append(c);
        }

        if (sb.Length > 0)
            yield return sb.ToString();
    }
}
