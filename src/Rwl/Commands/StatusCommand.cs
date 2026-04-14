using Rwl.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class StatusCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellation)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]  🚌 Ralph Wiggum Loop — Status Dashboard[/]");
        AnsiConsole.WriteLine();

        if (!File.Exists("TASKS.md"))
        {
            AnsiConsole.MarkupLine("[yellow]![/] No TASKS.md found. Run [bold]rwl init[/] first.");
            return 1;
        }

        // ── Task Summary ──
        var (pending, inProgress, done, failed, stopped) = TaskParser.CountStatuses("TASKS.md");
        var total = pending + inProgress + done + failed + stopped;

        AnsiConsole.MarkupLine("[bold underline]Tasks[/]");
        AnsiConsole.WriteLine();

        if (total > 0)
        {
            var pct = (double)done / total;
            var chart = new BreakdownChart()
                .Width(40)
                .ShowPercentage();

            if (done > 0) chart.AddItem("Done", done, Color.Green);
            if (inProgress > 0) chart.AddItem("In Progress", inProgress, Color.Cyan1);
            if (pending > 0) chart.AddItem("Pending", pending, Color.Grey);
            if (failed > 0) chart.AddItem("Failed", failed, Color.Red);
            if (stopped > 0) chart.AddItem("Stopped", stopped, Color.Red3);

            AnsiConsole.Write(chart);
            AnsiConsole.WriteLine();
        }

        var statusTable = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("Label")
            .AddColumn("Marker")
            .AddColumn("Count");

        statusTable.AddRow("[dim]Pending[/]", "[white][ ][/]", pending.ToString());
        statusTable.AddRow("[cyan]In Progress[/]", "[cyan][~][/]", inProgress.ToString());
        statusTable.AddRow("[green]Done[/]", "[green][x][/]", done.ToString());
        statusTable.AddRow("[red]Failed[/]", "[red][!][/]", failed.ToString());
        if (stopped > 0)
            statusTable.AddRow("[red bold]STOPPED[/]", "[red][STOP][/]", stopped.ToString());

        AnsiConsole.Write(statusTable);
        AnsiConsole.WriteLine();

        // ── Task List ──
        var tasks = TaskParser.Parse("TASKS.md");
        if (tasks.Count is > 0 and <= 30)
        {
            AnsiConsole.MarkupLine("  [dim]Task List:[/]");

            foreach (var task in tasks)
            {
                var (icon, color) = task.Status switch
                {
                    Models.TaskStatus.Done => ("✓", "green"),
                    Models.TaskStatus.InProgress => ("◉", "cyan"),
                    Models.TaskStatus.Failed => ("✗", "red"),
                    Models.TaskStatus.Stopped => ("■", "red"),
                    _ => ("○", "dim"),
                };
                AnsiConsole.MarkupLine($"    [{color}]{icon}[/] {Markup.Escape(task.Title)} [dim]([{color}]{task.Status}[/])[/]");
            }
        }

        // ── Progress Info ──
        if (File.Exists("PROGRESS.md"))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold underline]Progress[/]");
            AnsiConsole.WriteLine();

            var (iterCount, successes, failures, lines) = ProgressParser.Stats("PROGRESS.md");
            AnsiConsole.MarkupLine($"  Iterations: [bold]{iterCount}[/]  ([green]{successes}✓[/] [red]{failures}✗[/])");
            AnsiConsole.MarkupLine($"  File size:  {lines} lines");

            if (lines > 500)
                AnsiConsole.MarkupLine("  [yellow]![/] PROGRESS.md is large — consider: [bold]rwl compact[/]");

            // Last iteration
            var entries = ProgressParser.Parse("PROGRESS.md");
            if (entries.Count > 0)
            {
                var last = entries[^1];
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("  [dim]Last iteration:[/]");
                if (last.Task is not null)
                    AnsiConsole.MarkupLine($"    Task: {Markup.Escape(last.Task)}");
                if (last.Status is not null)
                    AnsiConsole.MarkupLine($"    Status: {Markup.Escape(last.Status)}");
            }
        }

        // ── Config Summary ──
        if (File.Exists("LOOP_CONFIG.md"))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold underline]Config[/]");
            AnsiConsole.WriteLine();
            var config = ConfigParser.Parse("LOOP_CONFIG.md");
            AnsiConsole.MarkupLine($"  Max iterations: [bold]{config.MaxIterations}[/]  Timeout: [bold]{config.TimeoutMinutes}m[/]");
        }

        // ── Stop conditions ──
        if (File.Exists(".loop-stop"))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("  [red]■ .loop-stop file exists[/] — loop halted");
        }

        AnsiConsole.WriteLine();
        return 0;
    }
}
