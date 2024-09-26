namespace PathCli;

[Flags]
enum PathProblem
{
    None = 0,
    Missing = 1,
    Empty = 2,
    NoExecutables = 4,
    Duplicate = 8,
}
