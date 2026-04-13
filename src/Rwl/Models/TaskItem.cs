namespace Rwl.Models;

public enum TaskStatus
{
    Pending,
    InProgress,
    Done,
    Failed,
    Stopped,
}

public sealed class TaskItem
{
    public int Number { get; init; }
    public string Title { get; init; } = "";
    public TaskStatus Status { get; init; } = TaskStatus.Pending;
    public string? Description { get; init; }
    public string? Files { get; init; }
    public string? DependsOn { get; init; }
    public string? Validation { get; init; }

    public string StatusMarker => Status switch
    {
        TaskStatus.Pending => "[ ]",
        TaskStatus.InProgress => "[~]",
        TaskStatus.Done => "[x]",
        TaskStatus.Failed => "[!]",
        TaskStatus.Stopped => "[STOP]",
        _ => "[ ]",
    };

    public string StatusEmoji => Status switch
    {
        TaskStatus.Pending => "○",
        TaskStatus.InProgress => "◉",
        TaskStatus.Done => "✓",
        TaskStatus.Failed => "✗",
        TaskStatus.Stopped => "■",
        _ => "○",
    };
}
