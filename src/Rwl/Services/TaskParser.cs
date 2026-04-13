using System.Text.RegularExpressions;
using Rwl.Models;
using TaskStatus = Rwl.Models.TaskStatus;

namespace Rwl.Services;

public static partial class TaskParser
{
    public static List<TaskItem> Parse(string filePath)
    {
        if (!File.Exists(filePath))
            return [];

        var lines = File.ReadAllLines(filePath);
        var tasks = new List<TaskItem>();
        var taskNum = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var heading = HeadingPattern().Match(lines[i]);
            if (!heading.Success) continue;

            taskNum++;
            var title = heading.Groups[1].Value.Trim();

            // Remove leading number prefix like "1. " from title
            var numPrefix = NumberPrefixPattern().Match(title);
            if (numPrefix.Success)
                title = title[numPrefix.Length..];

            var status = TaskStatus.Pending;
            string? description = null, files = null, dependsOn = null, validation = null;

            // Scan subsequent lines until next heading or end
            for (var j = i + 1; j < lines.Length; j++)
            {
                if (lines[j].StartsWith("### "))
                    break;

                var line = lines[j].Trim();

                if (line.Contains("[x]", StringComparison.Ordinal))
                    status = TaskStatus.Done;
                else if (line.Contains("[~]", StringComparison.Ordinal))
                    status = TaskStatus.InProgress;
                else if (line.Contains("[!]", StringComparison.Ordinal))
                    status = TaskStatus.Failed;
                else if (line.Contains("[STOP]", StringComparison.Ordinal))
                    status = TaskStatus.Stopped;
                else if (line.Contains("[ ]", StringComparison.Ordinal))
                    status = TaskStatus.Pending;

                if (line.Contains("**Description:**", StringComparison.Ordinal))
                    description = ExtractValue(line, "Description");
                if (line.Contains("**Files:**", StringComparison.Ordinal))
                    files = ExtractValue(line, "Files");
                if (line.Contains("**Depends on:**", StringComparison.Ordinal))
                    dependsOn = ExtractValue(line, "Depends on");
                if (line.Contains("**Validation:**", StringComparison.Ordinal))
                    validation = ExtractValue(line, "Validation");
            }

            tasks.Add(new TaskItem
            {
                Number = taskNum,
                Title = title,
                Status = status,
                Description = description,
                Files = files,
                DependsOn = dependsOn,
                Validation = validation,
            });
        }

        return tasks;
    }

    public static (int Pending, int InProgress, int Done, int Failed, int Stopped) CountStatuses(string filePath)
    {
        if (!File.Exists(filePath))
            return (0, 0, 0, 0, 0);

        var content = File.ReadAllText(filePath);
        return (
            Pending: PendingPattern().Matches(content).Count,
            InProgress: InProgressPattern().Matches(content).Count,
            Done: DonePattern().Matches(content).Count,
            Failed: FailedPattern().Matches(content).Count,
            Stopped: StoppedPattern().Matches(content).Count
        );
    }

    public static string? ExtractObjective(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var lines = File.ReadAllLines(filePath);
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith("## Objective", StringComparison.OrdinalIgnoreCase))
                continue;
            // Return the next non-empty line
            for (var j = i + 1; j < lines.Length; j++)
            {
                if (string.IsNullOrWhiteSpace(lines[j])) continue;
                if (lines[j].StartsWith("##")) break;
                return lines[j].Trim();
            }
        }
        return null;
    }

    private static string? ExtractValue(string line, string key)
    {
        var idx = line.IndexOf($"**{key}:**", StringComparison.Ordinal);
        if (idx < 0) return null;
        var after = line[(idx + key.Length + 6)..].Trim();
        return after.Trim('`');
    }

    [GeneratedRegex(@"^###\s+(.+)$")]
    private static partial Regex HeadingPattern();

    [GeneratedRegex(@"^\d+\.\s+")]
    private static partial Regex NumberPrefixPattern();

    [GeneratedRegex(@"\[ \]")]
    private static partial Regex PendingPattern();

    [GeneratedRegex(@"\[~\]")]
    private static partial Regex InProgressPattern();

    [GeneratedRegex(@"\[x\]")]
    private static partial Regex DonePattern();

    [GeneratedRegex(@"\[!\]")]
    private static partial Regex FailedPattern();

    [GeneratedRegex(@"\[STOP\]")]
    private static partial Regex StoppedPattern();
}
