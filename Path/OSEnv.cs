using System.Security;

namespace Path
{
    public static class OSEnv
    {
        private const string pathKey = "PATH";
        private const string pathExtKey = "PATHEXT";

        public static void WritePath(PathString path, bool global)
        {
            string value = path.ToString();
            try
            {
                Environment.SetEnvironmentVariable(pathKey, value, getEnvTarget(global));
            }
            catch (SecurityException)
            {
                Console.WriteLine("Access denied");
            }
        }

        private static EnvironmentVariableTarget getEnvTarget(bool global)
        {
            return global ? EnvironmentVariableTarget.Machine : EnvironmentVariableTarget.User;
        }

        public static PathString ReadPath(bool global)
        {
            string value = Environment.GetEnvironmentVariable(pathKey, getEnvTarget(global)) ?? string.Empty;
            return new PathString(value);
        }

        public static IReadOnlySet<string> GetExecutableExtensions()
        {
            var pathExt = Environment.GetEnvironmentVariable(pathExtKey);
            var exts = pathExt is string ext
                ? new HashSet<string>(ext.Split(';'))
                : new HashSet<string>();
            return exts;
        }
    }
}
