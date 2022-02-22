using NUnit.Framework;

namespace Path.Tests;

[TestFixture]
public class PathStringTest
{
    [Test]
    [TestCase("a;b", new[] { "a", "b" })]
    [TestCase("\"a\";\"b\"", new[] { "a", "b" })]
    [TestCase("\"a;b\";c", new[] { "a;b", "c" })]
    [TestCase("\"a;b\";\"c;d\"", new[] { "a;b", "c;d" })]
    public void ctor_validPathString(string input, string[] output)
    {
        var path = new PathString(input);
        Assert.That(path.Items, Is.EqualTo(output));
    }
}
