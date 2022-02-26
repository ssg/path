
namespace Path;

public interface IEnvironment
{
    IReadOnlySet<string> GetExecutableExtensions();
    PathString ReadPath(bool global);
    void WritePath(PathString path, bool global);
}