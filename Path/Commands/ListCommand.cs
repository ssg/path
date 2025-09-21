using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace PathCli.Commands;

class ListCommand : Command
{
    readonly IEnvironment env;

    public ListCommand(IEnvironment env)
        : base("list", "list directories in PATH")
    {
        this.AddGlobalOption();
        Handler = CommandHandler.Create(Run);
        this.env = env;
    }

    public void Run(bool global)
    {
        var path = global ? env.ReadGlobalPath() : env.ReadPath();
        foreach (string dir in path.Items)
        {
            Console.WriteLine(dir);
        }
    }
}
