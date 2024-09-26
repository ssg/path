using Microsoft.Extensions.DependencyInjection;
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
        checkOS();
        var provider = new ServiceCollection()
            .AddCommands(Assembly.GetExecutingAssembly())
            .AddSingleton<IEnvironment>(new OSEnvironment())
            .BuildServiceProvider();
        var rootCmd = new RootCommand($@"PATH environment variable manager v{version}
Copyright (c) 2022 Sedat Kapanoglu - https://github.com/ssg/path")
        {
            TreatUnmatchedTokensAsErrors = true
        };
        var builder = new CommandLineBuilder(rootCmd)
            .UseDefaults()
            .UseServiceProviderCommands(provider);
        var parser = builder.Build();
        return parser.Invoke(args);
    }

    private static void checkOS()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            Console.WriteLine("This tool is designed to work with Windows environment only");
            Environment.Exit(1);
        }
    }
}