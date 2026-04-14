using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class AddTaskSettings : CommandSettings
{
    [CommandArgument(0, "[title]")]
    [Description("Task title (will prompt if not provided)")]
    public string? Title { get; init; }
}

public sealed class AddTaskCommand : Command<AddTaskSettings>
{
    protected override int Execute(CommandContext context, AddTaskSettings settings, CancellationToken cancellation)
    {
        if (!File.Exists("TASKS.md"))
        {
            AnsiConsole.MarkupLine("[red]✗[/] TASKS.md not found. Run [bold]rwl init[/] or [bold]rwl plan[/] first.");
            return 1;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Add Task[/]");
        AnsiConsole.WriteLine();

        var content = File.ReadAllText("TASKS.md");
        var existingCount = System.Text.RegularExpressions.Regex.Matches(content, @"^###\s+", System.Text.RegularExpressions.RegexOptions.Multiline).Count;
        var nextNum = existingCount + 1;

        var title = settings.Title ?? AnsiConsole.Ask<string>("  Task title:");
        if (string.IsNullOrWhiteSpace(title))
        {
            AnsiConsole.MarkupLine("[red]✗[/] Title required.");
            return 1;
        }

        var files = AnsiConsole.Ask("  Files to modify [dim](optional)[/]:", "");
        var desc = AnsiConsole.Ask("  Description [dim](optional)[/]:", "");
        string? deps = null;
        if (existingCount > 0)
            deps = AnsiConsole.Ask("  Depends on tasks [dim](e.g. '1,2')[/]:", "");
        var validation = AnsiConsole.Ask("  Validation command [dim](optional)[/]:", "");

        // Build task block
        using var sw = new StringWriter();
        sw.WriteLine();
        sw.WriteLine($"### {nextNum}. {title}");
        sw.WriteLine("- **Status:** [ ]");
        if (!string.IsNullOrWhiteSpace(deps)) sw.WriteLine($"- **Depends on:** Task {deps}");
        if (!string.IsNullOrWhiteSpace(files)) sw.WriteLine($"- **Files:** `{files}`");
        if (!string.IsNullOrWhiteSpace(desc)) sw.WriteLine($"- **Description:** {desc}");
        if (!string.IsNullOrWhiteSpace(validation)) sw.WriteLine($"- **Validation:** `{validation}`");

        var taskBlock = sw.ToString();

        // Insert before trailing comment block, or append
        var lines = File.ReadAllLines("TASKS.md").ToList();
        var commentIdx = -1;
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].TrimStart().StartsWith("<!--", StringComparison.Ordinal))
            {
                commentIdx = i;
                break;
            }
        }

        if (commentIdx >= 0)
        {
            lines.Insert(commentIdx, taskBlock.TrimEnd());
            lines.Insert(commentIdx, "");
        }
        else
        {
            lines.Add(taskBlock.TrimEnd());
        }

        File.WriteAllLines("TASKS.md", lines);

        AnsiConsole.MarkupLine($"[green]✓[/] Task {nextNum} added: {Markup.Escape(title)}");
        AnsiConsole.WriteLine();

        return 0;
    }
}
