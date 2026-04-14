using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class ResetCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellation)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Reset Loop State[/]");
        AnsiConsole.WriteLine();

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to reset?")
                .AddChoices(
                    "Progress only (reset task statuses, clear PROGRESS.md)",
                    "Full reset (remove all state files)",
                    "Cancel"));

        switch (choice)
        {
            case var c when c.StartsWith("Progress", StringComparison.Ordinal):
                // Clear PROGRESS.md
                File.WriteAllText("PROGRESS.md", """
                    # Progress Log

                    > Ralph Wiggum Loop — Iteration History

                    ---

                    """.Replace("                    ", ""));

                // Remove stop flags and logs
                if (File.Exists(".loop-stop")) File.Delete(".loop-stop");
                if (Directory.Exists(".loop-logs")) Directory.Delete(".loop-logs", recursive: true);

                // Reset task statuses
                if (File.Exists("TASKS.md"))
                {
                    var content = File.ReadAllText("TASKS.md");
                    content = content
                        .Replace("[x]", "[ ]")
                        .Replace("[~]", "[ ]")
                        .Replace("[!]", "[ ]");
                    File.WriteAllText("TASKS.md", content);
                    AnsiConsole.MarkupLine("[green]✓[/] Task statuses reset to [ ]");
                }

                AnsiConsole.MarkupLine("[green]✓[/] Progress cleared, tasks kept.");
                break;

            case var c when c.StartsWith("Full", StringComparison.Ordinal):
                if (AnsiConsole.Confirm("Delete TASKS.md, PROGRESS.md, LOOP_CONFIG.md?", defaultValue: false))
                {
                    foreach (var f in new[] { "TASKS.md", "PROGRESS.md", "LOOP_CONFIG.md", ".loop-stop" })
                    {
                        if (File.Exists(f)) File.Delete(f);
                    }
                    if (Directory.Exists(".loop-logs")) Directory.Delete(".loop-logs", recursive: true);

                    AnsiConsole.MarkupLine("[green]✓[/] All state files removed.");
                    AnsiConsole.MarkupLine("[dim]  Run 'rwl init' to start fresh.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[dim]Cancelled.[/]");
                }
                break;

            default:
                AnsiConsole.MarkupLine("[dim]Cancelled.[/]");
                break;
        }

        AnsiConsole.WriteLine();
        return 0;
    }
}
