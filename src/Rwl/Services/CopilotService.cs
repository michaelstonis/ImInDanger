using GitHub.Copilot.SDK;
using Microsoft.Extensions.AI;
using Rwl.Models;
using Spectre.Console;
using TaskStatus = Rwl.Models.TaskStatus;

namespace Rwl.Services;

/// <summary>
/// Wraps the GitHub Copilot SDK client lifecycle and provides
/// high-level methods for running loop iterations.
/// </summary>
public sealed class CopilotService : IAsyncDisposable
{
    private CopilotClient? _client;

    public async Task<bool> InitializeAsync(CancellationToken ct = default)
    {
        try
        {
            _client = new CopilotClient();
            await _client.StartAsync(ct);

            var authStatus = await _client.GetAuthStatusAsync(ct);
            if (authStatus is null)
            {
                AnsiConsole.MarkupLine("[yellow]![/] Could not verify Copilot authentication status.");
            }

            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]![/] Copilot SDK init failed: {Markup.Escape(ex.Message)}");
            _client = null;
            return false;
        }
    }

    public bool IsAvailable => _client is not null;

    /// <summary>
    /// Checks if the SDK can connect. Returns a diagnostic message.
    /// </summary>
    public async Task<(bool Ok, string Message)> HealthCheckAsync(CancellationToken ct = default)
    {
        if (_client is null)
            return (false, "Client not initialized");

        try
        {
            var ping = await _client.PingAsync(cancellationToken: ct);
            return (true, "Connected");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Run a single loop iteration using the Copilot SDK.
    /// Creates a fresh session, sends the task prompt, and streams output.
    /// </summary>
    public async Task<bool> RunIterationAsync(
        string workingDirectory,
        LoopConfig config,
        CancellationToken ct = default)
    {
        if (_client is null)
            throw new InvalidOperationException("CopilotService not initialized. Call InitializeAsync first.");

        var tasks = TaskParser.Parse(Path.Combine(workingDirectory, "TASKS.md"));
        var nextTask = tasks.FirstOrDefault(t =>
            t.Status is TaskStatus.Pending or TaskStatus.Failed);

        if (nextTask is null)
        {
            AnsiConsole.MarkupLine("[green]✓[/] All tasks are complete — nothing to do.");
            return true;
        }

        var tools = CopilotTools.CreateTools(workingDirectory);
        var hooks = GuardrailHooks.Create(workingDirectory);

        var systemPrompt = BuildSystemMessage(nextTask, tasks, workingDirectory);
        var prompt = BuildTaskPrompt(nextTask);

        await using var session = await _client.CreateSessionAsync(new SessionConfig
        {
            OnPermissionRequest = PermissionHandler.ApproveAll,
            WorkingDirectory = workingDirectory,
            Tools = tools,
            Hooks = hooks,
            SystemMessage = new SystemMessageConfig
            {
                Mode = SystemMessageMode.Append,
                Content = systemPrompt,
            },
        }, ct);

        var success = false;

        // Subscribe to streaming events for live terminal output
        using var subscription = session.On(evt =>
        {
            switch (evt)
            {
                case AssistantMessageDeltaEvent delta:
                    var content = delta.Data?.DeltaContent;
                    if (!string.IsNullOrEmpty(content))
                        AnsiConsole.Write(Markup.Escape(content));
                    break;

                case SessionTaskCompleteEvent complete:
                    success = complete.Data?.Success == true;
                    break;

                case SessionErrorEvent error:
                    AnsiConsole.MarkupLine(
                        $"\n[red]✗ Agent error:[/] {Markup.Escape(error.Data?.Message ?? "unknown")}");
                    break;
            }
        });

        var timeout = TimeSpan.FromMinutes(config.TimeoutMinutes);

        try
        {
            var result = await session.SendAndWaitAsync(
                new MessageOptions { Prompt = prompt },
                timeout,
                ct);

            AnsiConsole.WriteLine();

            if (result?.Data?.Content is { } finalContent && !string.IsNullOrWhiteSpace(finalContent))
            {
                AnsiConsole.MarkupLine("[dim]─── Agent summary ───[/]");
                AnsiConsole.Write(Markup.Escape(finalContent));
                AnsiConsole.WriteLine();
            }

            return success;
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("\n[yellow]![/] Iteration cancelled.");
            return false;
        }
        catch (TimeoutException)
        {
            AnsiConsole.MarkupLine($"\n[yellow]![/] Iteration timed out after {config.TimeoutMinutes} minutes.");
            return false;
        }
    }

    private static string BuildSystemMessage(TaskItem currentTask, List<TaskItem> allTasks, string workingDirectory)
    {
        var totalPending = allTasks.Count(t => t.Status is TaskStatus.Pending);
        var totalDone = allTasks.Count(t => t.Status is TaskStatus.Done);
        var totalFailed = allTasks.Count(t => t.Status is TaskStatus.Failed);

        return $"""
            You are executing one iteration of the Ralph Wiggum Loop — an iterative, fresh-context agent pattern.

            ## Protocol
            1. You have tools to read and update the loop state files (TASKS.md, PROGRESS.md, LOOP_CONFIG.md).
            2. Complete EXACTLY ONE task per iteration — the task assigned below.
            3. After completing the task, use `update_task_status` to mark it done (or failed with error details).
            4. Use `append_progress` to log your iteration results.
            5. Use `run_validation` to run any configured validation commands after making changes.
            6. Do NOT attempt multiple tasks. Do NOT modify tasks other than the one assigned.

            ## Current State
            - Working directory: {workingDirectory}
            - Total tasks: {allTasks.Count} (pending: {totalPending}, done: {totalDone}, failed: {totalFailed})
            - Your assigned task: #{currentTask.Number} "{currentTask.Title}"

            ## Rules
            - Never delete or weaken tests
            - Never add bulk lint suppressions or `any` types to make checks pass
            - Respect path restrictions defined in LOOP_CONFIG.md
            - If the task fails, mark it as failed with a clear error description
            """;
    }

    private static string BuildTaskPrompt(TaskItem task)
    {
        var parts = new List<string>
        {
            $"Execute task #{task.Number}: {task.Title}",
        };

        if (task.Description is not null)
            parts.Add($"Description: {task.Description}");
        if (task.Files is not null)
            parts.Add($"Files to modify: {task.Files}");
        if (task.Validation is not null)
            parts.Add($"Validation: {task.Validation}");
        if (task.DependsOn is not null)
            parts.Add($"Depends on: {task.DependsOn}");

        parts.Add("Start by calling read_tasks and read_config to understand the full context, then execute the task.");

        return string.Join("\n", parts);
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            try
            {
                await _client.StopAsync();
            }
            catch
            {
                // Best-effort shutdown
            }

            await _client.DisposeAsync();
            _client = null;
        }
    }
}
