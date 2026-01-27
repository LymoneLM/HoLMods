using YuanAPI.Tools;

namespace HoLConsole;

internal static class PropCommands
{
    public static void Register()
    {
        ConsoleAPI.RegisterCommand(new CommandDef(
            name: "give",
            description: "Give a prop",
            usage: "give [PropID] [Count]",
            handler: (ctx, args) =>
            {
                var id = int.Parse(args.Positionals[0]);
                var count = int.Parse(args.Positionals[1]);
                PropTool.AddProp(id, count);
                return "";
            }
        ));
    }
}