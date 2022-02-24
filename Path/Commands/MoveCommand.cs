using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

public enum MoveType
{
    top,
    bottom,
    before,
    after,
}

class MoveCommand : Command
{
    public MoveCommand()
        : base("move", "move path directory to another place")
    {
        this.AddDirectoryArgument("directory to move");
        AddArgument(new Argument<MoveType>("move-type", "move action"));
        AddArgument(new Argument<string?>("destination", "destination folder for before/after functions")
        {
            Arity = ArgumentArity.ZeroOrOne
        });
        this.AddGlobalOption();
        Handler = CommandHandler.Create(run);
    }

    private static void run(string directory, MoveType moveType, string? destination, bool global)
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
        var path = OSEnv.ReadPath(global);
        if (path.Items.IndexOf(directory) < 0)
        {
            Console.Error.WriteLine("directory doesn't exist in PATH");
            return;
        }

        bool result = moveType switch
        {
            MoveType.before => isValidDestination() && path.MoveBefore(directory, destination!),
            MoveType.after => isValidDestination() && path.MoveAfter(directory, destination!),
            MoveType.top => path.MoveTo(directory, 0),
            MoveType.bottom => path.MoveTo(directory, path.Items.Count),
            _ => throw new ArgumentOutOfRangeException(nameof(moveType), $"Invalid value: {moveType}")
        };
        if (!result)
        {
            if (moveType is MoveType.before or MoveType.after)
            {
                Console.Error.WriteLine("Couldn't find destination directory in PATH");
            }
            else
            {
                Console.Error.WriteLine("Invalid index");
            }
        }
    }
}
