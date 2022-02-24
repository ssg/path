using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Builder;

namespace Path;

public static class CommandLineBuilderExtensions
{
    public static CommandLineBuilder UseServiceProviderCommands(this CommandLineBuilder builder, ServiceProvider serviceProvider)
    {
        foreach (var command in serviceProvider.GetServices<Command>())
        {
            builder.Command.AddCommand(command);
        }
        return builder;
    }
}
