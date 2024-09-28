using System.Text.RegularExpressions;

namespace PathCli;

class DelimitedString
{
    public char? QuoteChar { get; }
    public StringComparison PathComparison { get; }
    public List<string> Items { get; }
    public char Delimiter { get; }

    public DelimitedString(string value, char delimiter, char? quoteChar, StringComparison pathComparison)
    {
        Delimiter = delimiter;
        QuoteChar = quoteChar;
        PathComparison = pathComparison;
        Items = [];
        if (QuoteChar is not null)
        {
            var matches = Regex.Matches(value, $"""
                    \s*
                        (\{quoteChar}[^\{quoteChar}]+\{quoteChar}
                        |
                        [^\{delimiter}\{quoteChar}]+
                        )
                    \s*\{delimiter}?
                    """,
                RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
            Items.AddRange(matches.Select(m =>
            {
                string dir = m.Groups[1].Value;
                if (IsItemQuoted(dir))
                {
                    dir = dir[1..^1];
                }
                return dir;
            }));
        }
        else
        {
            Items.AddRange(value.Split(delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
    }

    protected bool IsItemQuoted(string item) => QuoteChar is not null
        && item.StartsWith(QuoteChar.Value)
        && item.EndsWith(QuoteChar.Value);

    public override string ToString()
    {
        return QuoteChar is not null
            ? String.Join(Delimiter, Items
                    .Select(s => s.Contains(Delimiter, StringComparison.Ordinal) ? $"{QuoteChar}{s}{QuoteChar}" : s))
            : String.Join(Delimiter, Items);
    }

    public bool HasItem(string item) => Items.Any(s => AreItemsSame(item, s));

    public bool AreItemsSame(string a, string b)
    {
        return string.Equals(a, b, PathComparison);
    }
}
