namespace PathCli.DirectoryAnalyzers;

public class ExistenceAnalyzer : IDirectoryAnalyzer
{
    public PathProblem? Analyze(DirectoryInfo directory)
    {
        return directory.Exists ? null : PathProblem.Missing;
    }
}
