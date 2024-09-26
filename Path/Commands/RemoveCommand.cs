using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace PathCli.Commands;

class RemoveCommand : Command
{
    private readonly IEnvironment env;

    public RemoveCommand(IEnvironment env)
        : base("remove", "remove all instances of the directory from PATH")
    {
        this.AddDirectoryArgument("directory to remove");
        Handler = CommandHandler.Create(Run);
        this.env = env;
    }

    public void Run(string directory, bool global)
    {
        var path = env.ReadPath(global);
        bool found = path.RemoveAll(directory);
        if (!found)
        {
            Console.WriteLine($"{directory} wasn't in PATH");
            return;
        }
        Console.WriteLine($"{directory} removed from PATH");
        env.WritePath(path, global);
    }
}
