using System.CommandLine;
using System.Reflection;

namespace Path;

static class RootCommandExtensions
{
    public static void AddSubCommands(this RootCommand rootCommand, Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Command)));
        foreach (Type type in types)
        {
            var cmd = (Activator.CreateInstance(type) as Command)!;
            rootCommand.AddCommand(cmd);
        }
    }
}
