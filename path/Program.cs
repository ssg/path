using Spectre.Console;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Security;

namespace Path;

public static class Program
{
    private const string pathKey = "PATH";
#pragma warning disable CS8602 // No possibility of Dereference of a possibly null reference.
    private static readonly string version = typeof(Program).Assembly.GetName().Version.ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

    public static int Main(string[] args)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            Console.WriteLine("This tool designed to work with Windows environment only");
        }

        static Option getGlobalOption()
        {
            return new Option<bool>(new[] { "--global", "-g" }, () => false,
                "list both system and user environment variables (requires admin privileges)");
        }

        static Argument getDirectoryOption(string description)
        {
            return new Argument<string>("directory", description);
        }

        var listCmd = new Command("list", "list directories in PATH")
        {
            getGlobalOption(),
        };
        listCmd.Handler = CommandHandler.Create<bool>(global => list(global));

        var addCmd = new Command("add", "add directory to PATH")
        {
            getDirectoryOption("directory to add"),
            getGlobalOption(),
        };
        addCmd.Handler = CommandHandler.Create<string, bool>((directory, global) => add(directory, global));

        var removeCmd = new Command("remove", "remove all instances of the directory from PATH")
        {
            getDirectoryOption("directory to remove"),
        };
        removeCmd.Handler = CommandHandler.Create<string, bool>((directory, global) => remove(directory, global));

        var analyzeCmd = new Command("analyze", "find invalid/duplicate/redundant entries in PATH")
        {
            new Option<bool>("--fix", () => false, "Make changes to the PATH to fix the issues"),
            getGlobalOption(),
        };
        analyzeCmd.Handler = CommandHandler.Create<bool, bool>((fix, global) => analyze(fix, global));

        RootCommand cmd = new($"PATH environment variable manager {version} - Copyright (c) 2022 Sedat Kapanoglu - https://github.com/ssg/path")
        {
            listCmd,
            addCmd,
            removeCmd,
            analyzeCmd,
        };

        cmd.TreatUnmatchedTokensAsErrors = true;
        return cmd.Invoke(args);
    }

    private static void analyze(bool fix, bool global)
    {
        var path = readPath(global);
        var pathExt = Environment.GetEnvironmentVariable("PATHEXT");
        var exts = pathExt is string ext 
            ? new HashSet<string>(ext.Split(';')) 
            : new HashSet<string>();
        var analyzer = new PathAnalyzer(exts);
        var problems = analyzer.Analyze(path);
        foreach (var problem in problems)
        {
            AnsiConsole.MarkupLine($"{problem.Key} [red]{problemToString(problem.Value)}[/]");
        }
    }

    private static string problemToString(PathProblem value)
    {
        var results = new List<string>();
        if (value.HasFlag(PathProblem.Empty))
        {
            results.Add("[[Empty]]");
        }
        if (value.HasFlag(PathProblem.NoExecutables))
        {
            results.Add("[[No executables]]");
        }
        if (value.HasFlag(PathProblem.Duplicate))
        {
            results.Add("[[Duplicate]]");
        }
        if (value.HasFlag(PathProblem.Missing))
        {
            results.Add("[[Missing]]");
        }
        return string.Join(" ", results);
    }

    private static void remove(string dir, bool global)
    {
        var path = readPath(global);
        bool found = path.RemoveAll(dir);
        if (found)
        {
            Console.WriteLine($"{dir} removed from PATH");
            writePath(path, global);
            return;
        }
        Console.WriteLine($"{dir} wasn't in PATH");
    }

    private static void add(string dir, bool global)
    {
        var path = readPath(global);
        if (!path.Add(dir))
        {
            Console.WriteLine($"{dir} is already in PATH");
        }
        else
        {
            writePath(path, global);
            Console.WriteLine($"{dir} added to PATH");
        }
    }

    private static void writePath(PathString path, bool global)
    {
        string value = path.ToString();
        try
        {
            Environment.SetEnvironmentVariable(pathKey, value, getEnvTarget(global));
        }
        catch (SecurityException)
        {
            Console.WriteLine("Access denied");
        }
    }

    private static EnvironmentVariableTarget getEnvTarget(bool global)
    {
        return global ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
    }

    private static PathString readPath(bool global)
    {
        string value = Environment.GetEnvironmentVariable(pathKey, getEnvTarget(global)) ?? string.Empty;
        return new PathString(value);
    }

    private static void list(bool global)
    {
        var path = readPath(global);
        foreach (string dir in path.Items)
        {
            Console.WriteLine(dir);
        }
    }

}