using NUnit.Framework;
using PathCli.DirectoryAnalyzers;

namespace PathCli.Tests;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
class PathAnalyzerTest
{
    private readonly TestDirectoryAnalyzer testAnalyzer1;
    private readonly TestDirectoryAnalyzer testAnalyzer2;
    private readonly PathAnalyzer analyzer;

    public PathAnalyzerTest()
    {
        testAnalyzer1 = new TestDirectoryAnalyzer();
        testAnalyzer2 = new TestDirectoryAnalyzer();
        var analyzers = new List<IDirectoryAnalyzer> { testAnalyzer1, testAnalyzer2 };
        analyzer = new PathAnalyzer(analyzers, StringComparer.Ordinal);
    }

    [Test]
    public void Analyze_EmptyPath_ReturnsEmptyResults()
    {
        var path = new UnixPathString("");
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Analyze_SingleDirectoryNoProblem_ReturnsEmptyResults()
    {
        var path = new UnixPathString("/usr/bin");
        var normalizedPath = new DirectoryInfo("/usr/bin").FullName;
        testAnalyzer1.SetResult(normalizedPath, null);
        testAnalyzer2.SetResult(normalizedPath, null);
        
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Analyze_SingleDirectoryWithProblem_ReturnsProblem()
    {
        var path = new UnixPathString("/usr/bin");
        var normalizedPath = new DirectoryInfo("/usr/bin").FullName;
        testAnalyzer1.SetResult(normalizedPath, PathProblem.Missing);
        
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results["/usr/bin"], Is.EqualTo(PathProblem.Missing));
    }

    [Test]
    public void Analyze_MultipleDirectoriesWithProblems_ReturnsAllProblems()
    {
        var path = new UnixPathString("/usr/bin:/usr/local/bin:/opt/bin");
        
        var normalizedPath1 = new DirectoryInfo("/usr/bin").FullName;
        var normalizedPath2 = new DirectoryInfo("/usr/local/bin").FullName;
        var normalizedPath3 = new DirectoryInfo("/opt/bin").FullName;
        
        testAnalyzer1.SetResult(normalizedPath1, PathProblem.Missing);
        testAnalyzer1.SetResult(normalizedPath2, null);
        testAnalyzer1.SetResult(normalizedPath3, PathProblem.Empty);
        testAnalyzer2.SetResult(normalizedPath2, PathProblem.NoExecutables);
        
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(3));
        Assert.That(results["/usr/bin"], Is.EqualTo(PathProblem.Missing));
        Assert.That(results["/usr/local/bin"], Is.EqualTo(PathProblem.NoExecutables));
        Assert.That(results["/opt/bin"], Is.EqualTo(PathProblem.Empty));
    }

    [Test]
    public void Analyze_FirstAnalyzerReturnsProblems_DoesNotRunSecondAnalyzer()
    {
        var path = new UnixPathString("/usr/bin");
        var normalizedPath = new DirectoryInfo("/usr/bin").FullName;
        
        testAnalyzer1.SetResult(normalizedPath, PathProblem.Missing);
        testAnalyzer2.SetResult(normalizedPath, PathProblem.NoExecutables);
        
        var results = analyzer.Analyze(path);
        
        // Only the first analyzer's result should be returned
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results["/usr/bin"], Is.EqualTo(PathProblem.Missing));
        
