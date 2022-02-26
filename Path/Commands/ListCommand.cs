using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

class ListCommand : Command
{
    private readonly IEnvironment env;

    public ListCommand(IEnvironment env)
        : base("list", "list directories in PATH")
    {
        this.AddGlobalOption();
        Handler = CommandHandler.Create(run);
        this.env = env;
    }

    private void run(bool global)
    {
        var path = env.ReadPath(global);
        foreach (string dir in path.Items)
        {
            Console.WriteLine(dir);
        }
    }
}
