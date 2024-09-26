using System.ComponentModel;
using System.Reflection;

namespace PathCli;

[Flags]
public enum PathProblem
{
    [Description("None")]
    None = 0,

    [Description("Missing")]
    Missing = 1,

    [Description("Empty")]
    Empty = 2,

    [Description("No executables")]
    NoExecutables = 4,

    [Description("Duplicate")]
    Duplicate = 8,
}

public static class PathProblemExtension
{
    /// <summary>
    /// Return the value of Description attribute.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static string Description(this PathProblem problem)
    {
        var attr = problem
            .GetType()
            .GetMember(problem.ToString())[0]
            .GetCustomAttributes<DescriptionAttribute>(false)
            .SingleOrDefault();
        return attr?.Description ?? problem.ToString();
    }

    public static string ToProblemString(this PathProblem value)
    {
        var results = new List<string>();
        foreach (var enumValue in Enum.GetValues<PathProblem>())
        {
            if (enumValue > 0 && value.HasFlag(enumValue))
            {
                string problemText = enumValue.Description();
                results.Add($"[[{problemText}]]");
            }
        }
        return string.Join(" ", results);
    }
}