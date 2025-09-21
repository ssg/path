using NUnit.Framework;

namespace PathCli.Tests;

[TestFixture]
public class WindowsPathStringTest
{
    [Test]
    [TestCase("a;b", new[] { "a", "b" })]
    [TestCase("\"a\";\"b\"", new[] { "a", "b" })]
    [TestCase("\"a;b\";c", new[] { "a;b", "c" })]
    [TestCase("\"a;b\";\"c;d\"", new[] { "a;b", "c;d" })]
    public void ctor_validPathString(string input, string[] output)
    {
        var path = new WindowsPathString(input);
        Assert.That(path.Items, Is.EqualTo(output));
    }

    [Test]
    [TestCase("a;b;c", "b", 0, "b;a;c")]
    [TestCase("a;b;c", "b", 1, "a;b;c")]
    [TestCase("a;b;c", "b", 2, "a;c;b")]
    public void MoveTo_ValidInput_Succeeds(string input, string dir, int destIndex, string output)
    {
        var path = new WindowsPathString(input);
        Assert.That(path.MoveTo(dir, destIndex), Is.True);
        Assert.That(path.ToString, Is.EqualTo(output));
    }

    [Test]
    [TestCase("a;b;c", "b", -1)]
    [TestCase("a;b;c", "d", 0)]
    [TestCase("a;b;c", "b", 3)]
    public void MoveTo_InvalidArguments_Fails(string input, string dir, int destIndex)
    {
        var path = new WindowsPathString(input);
        Assert.That(path.MoveTo(dir, destIndex), Is.False);
    }

    [Test]
    [TestCase("a;b;c", "b", "a", "b;a;c")]
    [TestCase("a;b;c", "b", "b", "a;b;c")]
    [TestCase("a;b;c", "b", "c", "a;b;c")]
    public void MoveBefore_ValidArguments_Succeeds(string input, string dir, string destDir, string output)
    {
        var path = new WindowsPathString(input);
        Assert.That(path.MoveBefore(dir, destDir), Is.True);
        Assert.That(path.ToString(), Is.EqualTo(output));
    }

    [Test]
    [TestCase("a;b;c", "b", "a", "a;b;c")]
    [TestCase("a;b;c", "b", "b", "a;b;c")]
    [TestCase("a;b;c", "b", "c", "a;c;b")]
    [TestCase("a;b;c;d", "b", "c", "a;c;b;d")]
    public void MoveAfter_ValidArguments_Succeeds(string input, string dir, string destDir, string output)
    {
        var path = new WindowsPathString(input);
        Assert.That(path.MoveAfter(dir, destDir), Is.True);
        Assert.That(path.ToString(), Is.EqualTo(output));
    }

    [Test]
    [TestCase("")]
    [TestCase("a")]
    [TestCase("a;b")]
    [TestCase("a;b;c")]
    [TestCase("a;\"b;c\"")]
    [TestCase("a;\"b;c\";d")]
    public void ToString_PreservesTheString(string input)
    {
        var path = new WindowsPathString(input);
        Assert.That(path.ToString(), Is.EqualTo(input));
    }

    // Add method tests
    [Test]
    [TestCase("a;b", "c", true, "a;b;c")]
    [TestCase("a;b", "a", false, "a;b")] // Duplicate item
    [TestCase("a;b", "A", false, "a;b")] // Case insensitive duplicate for Windows
    [TestCase("", "a", true, "a")]
    [TestCase("a;b", " c ", true, "a;b;c")] // Trimming whitespace
    [TestCase("a;b", "\"c\"", true, "a;b;c")] // Quoted item gets unquoted
    [TestCase("a;b", "c;d", true, "a;b;\"c;d\"")] // Item with delimiter gets quoted automatically
    public void Add_ValidScenarios_ReturnsExpectedResult(string initial, string itemToAdd, bool expectedResult, string expectedPath)
    {
        var path = new WindowsPathString(initial);
        var result = path.Add(itemToAdd);
        
        Assert.That(result, Is.EqualTo(expectedResult));
        Assert.That(path.ToString(), Is.EqualTo(expectedPath));
    }

    [Test]
    [TestCase("", "  ")]
    [TestCase("a;b", "  ")]
    public void Add_WhitespaceOnly_ReturnsTrue(string initial, string itemToAdd)
    {
        var path = new WindowsPathString(initial);
        var result = path.Add(itemToAdd);
        
        Assert.That(result, Is.True);
        Assert.That(path.Items.Last(), Is.EqualTo(""));
    }

    // RemoveAll method tests
    [Test]
    [TestCase("a;b;c", "b", true, "a;c")]
    [TestCase("a;b;c;b", "b", true, "a;c")] // Multiple occurrences
    [TestCase("a;b;c", "d", false, "a;b;c")] // Item not found
    [TestCase("a;b;c", "B", true, "a;c")] // Case insensitive for Windows
    [TestCase("", "a", false, "")] // Empty path
    [TestCase("a", "a", true, "")] // Remove only item
    [TestCase("a;a;a", "a", true, "")] // Remove all occurrences of same item
    public void RemoveAll_ValidScenarios_ReturnsExpectedResult(string initial, string itemToRemove, bool expectedResult, string expectedPath)
    {
        var path = new WindowsPathString(initial);
        var result = path.RemoveAll(itemToRemove);
        
        Assert.That(result, Is.EqualTo(expectedResult));
        Assert.That(path.ToString(), Is.EqualTo(expectedPath));
    }

    [Test]
    public void RemoveAll_MultipleOccurrencesAtDifferentPositions_RemovesAll()
    {
        var path = new WindowsPathString("a;b;a;c;a;d");
        var result = path.RemoveAll("a");
        
        Assert.That(result, Is.True);
        Assert.That(path.ToString(), Is.EqualTo("b;c;d"));
    }
}