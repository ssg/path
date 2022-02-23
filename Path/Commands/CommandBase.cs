using System.CommandLine;

namespace Path.Commands;

abstract class CommandBase
{
    protected static Option getGlobalOption()
    {
        return new Option<bool>(new[] { "--global", "-g" }, () => false,
            "use machine-level environment variables instead of user (requires admin privileges)");
    }

    protected static Argument getDirectoryOption(string description)
    {
        return new Argument<string>("directory", description);
    }

    public abstract Command GetCommand();
}
