using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

class AddCommand : Command
{
    private readonly IEnvironment env;

    public AddCommand(IEnvironment env)
        : base("add", "add directory to PATH")
    {
        this.AddDirectoryArgument("directory to add");
        this.AddGlobalOption();
        Handler = CommandHandler.Create(run);
        this.env = env;
    }

    private void run(string directory, bool global)
    {
        var path = env.ReadPath(global);
        if (!path.Add(directory))
        {
            Console.WriteLine($"{directory} is already in PATH");
            return;
        }
        env.WritePath(path, global);
        Console.WriteLine($"{directory} added to PATH");
    }
}
