using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Reflection;

namespace Path;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommands(this IServiceCollection services, Assembly assembly)
    {
        var baseType = typeof(Command);
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
        foreach (Type type in types)
        {
            _ = services.AddSingleton(baseType, type);
        }
        return services;
    }
}
