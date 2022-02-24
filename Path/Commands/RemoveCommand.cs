using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

class RemoveCommand : Command
{
    public RemoveCommand()
        : base("remove", "remove all instances of the directory from PATH")
    {
        this.AddDirectoryArgument("directory to remove");
        Handler = CommandHandler.Create(run);
    }

    private static void run(string directory, bool global)
    {
        var path = OSEnv.ReadPath(global);
        bool found = path.RemoveAll(directory);
        if (!found)
        {
            Console.WriteLine($"{directory} wasn't in PATH");
            return;
        }
        Console.WriteLine($"{directory} removed from PATH");
        OSEnv.WritePath(path, global);
    }
}
