using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

class RemoveCommand : CommandBase
{
    public override Command GetCommand()
    {
        var removeCmd = new Command("remove", "remove all instances of the directory from PATH")
        {
            getDirectoryOption("directory to remove"),
        };
        removeCmd.Handler = CommandHandler.Create<string, bool>((directory, global) => run(directory, global));
        return removeCmd;
    }

    private static void run(string dir, bool global)
    {
        var path = OSEnv.ReadPath(global);
        bool found = path.RemoveAll(dir);
        if (!found)
        {
            Console.WriteLine($"{dir} wasn't in PATH");
            return;
        }
        Console.WriteLine($"{dir} removed from PATH");
        OSEnv.WritePath(path, global);
    }
}
