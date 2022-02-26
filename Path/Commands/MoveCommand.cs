using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

public enum MoveType
{
    Top,
    Bottom,
    Before,
    After,
}

class MoveCommand : Command
{
    private readonly IEnvironment env;

    public MoveCommand(IEnvironment env)
        : base("move", "move path directory to another place")
    {
        this.AddDirectoryArgument("directory to move");
        AddArgument(new Argument<MoveType>("move-type", "move action"));
        AddArgument(new Argument<string?>("destination", "destination folder for before/after functions")
        {
            Arity = ArgumentArity.ZeroOrOne
        });
        this.AddGlobalOption();
        Handler = CommandHandler.Create(Run);
        this.env = env;
    }

    public int Run(string directory, MoveType moveType, string? destination, bool global)
    {
        bool isValidDestination()
        {
            if (destination is null)
            {
                Console.Error.WriteLine("destination is required for this operation");
                return false;
            }
            return true;
        }
        var path = env.ReadPath(global);
        if (path.Items.IndexOf(directory) < 0)
        {
            Console.Error.WriteLine("directory doesn't exist in PATH");
            return 1;
        }

        bool result = moveType switch
        {
            MoveType.Before => isValidDestination() && path.MoveBefore(directory, destination!),
            MoveType.After => isValidDestination() && path.MoveAfter(directory, destination!),
            MoveType.Top => path.MoveTo(directory, 0),
            MoveType.Bottom => path.MoveTo(directory, path.Items.Count),
            _ => throw new ArgumentOutOfRangeException(nameof(moveType), $"Invalid value: {moveType}")
        };
        if (!result)
        {
            if (moveType is MoveType.Before or MoveType.After)
            {
                Console.Error.WriteLine("Couldn't find destination directory in PATH");
            }
            else
            {
                Console.Error.WriteLine("Invalid index");
            }
            return 1;
        }
        return 0;
    }
}
