using NUnit.Framework;

namespace PathCli.Tests;

[TestFixture]
public class UnixPathStringTest
{
    [Test]
    [TestCase("", new string[] { })]
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

    // Add method tests
    [Test]
    [TestCase("a:b", "c", true, "a:b:c")]
    [TestCase("a:b", "a", false, "a:b")] // Duplicate item
    [TestCase("a:b", "A", true, "a:b:A")] // Case sensitive - different from Windows
    [TestCase("", "a", true, "a")] // Add to empty path
    [TestCase("a:b", " c ", true, "a:b:c")] // Trimming whitespace
    [TestCase("a:b", "/usr/local/bin", true, "a:b:/usr/local/bin")] // Typical Unix path
    public void Add_ValidScenarios_ReturnsExpectedResult(string initial, string itemToAdd, bool expectedResult, string expectedPath)
    {
        var path = new UnixPathString(initial);
        var result = path.Add(itemToAdd);
        
        Assert.That(result, Is.EqualTo(expectedResult));
        Assert.That(path.ToString(), Is.EqualTo(expectedPath));
    }

    [Test]
    [TestCase("", "  ")]
    [TestCase("a:b", "  ")]
    public void Add_WhitespaceOnly_ReturnsTrue(string initial, string itemToAdd)
    {
        var path = new UnixPathString(initial);
        var result = path.Add(itemToAdd);
        
        Assert.That(result, Is.True);
        Assert.That(path.Items.Last(), Is.EqualTo(""));
    }

    [Test]
    public void Add_ItemContainsDelimiter_ThrowsException()
    {
        var path = new UnixPathString("a:b");
        
        Assert.Throws<InvalidOperationException>(() => path.Add("c:d"));
    }

    [Test]
    public void Add_ItemContainsDelimiterInMiddle_ThrowsException()
    {
        var path = new UnixPathString("/usr/bin:/usr/local/bin");
        
        Assert.Throws<InvalidOperationException>(() => path.Add("/path:with:colons"));
    }

    // RemoveAll method tests
    [Test]
    [TestCase("a:b:c", "b", true, "a:c")]
    [TestCase("a:b:c:b", "b", true, "a:c")] // Multiple occurrences
    [TestCase("a:b:c", "d", false, "a:b:c")] // Item not found
    [TestCase("a:b:c", "B", false, "a:b:c")] // Case sensitive - different from Windows
    [TestCase("", "a", false, "")] // Empty path
    [TestCase("a", "a", true, "")] // Remove only item
    [TestCase("a:a:a", "a", true, "")] // Remove all occurrences of same item
    [TestCase("/usr/bin:/usr/local/bin:/opt/bin", "/usr/bin", true, "/usr/local/bin:/opt/bin")] // Unix paths
    public void RemoveAll_ValidScenarios_ReturnsExpectedResult(string initial, string itemToRemove, bool expectedResult, string expectedPath)
    {
        var path = new UnixPathString(initial);
        var result = path.RemoveAll(itemToRemove);
        
        Assert.That(result, Is.EqualTo(expectedResult));
        Assert.That(path.ToString(), Is.EqualTo(expectedPath));
    }

    [Test]
    public void RemoveAll_MultipleOccurrencesAtDifferentPositions_RemovesAll()
    {
        var path = new UnixPathString("a:b:a:c:a:d");
        var result = path.RemoveAll("a");
        
        Assert.That(result, Is.True);
        Assert.That(path.ToString(), Is.EqualTo("b:c:d"));
    }

    [Test]
    public void RemoveAll_CaseSensitive_OnlyRemovesExactMatches()
    {
        var path = new UnixPathString("a:A:b:a:B");
        var result = path.RemoveAll("a");
        
        Assert.That(result, Is.True);
        Assert.That(path.ToString(), Is.EqualTo("A:b:B"));
    }
}