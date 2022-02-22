﻿using Spectre.Console;
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
                "use machine-level environment variables instead of user (requires admin privileges)");
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
        if (!problems.Any())
        {
            Console.WriteLine("No problems with PATH found");
            return;
        }

        foreach (var (dir, problem) in problems)
        {
            AnsiConsole.MarkupLine($"{dir} [red]{problemToString(problem)}[/]");
        }

        if (!fix)
        {
            return;
        }

        int originalCount = path.Items.Count;
        int originalPathLen = path.ToString().Length;

        Console.WriteLine("Fixing problems:");
        foreach (var (dir, problem) in problems)
        {
            Console.Write($"{dir}: ");

            if ((problem & (PathProblem.Empty|PathProblem.Missing|PathProblem.NoExecutables)) != 0)
            {
                if (path.RemoveAll(dir))
                {
                    AnsiConsole.Markup("[green]Removed[/] ");
                }
                else
                {
                    AnsiConsole.Markup("[green]Sorted itself out, huh[/] ");
                }
            }

            if (problem.HasFlag(PathProblem.Duplicate))
            {
                // leave only the topmost entry in the PATH
                int index = path.Items.IndexOf(dir);
                bool cleanedUp = false;
                if (index >= 0)
                {
                    for (int i = index + 1; i < path.Items.Count; i++)
                    {
                        if (SemicolonSeparatedString.AreItemsSame(dir, path.Items[i]))
                        {
                            path.Items.RemoveAt(i);
                            cleanedUp = true;
                            i--;
                        }
                    }
                }
                if (cleanedUp)
                {
                    AnsiConsole.Markup("[green]Dupes removed[/] ");
                }
            }
            Console.WriteLine();
        }
        Console.WriteLine();

        // writePath(path, global);
        int newPathLen = path.ToString().Length;
        int savings = originalPathLen - newPathLen;
        int perc = (originalPathLen - newPathLen) * 100 / originalPathLen;

        int itemsRemoved = originalCount - path.Items.Count;
        if (itemsRemoved ==0)
        {
            Console.WriteLine("No problems fixed");
        }
        AnsiConsole.MarkupLine($"[green]{itemsRemoved}[/] unnecessary PATH items removed");
        AnsiConsole.MarkupLine($"[green]{savings}[/] characters saved ({perc}% saved)");
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