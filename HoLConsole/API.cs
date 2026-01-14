#nullable enable
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HoLConsole;

/// <summary>
/// Public API for other mods:
/// - Register commands
/// - Print lines
/// - Execute command lines
/// </summary>
public static class HoLConsoleAPI
{
    private static IConsoleHost? _host;
    private static readonly CommandRegistry _registry = new();

    public static bool IsInitialized => _host != null;

    public static void Initialize(IConsoleHost host) => _host = host;

    public static void Print(string text, ConsoleLevel level = ConsoleLevel.Info)
    {
        if (_host == null) return;
        foreach (var line in SplitLines(text))
            _host.Enqueue(new ConsoleLine(level, line));
    }

    public static string Execute(string line, ManualLogSource? logger = null)
    {
        var parsed = CommandLineParser.Parse(line);
        if (parsed.Tokens.Count == 0) return "";

        var cmdName = parsed.Tokens[0];
        if (_registry.TryResolve(cmdName, out var cmd) == false || cmd == null)
            return $"Unknown command: {cmdName} (type: help)";

        var ctx = new CommandContext
        {
            Logger = logger ?? BepInEx.Logging.Logger.CreateLogSource("HoLConsole"),
            Print = Print
        };

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

