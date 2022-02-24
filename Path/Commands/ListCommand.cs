using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

class ListCommand : Command
{
    public ListCommand()
        : base("list", "list directories in PATH")
    {
        this.AddGlobalOption();
        Handler = CommandHandler.Create(run);
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
