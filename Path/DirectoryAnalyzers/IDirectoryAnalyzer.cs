namespace PathCli.DirectoryAnalyzers;

public interface IDirectoryAnalyzer
{
    PathProblem? Analyze(DirectoryInfo directory);
}