        // Verify first analyzer was called
        Assert.That(testAnalyzer1.CallCount(normalizedPath), Is.EqualTo(1));
        // Second analyzer should not be called since first one returned a problem
        Assert.That(testAnalyzer2.CallCount(normalizedPath), Is.EqualTo(0));
    }

    [Test]
    public void Analyze_FirstAnalyzerReturnsNull_RunsSecondAnalyzer()
    {
        var path = new UnixPathString("/usr/bin");
        var normalizedPath = new DirectoryInfo("/usr/bin").FullName;
        
        testAnalyzer1.SetResult(normalizedPath, null);
        testAnalyzer2.SetResult(normalizedPath, PathProblem.NoExecutables);
        
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results["/usr/bin"], Is.EqualTo(PathProblem.NoExecutables));
        
        // Both analyzers should be called
        Assert.That(testAnalyzer1.CallCount(normalizedPath), Is.EqualTo(1));
        Assert.That(testAnalyzer2.CallCount(normalizedPath), Is.EqualTo(1));
    }

    [Test]
    public void Analyze_DuplicateDirectories_AddsDuplicateProblem()
    {
        var path = new UnixPathString("/usr/bin:/usr/local/bin:/usr/bin");
        
        var normalizedPath1 = new DirectoryInfo("/usr/bin").FullName;
        var normalizedPath2 = new DirectoryInfo("/usr/local/bin").FullName;
        
        testAnalyzer1.SetResult(normalizedPath1, null);
        testAnalyzer1.SetResult(normalizedPath2, null);
        
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results["/usr/bin"], Is.EqualTo(PathProblem.Duplicate));
    }

    [Test]
    public void Analyze_DuplicateDirectoriesWithOtherProblems_CombinesWithDuplicateProblem()
    {
        var path = new UnixPathString("/usr/bin:/usr/local/bin:/usr/bin");
        
        var normalizedPath1 = new DirectoryInfo("/usr/bin").FullName;
        var normalizedPath2 = new DirectoryInfo("/usr/local/bin").FullName;
        
        testAnalyzer1.SetResult(normalizedPath1, PathProblem.Missing);
        testAnalyzer1.SetResult(normalizedPath2, null);
        
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results["/usr/bin"], Is.EqualTo(PathProblem.Missing | PathProblem.Duplicate));
    }

    [Test]
    public void Analyze_MultipleDuplicateDirectories_AllMarkedAsDuplicate()
    {
        var path = new UnixPathString("/usr/bin:/usr/local/bin:/usr/bin:/opt/bin:/usr/local/bin");
        
        var normalizedPath1 = new DirectoryInfo("/usr/bin").FullName;
        var normalizedPath2 = new DirectoryInfo("/usr/local/bin").FullName;
        var normalizedPath3 = new DirectoryInfo("/opt/bin").FullName;
        
        testAnalyzer1.SetResult(normalizedPath1, null);
        testAnalyzer1.SetResult(normalizedPath2, null);
        testAnalyzer1.SetResult(normalizedPath3, null);
        
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results["/usr/bin"], Is.EqualTo(PathProblem.Duplicate));
        Assert.That(results["/usr/local/bin"], Is.EqualTo(PathProblem.Duplicate));
    }

    [Test]
    public void Analyze_CaseInsensitiveComparer_DuplicateDetectionUsesCaseSensitiveComparison()
    {
        var caseInsensitiveAnalyzer = new PathAnalyzer(
            [testAnalyzer1!, testAnalyzer2!], 
            StringComparer.OrdinalIgnoreCase);
        var path = new WindowsPathString(@"C:\Windows\System32;C:\Program Files;c:\windows\system32");
        
        var normalizedPath1 = new DirectoryInfo(@"C:\Windows\System32").FullName;
        var normalizedPath2 = new DirectoryInfo(@"C:\Program Files").FullName;
        var normalizedPath3 = new DirectoryInfo(@"c:\windows\system32").FullName;
        
        testAnalyzer1.SetResult(normalizedPath1, null);
        testAnalyzer1.SetResult(normalizedPath2, null);
        testAnalyzer1.SetResult(normalizedPath3, null);
        
        var results = caseInsensitiveAnalyzer.Analyze(path);
        
        // The duplicate detection in analyzeDupes method uses default string equality,
        // not the pathComparer, so "C:\Windows\System32" and "c:\windows\system32" 
        // are NOT detected as duplicates even with case-insensitive comparer
        // This test verifies the current behavior (which might be a bug, but it's the current implementation)
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Analyze_ExactStringDuplicates_AlwaysDetectedRegardlessOfComparer()
    {
        var caseInsensitiveAnalyzer = new PathAnalyzer(
            [testAnalyzer1!], 
            StringComparer.OrdinalIgnoreCase);
        var path = new WindowsPathString(@"C:\Windows\System32;C:\Program Files;C:\Windows\System32");
        
        var normalizedPath1 = new DirectoryInfo(@"C:\Windows\System32").FullName;
        var normalizedPath2 = new DirectoryInfo(@"C:\Program Files").FullName;
        
        testAnalyzer1.SetResult(normalizedPath1, null);
        testAnalyzer1.SetResult(normalizedPath2, null);
        
        var results = caseInsensitiveAnalyzer.Analyze(path);
        
        // Exact string duplicates are always detected
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[@"C:\Windows\System32"], Is.EqualTo(PathProblem.Duplicate));
    }

    [Test]
    public void Analyze_NoAnalyzersProvided_OnlyDetectsDuplicates()
    {
        var emptyAnalyzer = new PathAnalyzer([], StringComparer.Ordinal);
        var path = new UnixPathString("/usr/bin:/usr/local/bin:/usr/bin");
        
        var results = emptyAnalyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results["/usr/bin"], Is.EqualTo(PathProblem.Duplicate));
    }

    [Test]
    public void Analyze_AnalyzerReturnsNull_NoExceptionThrown()
    {
        var path = new UnixPathString("/usr/bin");
        var normalizedPath = new DirectoryInfo("/usr/bin").FullName;
        
        testAnalyzer1.SetResult(normalizedPath, null);
        testAnalyzer2.SetResult(normalizedPath, null);
        
        Assert.DoesNotThrow(() =>
        {
            var results = analyzer.Analyze(path);
            Assert.That(results, Is.Empty);
        });
    }

    [Test]
    public void Analyze_DuplicateWithAnalyzerProblem_CombinesCorrectly()
    {
        var path = new UnixPathString("/problem/dir:/problem/dir");
        var normalizedPath = new DirectoryInfo("/problem/dir").FullName;
        
        testAnalyzer1.SetResult(normalizedPath, PathProblem.Missing);
        
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(1));
        var expectedProblems = PathProblem.Missing | PathProblem.Duplicate;
        Assert.That(results["/problem/dir"], Is.EqualTo(expectedProblems));
    }

    [Test]
    public void Analyze_OrderOfAnalyzersMatters_FirstProblemWins()
    {
        var path = new UnixPathString("/usr/bin");
        var normalizedPath = new DirectoryInfo("/usr/bin").FullName;
        
        // First analyzer returns Missing
        testAnalyzer1.SetResult(normalizedPath, PathProblem.Missing);
        // Second analyzer returns NoExecutables (but shouldn't be called)
        testAnalyzer2.SetResult(normalizedPath, PathProblem.NoExecutables);
        
        var results = analyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results["/usr/bin"], Is.EqualTo(PathProblem.Missing));
    }

    [Test]
    public void Analyze_WindowsPathString_WorksCorrectly()
    {
        var windowsAnalyzer = new PathAnalyzer([testAnalyzer1!], StringComparer.OrdinalIgnoreCase);
        var path = new WindowsPathString(@"C:\Windows\System32;C:\Program Files");
        
        var normalizedPath1 = new DirectoryInfo(@"C:\Windows\System32").FullName;
        var normalizedPath2 = new DirectoryInfo(@"C:\Program Files").FullName;
        
        testAnalyzer1.SetResult(normalizedPath1, PathProblem.Missing);
        testAnalyzer1.SetResult(normalizedPath2, PathProblem.Empty);
        
        var results = windowsAnalyzer.Analyze(path);
        
        Assert.That(results, Has.Count.EqualTo(2));
        Assert.That(results[@"C:\Windows\System32"], Is.EqualTo(PathProblem.Missing));
        Assert.That(results[@"C:\Program Files"], Is.EqualTo(PathProblem.Empty));
    }

    private class TestDirectoryAnalyzer : IDirectoryAnalyzer
    {
        private readonly Dictionary<string, PathProblem?> results = [];
        private readonly Dictionary<string, int> callCounts = [];

        public PathProblem? Analyze(DirectoryInfo directory)
        {
            var path = directory.FullName;
            callCounts[path] = callCounts.GetValueOrDefault(path) + 1;
            return results.GetValueOrDefault(path);
        }

        public void SetResult(string path, PathProblem? problem)
        {
            results[path] = problem;
        }

        public int CallCount(string path)
        {
            return callCounts.GetValueOrDefault(path);
        }
    }
}