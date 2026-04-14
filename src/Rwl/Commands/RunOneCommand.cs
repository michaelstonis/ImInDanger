using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class RunOneCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellation)
    {
        if (!File.Exists("TASKS.md"))
        {
            AnsiConsole.MarkupLine("[red]✗[/] No TASKS.md found.");
            return 1;
        }

        AnsiConsole.MarkupLine("[bold]Running single iteration...[/]");
        AnsiConsole.WriteLine();

        // Try copilot CLI first
        var copilotPath = FindExecutable("copilot");
        if (copilotPath is not null)
        {
            return RunProcess(copilotPath,
                "--agent=ralph-wiggum-loop",
                "--prompt", "Execute one task from TASKS.md. Read TASKS.md and PROGRESS.md, find the next pending or failed task, execute it, then update both files.");
        }

        // Try gh copilot
        var ghPath = FindExecutable("gh");
        if (ghPath is not null)
        {
            AnsiConsole.MarkupLine("[yellow]![/] copilot CLI not found — attempting with 'gh copilot'...");
            return RunProcess(ghPath, "copilot", "suggest",
                "Execute one Ralph Wiggum Loop iteration: read TASKS.md, do the next pending task, update PROGRESS.md");
        }

        AnsiConsole.MarkupLine("[red]✗[/] Neither 'copilot' nor 'gh copilot' found.");
        return 1;
    }

    private static int RunProcess(string fileName, params string[] args)
    {
        var psi = new ProcessStartInfo { FileName = fileName, UseShellExecute = false };
        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        try
        {
            var proc = Process.Start(psi);
            proc?.WaitForExit();
            return proc?.ExitCode ?? 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }

    private static string? FindExecutable(string name)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "where" : "which",
                ArgumentList = { name },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var proc = Process.Start(psi);
            var output = proc?.StandardOutput.ReadToEnd()?.Trim();
            proc?.WaitForExit();
            return proc?.ExitCode == 0 && !string.IsNullOrEmpty(output) ? output.Split('\n')[0] : null;
        }
        catch
        {
            return null;
        }
    }
}
