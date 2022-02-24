using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Reflection;

namespace Path;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Command)));
        foreach (Type type in types)
        {
            Command cmd = (Command)Activator.CreateInstance(type)!;
            _ = services.AddSingleton(cmd);
        }
        return services;
    }
}
