namespace PathCli;

/// <summary>
/// Create an instance of PathAnalyzer.
/// </summary>
/// <param name="executableExtensions">List of executable extenions each prefixed with ".".</param>
class PathAnalyzer(HashSet<string> executableExtensions)
{
    public SortedDictionary<string, PathProblem>Analyze(PathString path)
    {
        // basic analysis
        var results = new SortedDictionary<string, PathProblem>(StringComparer.OrdinalIgnoreCase);
        foreach (string dir in path.Items)
        {
            var problem = analyzeBasicIssues(dir);
            if (problem != PathProblem.None)
            {
                setProblem(results, dir, problem);
            }
        }

        // analyze dupes
        if (executableExtensions.Count != 0)
        {
            var dupes = path.Items.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => g.Key).Distinct();
            foreach (string dir in dupes)
            {
                setProblem(results, dir, PathProblem.Duplicate);
            }
        }

        return results;
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

    private PathProblem analyzeBasicIssues(string dir)
    {
        DirectoryInfo dirInfo = new(dir);
        var problem = checkExistence(dirInfo);
        if (problem != PathProblem.None)
        {
            return problem;
        }

        problem = checkEmptyPath(dirInfo);
        return problem != PathProblem.None ? problem : checkMissingExecutables(dirInfo);
    }

    private PathProblem checkMissingExecutables(DirectoryInfo dirInfo)
    {
        bool exeFound = false;
        foreach (var file in dirInfo.EnumerateFiles())
        {
            if (executableExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
            {
                exeFound = true;
                break;
            }
        }
        return exeFound ? PathProblem.None : PathProblem.NoExecutables;
    }

    private static PathProblem checkEmptyPath(DirectoryInfo dirInfo)
    {
        return dirInfo.EnumerateDirectories().Any() ? PathProblem.None : PathProblem.Empty;
    }

    private static PathProblem checkExistence(DirectoryInfo dirInfo)
    {
        return !dirInfo.Exists ? PathProblem.Missing : PathProblem.None;
    }
}
