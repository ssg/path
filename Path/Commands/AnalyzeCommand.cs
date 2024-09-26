﻿using PathCli.DirectoryAnalyzers;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace PathCli.Commands;

class AnalyzeCommand : Command
{
    private readonly IEnvironment env;

    public AnalyzeCommand(IEnvironment env)
        : base("analyze", "find invalid/duplicate/redundant entries in PATH")
    {
        AddOption(new Option<bool>("--fix", () => false, "Make changes to the PATH to fix the issues"));
        AddOption(new Option<bool>("--whatif", () => false, "Don't save the repairs, just show them (implies --fix)"));
        this.AddGlobalOption();
        Handler = CommandHandler.Create(Run);
        this.env = env;
    }

    public void Run(bool fix, bool whatif, bool global)
    {
        var path = env.ReadPath(global);

        List<IDirectoryAnalyzer> analyzers = buildAnalyzers();

        var analyzer = new PathAnalyzer(analyzers);
        var problems = analyzer.Analyze(path);
        if (problems.Count == 0)
        {
            Console.WriteLine("No problems with PATH found");
            return;
        }

        foreach (var (dir, problem) in problems)
        {
            AnsiConsole.MarkupLine($"{dir} [red]{problem.ToProblemString()}[/]");
        }

        if (!fix && !whatif)
        {
            return;
        }

        fixProblems(whatif, global, path, problems);
    }

    private void fixProblems(bool whatif, bool global, PathString path, SortedDictionary<string, PathProblem> problems)
    {
        int originalCount = path.Items.Count;
        int originalPathLen = path.ToString().Length;

        Console.WriteLine();
        Console.WriteLine("Fixing problems:");
        Console.WriteLine();
        foreach (var (dir, problem) in problems)
        {
            fixProblem(path, dir, problem);
        }
        Console.WriteLine();

        if (!whatif)
        {
            env.WritePath(path, global);
            Console.WriteLine("Changes saved");
        }
        int newPathLen = path.ToString().Length;
        int savings = originalPathLen - newPathLen;
        int perc = (originalPathLen - newPathLen) * 100 / originalPathLen;

        int itemsRemoved = originalCount - path.Items.Count;
        if (itemsRemoved == 0)
        {
            Console.WriteLine("No problems fixed");
        }
        AnsiConsole.MarkupLine($"[green]{itemsRemoved}[/] unnecessary PATH items removed");
        AnsiConsole.MarkupLine($"[green]{savings}[/] characters saved ({perc}% saved)");
    }

    private static void fixProblem(PathString path, string dir, PathProblem problem)
    {
        Console.Write($"{dir}: ");

        if ((problem & (PathProblem.Empty | PathProblem.Missing | PathProblem.NoExecutables)) != 0)
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

    private List<IDirectoryAnalyzer> buildAnalyzers()
    {
        var analyzers = new List<IDirectoryAnalyzer>
        {
            new ExistenceAnalyzer(),
            new EmptyAnalyzer(),
        };
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                var exts = env.GetExecutableExtensions();
                analyzers.Add(new WindowsMissingExecutableAnalyzer(exts));
                break;

            case PlatformID.Unix:
                analyzers.Add(new UnixMissingExecutableAnalyzer());
                break;
            default:
                Console.WriteLine($"Unsupported OS architecture: {Environment.OSVersion.Platform}");
                Environment.Exit(1);
                break;
        }
        return analyzers;
    }
}

