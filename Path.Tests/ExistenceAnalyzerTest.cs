using NUnit.Framework;
using PathCli.DirectoryAnalyzers;

namespace PathCli.Tests;

[TestFixture]
class ExistenceAnalyzerTest
{
    [Test]
    public void Analyze_MissingDirectory_ReturnsMissingProblem()
    {
        var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        var analyzer = new ExistenceAnalyzer();
        Assert.That(analyzer.Analyze(directory), Is.EqualTo(PathProblem.Missing));
    }

    [Test]
    public void Analyze_ExistsAsFile_ReturnsMissingProblem()
    {
        var path = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()); // GetTempFileName creates a file
        var directory = new DirectoryInfo(path);
        var analyzer = new ExistenceAnalyzer();
        Assert.That(analyzer.Analyze(directory), Is.EqualTo(PathProblem.Missing));
    }

    [Test]
    public void Analyze_ExistingDirectory_ReturnsNull()
    {
        var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        directory.Create();

        var analyzer = new ExistenceAnalyzer();
        Assert.That(analyzer.Analyze(directory), Is.Null);
    }
}
