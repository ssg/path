using System.Text.RegularExpressions;

namespace Path;

/// <summary>
/// Case-insensitive item list separated by semicolon
/// </summary>
internal class SemicolonSeparatedString
{
    protected const char PathSeparator = ';';
    protected const char QuoteChar = '"';

    public IList<string> Items { get; }

    private static readonly Regex parser = new("\\s*(\"[^;\"]+\"|[^;\"]+)\\s*;?",
        RegexOptions.Compiled
        |RegexOptions.CultureInvariant
        |RegexOptions.Singleline);

    public SemicolonSeparatedString(string value)
    {
        var matches = parser.Matches(value);
        Items = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            string dir = match.Groups[1].Value;
            if (IsItemEscaped(dir))
            {
                dir = dir[1..^1];
            }
            Items.Add(dir);
        }
    }

    protected static bool IsItemEscaped(string item)
    {
        return item.StartsWith(QuoteChar) && item.EndsWith(QuoteChar);
    }

    public override string ToString()
    {
        return string.Join(PathSeparator, Items
            .Select(s => s.Contains(PathSeparator, StringComparison.Ordinal) ? $"{QuoteChar}{s}{QuoteChar}" : s));
    }

    public bool HasItem(string item)
    {
        return Items.Any(s => AreItemsSame(item, s));
    }

    public static bool AreItemsSame(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
