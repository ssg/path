using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

class ListCommand : CommandBase
{
    public override Command GetCommand()
    {
        var listCmd = new Command("list", "list directories in PATH")
        {
            getGlobalOption(),
        };
        listCmd.Handler = CommandHandler.Create<bool>(global => run(global));
        return listCmd;
    }

    private static void run(bool global)
    {
        var path = OSEnv.ReadPath(global);
        foreach (string dir in path.Items)
        {
            Console.WriteLine(dir);
        }
    }
}
