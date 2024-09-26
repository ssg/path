namespace PathCli.DirectoryAnalyzers;

internal class UnixMissingExecutableAnalyzer : IDirectoryAnalyzer
{
    const UnixFileMode anyExecuteFlag = UnixFileMode.UserExecute
                                      | UnixFileMode.GroupExecute
                                      | UnixFileMode.OtherExecute;

    public PathProblem? Analyze(DirectoryInfo directory)
    {
        return directory
            .EnumerateFiles()
            .Any(f => (f.UnixFileMode & anyExecuteFlag) != 0)
            ? null
            : PathProblem.NoExecutables;
    }
}