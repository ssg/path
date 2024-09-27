namespace PathCli.DirectoryAnalyzers;

public class WindowsMissingExecutableAnalyzer(HashSet<string> executableExtensions) : IDirectoryAnalyzer
{
    public PathProblem? Analyze(DirectoryInfo directory)
    {
        return directory.EnumerateFiles()
            .Any(f => executableExtensions.Contains(f.Extension))
            ? null
            : PathProblem.NoExecutables;
    }
}
