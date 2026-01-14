#nullable enable
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HoLConsole;

// =========================
// Host bridge (thread-safe enqueue by UI side)
// =========================
public interface IConsoleHost
{
    void Enqueue(ConsoleLine line);
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
    Func<CommandContext, ParsedArgs, string?> Handler { get; }
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
    public Func<CommandContext, ParsedArgs, string?> Handler { get; }

    public CommandDef(string name, string description, string usage, Func<CommandContext, ParsedArgs, string?> handler, bool hidden = false)
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
    public List<string> Tokens = new();
    public string Seed = "";
    public int TokenIndex; // index in Tokens that is being completed
    public bool CompletingCommandName; // true if TokenIndex == 0
}

public sealed class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);

    public void Register(ICommand cmd) => _commands[cmd.Name] = cmd;

    public bool Unregister(string name)
    {
        _aliases.Remove(name);
        return _commands.Remove(name);
    }

    public void RegisterAlias(string alias, string target) => _aliases[alias] = target;

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

    public IEnumerable<string> ListNames() => _commands.Keys;
}

// =========================
// Args parsing (quotes already handled by tokenizer)
// =========================
public sealed class ParsedArgs
{
    public readonly List<string> Positionals = new();
    public readonly Dictionary<string, string?> Flags = new(StringComparer.OrdinalIgnoreCase);

    public bool HasFlag(string key) => Flags.ContainsKey(key);

    public string? GetFlag(string key, string? defaultValue = null)
        => Flags.TryGetValue(key, out var v) ? v : defaultValue;

    public int GetInt(string key, int defaultValue = 0)
        => int.TryParse(GetFlag(key), out var i) ? i : defaultValue;

    public float GetFloat(string key, float defaultValue = 0f)
        => float.TryParse(GetFlag(key), out var f) ? f : defaultValue;

    private static bool LooksLikeShortFlag(string token)
    {
        // -x, -abc are flags; but -10 / -3.14 are numbers -> NOT flags
        if (token.Length < 2 || token[0] != '-') return false;
        if (token == "-") return false;
        if (char.IsDigit(token[1])) return false; // negative number
        return true;
    }

    private static bool LooksLikeLongFlag(string token)
    {
        // --key is flag; --10 is weird but treat as not-flag to avoid eating negative number style inputs
        if (!token.StartsWith("--", StringComparison.Ordinal) || token.Length <= 2) return false;
        if (char.IsDigit(token[2])) return false;
        return true;
    }

    public static ParsedArgs FromTokens(IReadOnlyList<string> tokens)
    {
        var a = new ParsedArgs();

        for (int i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];

            // --key=value or -k=value
            if ((LooksLikeLongFlag(t) || LooksLikeShortFlag(t)) && t.Contains("="))
            {
                var idx = t.IndexOf('=');
                var k = t.Substring(0, idx).TrimStart('-');
                var v = t.Substring(idx + 1);
                a.Flags[k] = v;
                continue;
            }

            // --key value / -k value / --flag
            if (LooksLikeLongFlag(t))
            {
                var k = t.Substring(2);
                string? v = null;

                // next token can be a negative number; allow it as a value
                if (i + 1 < tokens.Count && !(LooksLikeLongFlag(tokens[i + 1]) || LooksLikeShortFlag(tokens[i + 1])))
                {
                    v = tokens[i + 1];
                    i++;
                }

                a.Flags[k] = v;
                continue;
            }

            if (LooksLikeShortFlag(t))
            {
                var k = t.Substring(1);
                string? v = null;

                if (i + 1 < tokens.Count && !(LooksLikeLongFlag(tokens[i + 1]) || LooksLikeShortFlag(tokens[i + 1])))
                {
                    v = tokens[i + 1];
                    i++;
                }

                a.Flags[k] = v;
                continue;
            }

            a.Positionals.Add(t);
        }

        return a;
    }
}

// =========================
// Tokenizer + completion split helper
// =========================
public static class CommandLineParser
{
    public sealed class ParseResult
    {
        public readonly List<string> Tokens = new();
    }

    public sealed class CompletionSplit
    {
        public readonly List<string> Tokens = new();
        public bool HasTrailingSpace;
        public int TokenCount => Tokens.Count;
        public string LastToken => Tokens.Count == 0 ? "" : Tokens.Last();
    }

    public static ParseResult Parse(string input)
    {
        var r = new ParseResult();
        foreach (var t in Tokenize(input))
            r.Tokens.Add(t);
        return r;
    }

    public static CompletionSplit SplitForCompletion(string input)
    {
        var s = new CompletionSplit();
        s.HasTrailingSpace = input.Length > 0 && char.IsWhiteSpace(input.Last());

        foreach (var t in Tokenize(input))
            s.Tokens.Add(t);

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
