using Rwl.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class PlanCommand : Command
{
    public override int Execute(CommandContext context)
    {
        Banner.Show();
        AnsiConsole.MarkupLine("[bold]Task Planner[/]");
        AnsiConsole.MarkupLine("[dim]Create a structured task list for the Ralph Wiggum Loop.[/]");
        AnsiConsole.WriteLine();

        // ── Objective ──
        AnsiConsole.MarkupLine("[bold underline]Objective[/]");
        var objective = AnsiConsole.Ask<string>("  What is the overall goal of this loop run?");
        if (string.IsNullOrWhiteSpace(objective))
        {
            AnsiConsole.MarkupLine("[red]✗[/] Objective is required.");
            return 1;
        }

        // ── Success Criteria ──
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold underline]Success Criteria[/]");
        var criteria = AnsiConsole.Ask("  How will you know it's complete?", "All tests pass, build succeeds");

        // ── Default Validation ──
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold underline]Default Validation Command[/]");
        var project = ProjectDetector.Detect();
        var validation = AnsiConsole.Ask("  Validation command?", project.TestCommand ?? "make test");

        // ── Tasks ──
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold underline]Tasks[/]");
        AnsiConsole.MarkupLine("[dim]  Add tasks one at a time. Leave title empty to finish.[/]");
        AnsiConsole.WriteLine();

        var tasks = new List<(string Title, string? Files, string? Desc, string? Deps, string Validation)>();
        var taskNum = 0;

        while (true)
        {
            taskNum++;
            AnsiConsole.MarkupLine($"  [cyan]── Task {taskNum} ──[/]");

            var title = AnsiConsole.Ask("  Title ([dim]empty to finish[/]):", "");
            if (string.IsNullOrWhiteSpace(title))
                break;

            var files = AskOptional("  Files to modify?");
            var desc = AskOptional("  Description?");
            string? deps = null;
            if (taskNum > 1)
                deps = AskOptional("  Depends on tasks (e.g. '1,2')?");
            var taskVal = AnsiConsole.Ask("  Validation?", validation);

            tasks.Add((title, files, desc, deps, taskVal));
            AnsiConsole.WriteLine();
        }

        if (tasks.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]![/] No tasks defined.");
            return 1;
        }

        // ── Final smoke test ──
        AnsiConsole.WriteLine();
        if (AnsiConsole.Confirm("Add a final smoke test task?", defaultValue: true))
        {
            tasks.Add(("Final Smoke Test", null, "Run full validation suite to verify overall integration", "All previous tasks", validation));
        }

        // ── Generate TASKS.md ──
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Generating TASKS.md[/]");

        var content = GenerateTasksContent(objective, criteria, tasks);

        if (File.Exists("TASKS.md"))
        {
            var overwrite = AnsiConsole.Confirm("TASKS.md exists. Overwrite?", defaultValue: false);
            if (overwrite)
            {
                File.WriteAllText("TASKS.md", content);
                AnsiConsole.MarkupLine($"[green]✓[/] TASKS.md updated with {tasks.Count} tasks");
            }
            else
            {
                File.WriteAllText("TASKS.generated.md", content);
                AnsiConsole.MarkupLine("[green]✓[/] Saved to TASKS.generated.md (rename when ready)");
            }
        }
        else
        {
            File.WriteAllText("TASKS.md", content);
            AnsiConsole.MarkupLine($"[green]✓[/] TASKS.md created with {tasks.Count} tasks");
        }

        // Create PROGRESS.md if missing
        if (!File.Exists("PROGRESS.md"))
        {
            File.WriteAllText("PROGRESS.md", """
                # Progress Log

                > Ralph Wiggum Loop — Iteration History

                ---

                """.Replace("                ", ""));
            AnsiConsole.MarkupLine("[green]✓[/] PROGRESS.md created");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  [bold]Ready![/] Run [cyan]rwl doctor[/] to verify, then [cyan]rwl run[/] to start.");
        AnsiConsole.WriteLine();

        return 0;
    }

    private static string? AskOptional(string prompt)
    {
        var val = AnsiConsole.Ask(prompt, "");
        return string.IsNullOrWhiteSpace(val) ? null : val;
    }

    private static string GenerateTasksContent(
        string objective,
        string criteria,
        List<(string Title, string? Files, string? Desc, string? Deps, string Validation)> tasks)
    {
        using var sw = new StringWriter();
        sw.WriteLine("# Tasks");
        sw.WriteLine();
        sw.WriteLine("## Objective");
        sw.WriteLine(objective);
        sw.WriteLine();
        sw.WriteLine("## Success Criteria");
        sw.WriteLine(criteria);
        sw.WriteLine();
        sw.WriteLine("## Task List");

        for (var i = 0; i < tasks.Count; i++)
        {
            var (title, files, desc, deps, val) = tasks[i];
            var num = i + 1;
            sw.WriteLine();
            sw.WriteLine($"### {num}. {title}");
            sw.WriteLine("- **Status:** [ ]");
            if (deps is not null) sw.WriteLine($"- **Depends on:** Task {deps}");
            if (files is not null) sw.WriteLine($"- **Files:** `{files}`");
            if (desc is not null) sw.WriteLine($"- **Description:** {desc}");
            sw.WriteLine($"- **Validation:** `{val}`");
        }

        sw.WriteLine();
        sw.WriteLine("<!--");
        sw.WriteLine("Status markers:");
        sw.WriteLine("  [ ]     pending — not yet started");
        sw.WriteLine("  [~]     in progress — being worked on");
        sw.WriteLine("  [x]     done — completed successfully");
        sw.WriteLine("  [!]     failed — include error details below");
        sw.WriteLine("  [STOP]  emergency stop — halts the loop");
        sw.WriteLine("-->");

        return sw.ToString();
    }
}
