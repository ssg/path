using Microsoft.Extensions.DependencyInjection;
using PathCli.DirectoryAnalyzers;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Reflection;

namespace PathCli;

public static class Program
{
#pragma warning disable CS8602 // No possibility of Dereference of a possibly null reference.
    private static readonly string version = typeof(Program).Assembly.GetName().Version.ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

    public static int Main(string[] args)
    {
        var services = new ServiceCollection();

        configureServices(services);

        var provider = services.BuildServiceProvider();
        var rootCmd = new RootCommand($"""
            PATH environment variable manager v{version}
            Copyright (c) 2022-2024 Sedat Kapanoglu - https://github.com/ssg/path
            """)
        {
            TreatUnmatchedTokensAsErrors = true
        };
        var builder = new CommandLineBuilder(rootCmd)
            .UseDefaults()
            .UseServiceProviderCommands(provider);
        var parser = builder.Build();
        return parser.Invoke(args);
    }

#pragma warning disable IDE0058 // Expression value is never used
    private static void configureServices(ServiceCollection services)
    {
        services
            .AddCommands(Assembly.GetExecutingAssembly())
            .AddSingleton<IEnvironment, OSEnvironment>()
            .AddSingleton<IDirectoryAnalyzer, ExistenceAnalyzer>()
            .AddSingleton<IDirectoryAnalyzer, EmptyAnalyzer>();

        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                {
                    services
                        .AddSingleton(StringComparer.OrdinalIgnoreCase)
                        .AddSingleton<IDirectoryAnalyzer>((sp) =>
                        {
                            var env = sp.GetRequiredService<IEnvironment>();
                            return new WindowsMissingExecutableAnalyzer(env.GetExecutableExtensions());
                        });
                    break;
                }

            case PlatformID.Unix:
                services.AddSingleton(StringComparer.Ordinal)
                    .AddSingleton<IDirectoryAnalyzer, UnixMissingExecutableAnalyzer>();
                break;

            case PlatformID.MacOSX:
                services.AddSingleton(StringComparer.OrdinalIgnoreCase)
                    .AddSingleton<IDirectoryAnalyzer, UnixMissingExecutableAnalyzer>();
                break;

            default:
                Console.WriteLine($"Unsupported OS architecture: {Environment.OSVersion.Platform}");
                Environment.Exit(1);
                return;
        }

        services.AddSingleton<PathAnalyzer>();
    }
#pragma warning restore IDE0058 // Expression value is never used
}