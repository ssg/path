using System.Security;

namespace PathCli;

class UnixEnvironment : IEnvironment
{
    const string pathKey = "PATH";

    public PathString ReadPath()
    {
        string value = Environment.GetEnvironmentVariable(pathKey) ?? String.Empty;
        return new UnixPathString(value);
    }

    public PathString ReadGlobalPath()
    {
        string value = Environment.GetEnvironmentVariable(pathKey, EnvironmentVariableTarget.Machine) ?? String.Empty;
        return new UnixPathString(value);
    }

    public void WritePath(PathString path)
    {
        throw new NotImplementedException("Only --global is supported for setting path on Unix systems");
    }

    public void WriteGlobalPath(PathString path)
    {
        string value = path.ToString();
        try
        {
            Environment.SetEnvironmentVariable(pathKey, value, EnvironmentVariableTarget.Machine);
        }
        catch (SecurityException)
        {
            Console.WriteLine("Access denied");
            Environment.Exit(1);
        }
    }
}
