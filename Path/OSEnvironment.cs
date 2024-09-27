using System.Security;

namespace PathCli;

class OSEnvironment : IEnvironment
{
    const string pathKey = "PATH";
    const string pathExtKey = "PATHEXT";
    const char pathExtSeparator = ';';
    readonly bool isWindows;

    public OSEnvironment()
    {
        isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
    }

    public void WritePath(PathString path, bool global)
    {
        string value = path.ToString();
        try
        {
            Environment.SetEnvironmentVariable(pathKey, value, getEnvTarget(global));
        }
        catch (SecurityException)
        {
            Console.WriteLine("Access denied");
            Environment.Exit(1);
        }
    }

    private static EnvironmentVariableTarget getEnvTarget(bool global)
    {
        return global ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
    }

    public PathString ReadPath(bool global)
    {
        string value = Environment.GetEnvironmentVariable(pathKey, getEnvTarget(global)) ?? string.Empty;
        return isWindows ? new WindowsPathString(value) : new UnixPathString(value);
    }

    public HashSet<string> GetExecutableExtensions()
    {
        var pathExt = Environment.GetEnvironmentVariable(pathExtKey);
        var exts = pathExt is string ext
            ? new HashSet<string>(ext.Split(pathExtSeparator), StringComparer.OrdinalIgnoreCase)
            : [];
        return exts;
    }
}
