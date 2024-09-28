namespace PathCli.DirectoryAnalyzers;

class ExistenceAnalyzer : IDirectoryAnalyzer
{
    public PathProblem? Analyze(DirectoryInfo directory)
    {
        return directory.Exists ? null : PathProblem.Missing;
    }
}
