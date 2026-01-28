using System;
using YuanAPI.Tools;

namespace HoLConsole;

internal static class PropCommands
{
    private const int MAX_GIVE_COUNT = 1000_0000;

    public static void Register()
    {
        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "give",
            description: "Give a prop (default: free, no storage cost)",
            usage: "give <PropID> [Count] [--free|--cost|--storage[=true|false]] [--silent|-s]",
            handler: GiveHandler
        ));
    }

    private static string GiveHandler(CommandContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 1)
            return "Usage: give <PropID> [Count] [--free|--cost|--storage[=true|false]] [--silent|-s]\nExample: give 12 5 --free";
        
        if (!int.TryParse(args.Positionals[0], out var id))
            return $"Invalid PropID: \"{args.Positionals[0]}\" (must be integer)";
        
        int count = 1;
        if (args.Positionals.Count >= 2)
        {
            if (!int.TryParse(args.Positionals[1], out count))
                return $"Invalid Count: \"{args.Positionals[1]}\" (must be integer)";
        }
        
        if (count <= 0)
            return "Count must be > 0";
        
        if (count > MAX_GIVE_COUNT)
            return $"Count too large (max {MAX_GIVE_COUNT})";
        
        bool storage = ResolveStorageFlag(args); // 默认 false（free）
        bool silence = args.HasFlag("silent") || args.HasFlag("silence") || args.HasFlag("s");
        
        bool ok;
        try
        {
            ok = PropTool.AddProp(id, count, storage: storage, silence: silence);
        }
        catch (Exception ex)
        {
            ctx.Logger.LogError(ex);
            return "Fatal error: exception thrown by PropTool.AddProp";
        }
        
        if (!ok)
        {
            if (storage)
                return $"Failed to add prop {id} x{count} (invalid PropID or insufficient storage)";
            return $"Failed to add prop {id} x{count} (invalid PropID)";
        }

        return storage
            ? $"OK: added prop {id} x{count} (storage consumed)"
            : $"OK: added prop {id} x{count}";
    }

    private static bool ResolveStorageFlag(ParsedArgs args)
    {
        bool storage = false;
        
        if (args.HasFlag("free"))
            return false;
        
        if (args.HasFlag("cost") || args.HasFlag("buy"))
            return true;
        
        if (args.HasFlag("storage"))
        {
            var v = args.GetFlag("storage");
            if (v == null) return true;

            if (TryParseBoolLoose(v, out var b))
                return b;
            
            return storage;
        }

        return storage;
    }

    private static bool TryParseBoolLoose(string s, out bool b)
    {
        s = s.Trim();

        if (bool.TryParse(s, out b)) return true;

        // 兼容 1/0、yes/no、y/n、on/off
        switch (s.ToLowerInvariant())
        {
            case "1":
            case "yes":
            case "y":
            case "on":
                b = true; return true;

            case "0":
            case "no":
            case "n":
            case "off":
                b = false; return true;

            default:
                b = false; return false;
        }
    }
}
