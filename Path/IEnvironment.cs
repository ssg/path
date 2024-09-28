
namespace PathCli;

interface IEnvironment
{
    PathString ReadPath();
    PathString ReadGlobalPath();
    void WritePath(PathString path);
    void WriteGlobalPath(PathString path);
}