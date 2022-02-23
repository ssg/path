using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

class AddCommand : CommandBase
{
    public override Command GetCommand()
    {
        var addCmd = new Command("add", "add directory to PATH")
        {
            getDirectoryOption("directory to add"),
            getGlobalOption(),
        };
        addCmd.Handler = CommandHandler.Create<string, bool>((directory, global) => run(directory, global));
        return addCmd;
    }

    private static void run(string dir, bool global)
    {
        var path = OSEnv.ReadPath(global);
        if (!path.Add(dir))
        {
            Console.WriteLine($"{dir} is already in PATH");
            return;
        }
        OSEnv.WritePath(path, global);
        Console.WriteLine($"{dir} added to PATH");
    }
}
