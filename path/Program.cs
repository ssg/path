using System.CommandLine;
using System.CommandLine.Parsing;
using System.Reflection;

namespace Path;

public static class Program
{
#pragma warning disable CS8602 // No possibility of Dereference of a possibly null reference.
    private static readonly string version = typeof(Program).Assembly.GetName().Version.ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

    public static int Main(string[] args)
    {
        checkOS();
        RootCommand cmd = new($@"PATH environment variable manager v{version}
Copyright (c) 2022 Sedat Kapanoglu - https://github.com/ssg/path");
        cmd.TreatUnmatchedTokensAsErrors = true;
        cmd.AddSubCommands(Assembly.GetExecutingAssembly());
        return cmd.Invoke(args);
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