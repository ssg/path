namespace PathCli;

class WindowsPathString(string value) 
    : PathString(value, delimiter: ';', quoteChar: '"', StringComparison.OrdinalIgnoreCase)
{
}

class UnixPathString(string value) 
    : PathString(value, delimiter: ':', quoteChar: null, StringComparison.Ordinal)
{
}

abstract class PathString(string value, char delimiter, char? quoteChar, StringComparison pathComparison) 
    : DelimitedString(value, delimiter, quoteChar, pathComparison), IPathString
{
    public bool Add(string item)
    {
        item = item.Trim();
        if (IsItemQuoted(item))
        {
            item = item[1..^1];
        }

        if (HasItem(item))
        {
            return false;
        }

        if (QuoteChar is null && item.Contains(Delimiter))
        {
            throw new InvalidOperationException($"Item cannot contain delimiter '{Delimiter}'");
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

    public bool MoveBefore(string directory, string destination)
    {
        int srcIndex = Items.IndexOf(directory);
        int destIndex = Items.IndexOf(destination);
        if (destIndex > srcIndex)
        {
            destIndex--;
        }
        return move(directory, srcIndex, destIndex);
    }

    public bool MoveAfter(string directory, string destination)
    {
        int srcIndex = Items.IndexOf(directory);
        int destIndex = Items.IndexOf(destination);
        if (srcIndex > destIndex)
        {
            destIndex++;
        }
        return move(directory, srcIndex, destIndex);
    }

    public bool MoveTo(string directory, int destinationIndex)
    {
        return move(directory, Items.IndexOf(directory), destinationIndex);
    }

    private bool move(string directory, int srcIndex, int destIndex)
    {
        if (srcIndex < 0 || destIndex < 0 || destIndex >= Items.Count)
        {
            return false;
        }
        if (srcIndex != destIndex)
        {
            Items.RemoveAt(srcIndex);
            Items.Insert(destIndex, directory);
        }
        return true;
    }
}
