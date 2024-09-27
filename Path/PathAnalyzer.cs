using PathCli.DirectoryAnalyzers;

namespace PathCli;

class PathAnalyzer(IEnumerable<IDirectoryAnalyzer> directoryAnalyzers, StringComparer pathComparer)
{
    readonly StringComparer pathComparer = pathComparer;

    public SortedDictionary<string, PathProblem> Analyze(PathString path)
    {
        SortedDictionary<string, PathProblem> results = new(pathComparer);
        foreach (string dir in path.Items)
        {
            DirectoryInfo dirInfo = new(dir);
            var result = runDirectoryAnalyzers(dirInfo);
            if (result is PathProblem problem)
            {
                setProblem(results, dir, problem);
            }
        }

        analyzeDupes(path, results);

        return results;
    }

    private static void analyzeDupes(PathString path, SortedDictionary<string, PathProblem> results)
    {
        var dupes = path.Items.GroupBy(s => s)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .Distinct();
        foreach (string dir in dupes)
        {
            setProblem(results, dir, PathProblem.Duplicate);
        }
    }

    private static void setProblem(SortedDictionary<string, PathProblem> results, string dir, PathProblem problem)
    {
        if (results.ContainsKey(dir))
        {
            results[dir] |= problem;
            return;
        }
        results.Add(dir, problem);
    }

    private PathProblem? runDirectoryAnalyzers(DirectoryInfo dirInfo)
    {
        foreach (var analyzer in directoryAnalyzers)
        {
            if (analyzer.Analyze(dirInfo) is PathProblem problem)
            {
                return problem;
            }
        }

        return null;
    }

}
