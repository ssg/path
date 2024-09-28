
namespace PathCli.DirectoryAnalyzers;

class WindowsMissingExecutableAnalyzer(HashSet<string> executableExtensions) : IDirectoryAnalyzer
{
    public WindowsMissingExecutableAnalyzer()
        : this(WindowsEnvironment.GetExecutableExtensions())
    {
    }

    public PathProblem? Analyze(DirectoryInfo directory)
    {
        return directory.EnumerateFiles()
            .Any(f => executableExtensions.Contains(f.Extension))
            ? null
            : PathProblem.NoExecutables;
    }
}
