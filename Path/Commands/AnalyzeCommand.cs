﻿using Spectre.Console;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace Path.Commands;

class AnalyzeCommand : CommandBase
{
    public override Command GetCommand()
    {
        var analyzeCmd = new Command("analyze", "find invalid/duplicate/redundant entries in PATH")
        {
            new Option<bool>("--fix", () => false, "Make changes to the PATH to fix the issues"),
            new Option<bool>("--whatif", () => false, "Don't save the repairs, just show them (implies --fix)"),
            getGlobalOption(),
        };
        analyzeCmd.Handler = CommandHandler.Create<bool, bool, bool>((fix, whatif, global) => run(fix, whatif, global));
        return analyzeCmd;
    }

    private static void run(bool fix, bool whatif, bool global)
    {
        var path = OSEnv.ReadPath(global);
        IReadOnlySet<string> exts = OSEnv.GetExecutableExtensions();
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

        if (!fix && !whatif)
        {
            return;
        }

        int originalCount = path.Items.Count;
        int originalPathLen = path.ToString().Length;

        Console.WriteLine();
        Console.WriteLine("Fixing problems:");
        Console.WriteLine();
        foreach (var (dir, problem) in problems)
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
        Console.WriteLine();

        if (!whatif)
        {
            OSEnv.WritePath(path, global);
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

    private static readonly IReadOnlyDictionary<PathProblem, string> problemTextMap = new Dictionary<PathProblem, string>
    {
        [PathProblem.Empty] = "Empty",
        [PathProblem.NoExecutables] = "No executables",
        [PathProblem.Duplicate] = "Duplicate",
        [PathProblem.Missing] = "Missing",
    };

    private static string problemToString(PathProblem value)
    {
        var results = new List<string>();
        foreach (var enumValue in Enum.GetValues<PathProblem>())
        {
            if (problemTextMap.ContainsKey(enumValue) && value.HasFlag(enumValue))
            {
                results.Add($"[[{problemTextMap[enumValue]}]]");
            }
        }
        return string.Join(" ", results);
    }
}
