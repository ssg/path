using Path.Commands;
using System.CommandLine;
using System.Reflection;

namespace Path;

static class CommandBuilderExtension
{
    public static void AddSubCommands(this Command rootCommand, Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(CommandBase)));
        foreach (Type type in types)
        {
            var cmd = (Activator.CreateInstance(type) as CommandBase)!;
            rootCommand.AddCommand(cmd.GetCommand());
        }
    }
}
