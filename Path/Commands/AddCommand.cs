using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Security;

namespace PathCli.Commands;

class AddCommand : Command
{
    private readonly IEnvironment env;

    public AddCommand(IEnvironment env)
        : base("add", "add directory to PATH")
    {
        this.AddDirectoryArgument("directory to add");
        this.AddGlobalOption();
        Handler = CommandHandler.Create(Run);
        this.env = env;
    }

    public void Run(string directory, bool global)
    {
        if (global)
        {
            runGlobal(directory);
        }

        var path = env.ReadPath();
        if (!path.Add(directory))
        {
            alreadyInPath(directory);
            return;
        }
        try
        {
            env.WritePath(path);
        }
        catch (NotImplementedException)
        {
            Console.Error.WriteLine("This operation isn't supported");
            Environment.Exit(1);
        }

        reportAdded(directory);
    }

    private void runGlobal(string directory)
    {
        var path = env.ReadGlobalPath();
        if (!path.Add(directory))
        {
            alreadyInPath(directory);
            return;
        }
        try
        {
            env.WriteGlobalPath(path);
        }
        catch (SecurityException)
        {
            Console.Error.WriteLine("Access denied");
            Environment.Exit(1);
        }

        reportAdded(directory);
    }

    private static void reportAdded(string directory)
    {
        Console.WriteLine($"{directory} added to PATH");
    }

    private static void alreadyInPath(string directory)
    {
        Console.WriteLine($"{directory} is already in PATH");
        return;
    }
}
