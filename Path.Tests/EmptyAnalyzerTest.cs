using NUnit.Framework;
using PathCli.DirectoryAnalyzers;

namespace PathCli.Tests;

[TestFixture]
class EmptyAnalyzerTest
{
    [Test]
    public void Analyze_EmptyDirectory_ReturnsEmptyProblem()
    {
        var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        directory.Create();

        var analyzer = new EmptyAnalyzer();
        Assert.That(analyzer.Analyze(directory), Is.EqualTo(PathProblem.Empty));
    }

    [Test]
    public void Analyze_FullDirectory_ReturnsNull()
    {
        string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var directory = new DirectoryInfo(path);
        directory.Create();
        File.WriteAllText(Path.Combine(path, "hello.txt"), "test");

        var analyzer = new EmptyAnalyzer();
        Assert.That(analyzer.Analyze(directory), Is.Null);
    }
}
