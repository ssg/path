using System.Security;

namespace PathCli;

class WindowsEnvironment : IEnvironment
{
    const string pathKey = "PATH";

    public void WritePath(PathString path)
    {
        string value = path.ToString();
        Environment.SetEnvironmentVariable(pathKey, value);
    }

    public void WriteGlobalPath(PathString path)
    {
        string value = path.ToString();
        Environment.SetEnvironmentVariable(pathKey, value, EnvironmentVariableTarget.Machine);
    }

    public PathString ReadPath()
    {
        string value = Environment.GetEnvironmentVariable(pathKey) ?? string.Empty;
        return new WindowsPathString(value);
    }

    public PathString ReadGlobalPath()
    {
        string value = Environment.GetEnvironmentVariable(pathKey, EnvironmentVariableTarget.Machine) ?? string.Empty;
        return new WindowsPathString(value);
    }

    public static HashSet<string> GetExecutableExtensions()
    {
        const string pathExtKey = "PATHEXT";
        const char pathExtSeparator = ';';

        var pathExt = Environment.GetEnvironmentVariable(pathExtKey);
        var exts = pathExt is string ext
            ? new HashSet<string>(ext.Split(pathExtSeparator), StringComparer.OrdinalIgnoreCase)
            : [];
        return exts;
    }
}
