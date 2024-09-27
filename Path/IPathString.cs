namespace PathCli;

public interface IPathString
{
    bool Add(string item);
    bool MoveAfter(string directory, string destination);
    bool MoveBefore(string directory, string destination);
    bool MoveTo(string directory, int destinationIndex);
    bool RemoveAll(string dir);
}