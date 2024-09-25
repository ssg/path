using System.CommandLine;

namespace Path;

public static class CommandExtensions
{
    public static void AddGlobalOption(this Command cmd)
    {
        cmd.AddOption(new Option<bool>(["--global", "-g"], () => false,
            "use machine-level environment variables instead of user (requires admin privileges)"));
    }

    public static void AddDirectoryArgument(this Command cmd, string description)
    {
        cmd.AddArgument(new Argument<string>("directory", description));
    }
}
