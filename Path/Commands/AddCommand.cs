using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

class AddCommand : Command
{
    public AddCommand()
        : base("add", "add directory to PATH")
    {
        this.AddDirectoryArgument("directory to add");
        this.AddGlobalOption();
        Handler = CommandHandler.Create(run);
    }

    private static void run(string directory, bool global)
    {
        var path = OSEnv.ReadPath(global);
        if (!path.Add(directory))
        {
            Console.WriteLine($"{directory} is already in PATH");
            return;
        }
        OSEnv.WritePath(path, global);
        Console.WriteLine($"{directory} added to PATH");
    }
}
