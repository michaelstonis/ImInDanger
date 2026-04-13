namespace Rwl.Models;

public sealed class ProjectInfo
{
    public string Type { get; init; } = "unknown";
    public string? TestCommand { get; init; }
    public string? BuildCommand { get; init; }
    public string? LintCommand { get; init; }
    public List<string> SourceDirs { get; init; } = [];
}
