
namespace PathCli;

interface IEnvironment
{
    HashSet<string> GetExecutableExtensions();
    PathString ReadPath(bool global);
    void WritePath(PathString path, bool global);
}