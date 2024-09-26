using NUnit.Framework;
using PathCli.DirectoryAnalyzers;

namespace PathCli.Tests;

[TestFixture]
class WindowsMissingExecutableAnalyzerTest
{
    [Test]
    public void Analyze_NoExecutables_ReturnsNoExecutablesProblem()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var directory = new DirectoryInfo(path);
        directory.Create();

        File.WriteAllText(Path.Combine(path, "hello.txt"), "test");
        File.WriteAllText(Path.Combine(path, "hello.md"), "test");

        var analyzer = new WindowsMissingExecutableAnalyzer([".exe"]);
        Assert.That(analyzer.Analyze(directory), Is.EqualTo(PathProblem.NoExecutables));
    }

    [Test]
    public void Analyze_Executables_ReturnsNull()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var directory = new DirectoryInfo(path);
        directory.Create();

        File.WriteAllText(Path.Combine(path, "hello.txt"), "test");
        File.WriteAllText(Path.Combine(path, "hello.exe"), "test");
        File.WriteAllText(Path.Combine(path, "hello.md"), "test");

        var analyzer = new WindowsMissingExecutableAnalyzer([".exe"]);
        Assert.That(analyzer.Analyze(directory), Is.Null);
    }

}
