using NUnit.Framework;

namespace PathCli.Tests;

[TestFixture]
public class UnixPathStringTest
{
#pragma warning disable CA1861 // Avoid constant arrays as arguments
    [Test]
    [TestCase("a:b", new[] { "a", "b" })]
    [TestCase("a:b", new[] { "a", "b" })]
    [TestCase("a:b:c", new[] { "a", "b", "c" })]
    [TestCase("a:b:c:d", new[] { "a", "b", "c", "d" })]
    public void ctor_validPathString(string input, string[] output)
    {
        var path = new UnixPathString(input);
        Assert.That(path.Items, Is.EqualTo(output));
    }

    [Test]
    [TestCase("a:b:c", "b", 0, "b:a:c")]
    [TestCase("a:b:c", "b", 1, "a:b:c")]
    [TestCase("a:b:c", "b", 2, "a:c:b")]
    public void MoveTo_ValidInput_Succeeds(string input, string dir, int destIndex, string output)
    {
        var path = new UnixPathString(input);
        Assert.That(path.MoveTo(dir, destIndex), Is.True);
        Assert.That(path.ToString, Is.EqualTo(output));
    }

    [Test]
    [TestCase("a:b:c", "b", -1)]
    [TestCase("a:b:c", "d", 0)]
    [TestCase("a:b:c", "b", 3)]
    public void MoveTo_InvalidArguments_Fails(string input, string dir, int destIndex)
    {
        var path = new UnixPathString(input);
        Assert.That(path.MoveTo(dir, destIndex), Is.False);
    }

    [Test]
    [TestCase("a:b:c", "b", "a", "b:a:c")]
    [TestCase("a:b:c", "b", "b", "a:b:c")]
    [TestCase("a:b:c", "b", "c", "a:b:c")]
    public void MoveBefore_ValidArguments_Succeeds(string input, string dir, string destDir, string output)
    {
        var path = new UnixPathString(input);
        Assert.That(path.MoveBefore(dir, destDir), Is.True);
        Assert.That(path.ToString(), Is.EqualTo(output));
    }

    [Test]
    [TestCase("a:b:c", "b", "a", "a:b:c")]
    [TestCase("a:b:c", "b", "b", "a:b:c")]
    [TestCase("a:b:c", "b", "c", "a:c:b")]
    [TestCase("a:b:c:d", "b", "c", "a:c:b:d")]
    public void MoveAfter_ValidArguments_Succeeds(string input, string dir, string destDir, string output)
    {
        var path = new UnixPathString(input);
        Assert.That(path.MoveAfter(dir, destDir), Is.True);
        Assert.That(path.ToString(), Is.EqualTo(output));
    }

    [Test]
    [TestCase("")]
    [TestCase("a")]
    [TestCase("a:b")]
    [TestCase("a:b:c")]
    [TestCase("a:b:c")]
    [TestCase("a:b:c:d")]
    public void ToString_PreservesTheString(string input)
    {
        var path = new UnixPathString(input);
        Assert.That(path.ToString(), Is.EqualTo(input));
    }
#pragma warning restore CA1861 // Avoid constant arrays as arguments
}