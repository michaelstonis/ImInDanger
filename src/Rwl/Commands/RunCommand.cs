using System.Diagnostics;
using Rwl.Models;
using Rwl.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using TaskStatus = Rwl.Models.TaskStatus;

namespace Rwl.Commands;

public sealed class RunCommand : AsyncCommand
{
    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellation)
    {
        if (!File.Exists("TASKS.md"))
        {
            AnsiConsole.MarkupLine("[red]✗[/] No TASKS.md found. Run [bold]rwl init[/] first.");
            return 1;
        }

        AnsiConsole.MarkupLine("[bold]Starting Ralph Wiggum Loop...[/]");
        AnsiConsole.WriteLine();

        // Try SDK-based loop first
        await using var copilot = new CopilotService();
        if (await copilot.InitializeAsync(cancellation))
        {
            AnsiConsole.MarkupLine("[green]✓[/] Copilot SDK connected — running native loop");
            AnsiConsole.WriteLine();
            return await RunSdkLoop(copilot, cancellation);
        }

        // Fall back to shell-based loop runner
        AnsiConsole.MarkupLine("[dim]Falling back to shell-based loop runner...[/]");
        return RunShellLoop();
    }

    private static async Task<int> RunSdkLoop(CopilotService copilot, CancellationToken ct)
    {
        var workingDir = Directory.GetCurrentDirectory();
        var config = ConfigParser.Parse("LOOP_CONFIG.md");
        var iteration = 0;

        while (!ct.IsCancellationRequested)
        {
            iteration++;

            // Check stop conditions
            if (File.Exists(".loop-stop"))
            {
                AnsiConsole.MarkupLine("[yellow]■[/] Stop flag detected (.loop-stop). Halting.");
                break;
            }

            if (iteration > config.MaxIterations)
            {
                AnsiConsole.MarkupLine($"[yellow]■[/] Reached max iterations ({config.MaxIterations}). Stopping.");
                break;
            }

            // Check if there are remaining tasks
            var tasks = TaskParser.Parse("TASKS.md");
            var pending = tasks.Where(t => t.Status is TaskStatus.Pending or TaskStatus.Failed).ToList();

            if (pending.Count == 0)
            {
                AnsiConsole.MarkupLine("[green bold]✓ All tasks complete![/]");
                ShowSummary(tasks);
                return 0;
            }

            // Check for STOP marker
            if (tasks.Any(t => t.Status is TaskStatus.Stopped))
            {
                AnsiConsole.MarkupLine("[yellow]■[/] STOP marker found in TASKS.md. Halting.");
                break;
            }

            AnsiConsole.Write(new Rule($"[bold]Iteration {iteration}[/]"));
            AnsiConsole.MarkupLine($"  [dim]Remaining: {pending.Count} task(s)[/]");
            AnsiConsole.WriteLine();

            // Run one iteration with a fresh session
            var success = await copilot.RunIterationAsync(workingDir, config, ct);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(success
                ? $"  [green]✓[/] Iteration {iteration} completed successfully"
                : $"  [yellow]![/] Iteration {iteration} completed with issues");
            AnsiConsole.WriteLine();

            // Re-read config in case it was updated
            config = ConfigParser.Parse("LOOP_CONFIG.md");
        }

        // Final summary
        var finalTasks = TaskParser.Parse("TASKS.md");
        ShowSummary(finalTasks);

        var allDone = finalTasks.All(t => t.Status is TaskStatus.Done);
        return allDone ? 0 : 1;
    }

    private static void ShowSummary(List<TaskItem> tasks)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Loop Summary[/]"));
        var counts = (
            Done: tasks.Count(t => t.Status is TaskStatus.Done),
            Failed: tasks.Count(t => t.Status is TaskStatus.Failed),
            Pending: tasks.Count(t => t.Status is TaskStatus.Pending),
            InProgress: tasks.Count(t => t.Status is TaskStatus.InProgress)
        );

        AnsiConsole.MarkupLine($"  [green]Done:[/] {counts.Done}  [red]Failed:[/] {counts.Failed}  [dim]Pending:[/] {counts.Pending}  [yellow]In Progress:[/] {counts.InProgress}");
        AnsiConsole.WriteLine();
    }

    private static int RunShellLoop()
    {
        var runner = Path.Combine(".github", "skills", "loop-runner", "run-loop.sh");

        if (!File.Exists(runner))
        {
            AnsiConsole.MarkupLine($"[red]✗[/] Loop runner not found at {runner}");
            AnsiConsole.MarkupLine("  Run [bold]rwl init[/] to install components");
            return 1;
        }

        var psi = new ProcessStartInfo
        {
            FileName = "bash",
            ArgumentList = { runner },
            UseShellExecute = false,
        };

        try
        {
            var process = Process.Start(psi);
            process?.WaitForExit();
            return process?.ExitCode ?? 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗[/] Failed to start loop: {Markup.Escape(ex.Message)}");
            return 1;
        }
    }
}
