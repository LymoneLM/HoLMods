#nullable enable
using System;
using System.Collections.Generic;

namespace HoLConsole;

public static class ConsoleAPI
{
    private static Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);
    private static Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);
    
    public static void RegisterCommand(ICommand cmd) => _commands[cmd.Name] = cmd;
    public static bool UnregisterCommand(string name) => _aliases.Remove(name) || _commands.Remove(name);

    public static void RegisterAlias(string alias, string target) => _aliases[alias] = target;
    
    public static bool TryResolveCommand(string name, out ICommand? cmd)
    {
        cmd = null;
        if (_aliases.TryGetValue(name, out var target))
            name = target;
        return _commands.TryGetValue(name, out cmd);
    }
    
    public static ICommand? TryGetCommand(string name)
    {
        TryResolveCommand(name, out var cmd);
        return cmd;
    }

    public static IEnumerable<string> ListCommandNames() => _commands.Keys;
}

