namespace Path;

/// <summary>
/// PATH version of a SemicolonSeparatedString
/// </summary>
public class PathString : SemicolonSeparatedString
{
    public PathString(string value)
        : base(value)
    {
    }

    public bool Add(string item)
    {
        item = item.Trim();
        if (IsItemEscaped(item))
        {
            item = item[1..^1];
        }

        if (HasItem(item))
        {
            return false;
        }
        Items.Add(item);
        return true;
    }

    public bool RemoveAll(string dir)
    {
        bool found = false;
        for (int i = 0; i < Items.Count; i++)
        {
            if (AreItemsSame(dir, Items[i]))
            {
                Items.RemoveAt(i);
                found = true;
                i -= 1;
            }
        }
        return found;
    }
}
