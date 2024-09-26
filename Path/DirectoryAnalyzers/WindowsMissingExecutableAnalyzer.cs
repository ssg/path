namespace PathCli.DirectoryAnalyzers;

public class WindowsMissingExecutableAnalyzer(HashSet<string> executableExtensions) : IDirectoryAnalyzer
{
    public PathProblem? Analyze(DirectoryInfo directory)
    {
        foreach (var file in directory.EnumerateFiles())
        {
            if (executableExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
            {
                return null;
            }
        }
        return PathProblem.NoExecutables;
    }
}
