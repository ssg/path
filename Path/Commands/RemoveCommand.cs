using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Security;

namespace PathCli.Commands;

class RemoveCommand : Command
{
    readonly IEnvironment env;

    public RemoveCommand(IEnvironment env)
        : base("remove", "remove all instances of the directory from PATH")
    {
        this.AddDirectoryArgument("directory to remove");
        Handler = CommandHandler.Create(Run);
        this.env = env;
    }

    public void Run(string directory, bool global)
    {
        var path = global ? env.ReadGlobalPath() : env.ReadPath();
        bool found = path.RemoveAll(directory);
        if (!found)
        {
            Console.WriteLine($"{directory} wasn't in PATH");
            return;
        }
        Console.WriteLine($"{directory} removed from PATH");

        try
        {
            if (global)
            {
                env.WriteGlobalPath(path);
            }
            else
            {
                env.WritePath(path);
            }
        }
        catch (SecurityException)
        {
            Console.Error.WriteLine("Access denied. Try running the command as an administrator.");
            Environment.Exit(1);
        }
    }
}
