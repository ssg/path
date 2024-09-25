namespace Path;

/// <summary>
/// PATH version of a SemicolonSeparatedString
/// </summary>
public class PathString(string value) : SemicolonSeparatedString(value)
{
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
