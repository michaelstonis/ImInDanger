namespace Rwl.Models;

public sealed class LoopConfig
{
    public int MaxIterations { get; init; } = 20;
    public int TimeoutMinutes { get; init; } = 10;
    public int AutoReviewInterval { get; init; } = 5;
    public List<string> ValidationCommands { get; init; } = [];
    public List<string> AllowedPaths { get; init; } = [];
    public List<string> RestrictedPaths { get; init; } = [];
    public int MaxLinesPerIteration { get; init; } = 200;
    public int MaxFilesPerIteration { get; init; } = 10;
}
