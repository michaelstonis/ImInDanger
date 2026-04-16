using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.AI;
using Rwl.Models;
using TaskStatus = Rwl.Models.TaskStatus;

namespace Rwl.Services;

/// <summary>
/// Custom tools exposed to the Copilot agent via the SDK.
/// Each method becomes a callable tool the agent can invoke during a session.
/// </summary>
public static class CopilotTools
{
    /// <summary>
    /// Creates the list of AIFunction tools for a loop iteration.
    /// </summary>
    public static IList<AIFunction> CreateTools(string workingDirectory)
    {
        var tasksPath = Path.Combine(workingDirectory, "TASKS.md");
        var progressPath = Path.Combine(workingDirectory, "PROGRESS.md");
        var configPath = Path.Combine(workingDirectory, "LOOP_CONFIG.md");

        return
        [
            AIFunctionFactory.Create(
                ([Description("Read all tasks from TASKS.md with their statuses")] CancellationToken ct) =>
                    ReadTasks(tasksPath),
                "read_tasks",
                "Read and return all tasks from TASKS.md with their current statuses, descriptions, and metadata."),

            AIFunctionFactory.Create(
                ([Description("Maximum number of recent entries to return (default 5)")] int? maxEntries, CancellationToken ct) =>
                    ReadProgress(progressPath, maxEntries ?? 5),
                "read_progress",
                "Read recent iteration entries from PROGRESS.md. Returns the most recent entries first."),

            AIFunctionFactory.Create(
                (CancellationToken ct) => ReadConfig(configPath),
                "read_config",
                "Read the loop configuration from LOOP_CONFIG.md including validation commands, path restrictions, and limits."),

            AIFunctionFactory.Create(
                ([Description("The task number to update")] int taskNumber,
                 [Description("New status: 'done', 'failed', or 'in_progress'")] string status,
                 [Description("Error details if marking as failed")] string? errorDetails,
                 CancellationToken ct) =>
                    UpdateTaskStatus(tasksPath, taskNumber, status, errorDetails),
                "update_task_status",
                "Update a task's status marker in TASKS.md. Use 'done' for completed tasks, 'failed' with error details for failures."),

            AIFunctionFactory.Create(
                ([Description("The task description that was worked on")] string task,
                 [Description("Outcome: 'success', 'failed', 'partial', or 'timed_out'")] string status,
                 [Description("List of files that were modified, created, or deleted")] string[] changes,
                 [Description("Results of running validation commands")] string? validation,
                 [Description("Additional observations for future iterations")] string? notes,
                 CancellationToken ct) =>
                    AppendProgress(progressPath, task, status, changes, validation, notes),
                "append_progress",
                "Append a new iteration entry to PROGRESS.md documenting what was done and the outcome."),

            AIFunctionFactory.Create(
                (CancellationToken ct) => RunValidation(configPath, workingDirectory),
                "run_validation",
                "Execute the validation commands defined in LOOP_CONFIG.md and return their output."),
        ];
    }

    internal static string ReadTasks(string tasksPath)
    {
        var tasks = TaskParser.Parse(tasksPath);
        if (tasks.Count == 0)
            return "No tasks found in TASKS.md.";

        var sb = new StringBuilder();
        var objective = TaskParser.ExtractObjective(tasksPath);
        if (objective is not null)
            sb.AppendLine($"Objective: {objective}").AppendLine();

        foreach (var t in tasks)
        {
            sb.AppendLine($"Task #{t.Number}: {t.StatusMarker} {t.Title}");
            if (t.Description is not null) sb.AppendLine($"  Description: {t.Description}");
            if (t.Files is not null) sb.AppendLine($"  Files: {t.Files}");
            if (t.DependsOn is not null) sb.AppendLine($"  Depends on: {t.DependsOn}");
            if (t.Validation is not null) sb.AppendLine($"  Validation: {t.Validation}");
        }

        return sb.ToString();
    }

