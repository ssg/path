﻿namespace PathCli.DirectoryAnalyzers;

public class EmptyAnalyzer : IDirectoryAnalyzer
{
    public PathProblem? Analyze(DirectoryInfo directory)
    {
        return directory.EnumerateDirectories().Any() || directory.EnumerateFiles().Any()
            ? null
            : PathProblem.Empty;
    }
}