    internal static string ReadProgress(string progressPath, int maxEntries)
    {
        var entries = ProgressParser.Parse(progressPath);
        if (entries.Count == 0)
            return "No progress entries found. This may be the first iteration.";

        var recent = entries.OrderByDescending(e => e.Iteration).Take(maxEntries);
        var sb = new StringBuilder();
        var stats = ProgressParser.Stats(progressPath);
        sb.AppendLine($"Total iterations: {stats.Total} (successes: {stats.Successes}, failures: {stats.Failures})");
        sb.AppendLine();

        foreach (var entry in recent)
        {
            sb.AppendLine($"--- Iteration {entry.Iteration} ---");
            if (entry.Task is not null) sb.AppendLine($"Task: {entry.Task}");
            if (entry.Status is not null) sb.AppendLine($"Status: {entry.Status}");
            if (entry.Changes.Count > 0) sb.AppendLine($"Changes: {string.Join(", ", entry.Changes)}");
            if (entry.Validation is not null) sb.AppendLine($"Validation: {entry.Validation}");
            if (entry.Notes is not null) sb.AppendLine($"Notes: {entry.Notes}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    internal static string ReadConfig(string configPath)
    {
        var config = ConfigParser.Parse(configPath);
        var sb = new StringBuilder();
        sb.AppendLine($"max_iterations: {config.MaxIterations}");
        sb.AppendLine($"timeout_minutes: {config.TimeoutMinutes}");
        sb.AppendLine($"auto_review_interval: {config.AutoReviewInterval}");
        sb.AppendLine($"max_lines_per_iteration: {config.MaxLinesPerIteration}");
        sb.AppendLine($"max_files_per_iteration: {config.MaxFilesPerIteration}");

        if (config.ValidationCommands.Count > 0)
        {
            sb.AppendLine("Validation commands:");
            foreach (var cmd in config.ValidationCommands)
                sb.AppendLine($"  - {cmd}");
        }

        if (config.AllowedPaths.Count > 0)
        {
            sb.AppendLine("Allowed paths:");
            foreach (var p in config.AllowedPaths)
                sb.AppendLine($"  - {p}");
        }

        if (config.RestrictedPaths.Count > 0)
        {
            sb.AppendLine("Restricted paths:");
            foreach (var p in config.RestrictedPaths)
                sb.AppendLine($"  - {p}");
        }

        return sb.ToString();
    }

    internal static string UpdateTaskStatus(string tasksPath, int taskNumber, string status, string? errorDetails)
    {
        if (!File.Exists(tasksPath))
            return "Error: TASKS.md not found.";

        var newMarker = status.ToLowerInvariant() switch
        {
            "done" => "[x]",
            "failed" => "[!]",
            "in_progress" => "[~]",
            "pending" => "[ ]",
            _ => null,
        };

        if (newMarker is null)
            return $"Error: Invalid status '{status}'. Use 'done', 'failed', 'in_progress', or 'pending'.";

        var lines = File.ReadAllLines(tasksPath);
        var taskNum = 0;
        var updated = false;

        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith("### ")) continue;
            taskNum++;

            if (taskNum != taskNumber) continue;

            // Find the status marker line in this task's block
            for (var j = i + 1; j < lines.Length; j++)
            {
                if (lines[j].StartsWith("### ")) break;

                if (lines[j].Contains("[ ]") || lines[j].Contains("[~]") ||
                    lines[j].Contains("[x]") || lines[j].Contains("[!]") ||
                    lines[j].Contains("[STOP]"))
                {
                    lines[j] = lines[j]
                        .Replace("[ ]", newMarker)
                        .Replace("[~]", newMarker)
                        .Replace("[x]", newMarker)
                        .Replace("[!]", newMarker)
                        .Replace("[STOP]", newMarker);

                    // Append error details for failed tasks
                    if (status.Equals("failed", StringComparison.OrdinalIgnoreCase) && errorDetails is not null)
                    {
                        lines[j] = lines[j].TrimEnd() + $" — {errorDetails}";
                    }

                    updated = true;
                    break;
                }
            }

            break;
        }

        if (!updated)
            return $"Error: Could not find task #{taskNumber} or its status marker.";

        File.WriteAllLines(tasksPath, lines);
        return $"Task #{taskNumber} status updated to {newMarker}.";
    }

    internal static string AppendProgress(
        string progressPath, string task, string status,
        string[] changes, string? validation, string? notes)
    {
        var existingEntries = ProgressParser.Parse(progressPath);
        var nextIteration = existingEntries.Count > 0
            ? existingEntries.Max(e => e.Iteration) + 1
            : 1;

        var statusEmoji = status.ToLowerInvariant() switch
        {
            "success" => "✅ Success",
            "failed" => "❌ Failed",
            "partial" => "⚠️ Partial",
            "timed_out" => "⏰ Timed out",
            _ => status,
        };

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"## Iteration {nextIteration} — {DateTime.UtcNow:O}");
        sb.AppendLine();
        sb.AppendLine($"**Task:** {task}");
        sb.AppendLine($"**Status:** {statusEmoji}");
        sb.AppendLine("**Changes:**");
        foreach (var change in changes)
            sb.AppendLine($"- {change}");
        if (validation is not null)
            sb.AppendLine($"**Validation:** {validation}");
        if (notes is not null)
            sb.AppendLine($"**Notes:** {notes}");

        File.AppendAllText(progressPath, sb.ToString());

        return $"Progress entry for iteration {nextIteration} appended.";
    }

    internal static string RunValidation(string configPath, string workingDirectory)
    {
        var config = ConfigParser.Parse(configPath);

        if (config.ValidationCommands.Count == 0)
            return "No validation commands configured in LOOP_CONFIG.md.";

        var sb = new StringBuilder();
        foreach (var cmd in config.ValidationCommands)
        {
            sb.AppendLine($"$ {cmd}");
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = OperatingSystem.IsWindows() ? "cmd" : "bash",
                    ArgumentList = { OperatingSystem.IsWindows() ? "/c" : "-c", cmd },
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                using var proc = Process.Start(psi);
                if (proc is null)
                {
                    sb.AppendLine("Error: Failed to start process.");
                    continue;
                }

                var stdout = proc.StandardOutput.ReadToEnd();
                var stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit(TimeSpan.FromMinutes(2));

                if (!string.IsNullOrEmpty(stdout))
                    sb.AppendLine(stdout.TrimEnd());
                if (!string.IsNullOrEmpty(stderr))
                    sb.AppendLine($"STDERR: {stderr.TrimEnd()}");

                sb.AppendLine($"Exit code: {proc.ExitCode}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error: {ex.Message}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
