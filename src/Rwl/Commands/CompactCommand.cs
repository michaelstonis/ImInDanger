using System.ComponentModel;
using System.Text;
using Rwl.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class CompactSettings : CommandSettings
{
    [CommandArgument(0, "[keep]")]
    [Description("Number of recent iterations to keep (default: 5)")]
    [DefaultValue(5)]
    public int Keep { get; init; } = 5;
}

public sealed class CompactCommand : Command<CompactSettings>
{
    public override int Execute(CommandContext context, CompactSettings settings)
    {
        if (!File.Exists("PROGRESS.md"))
        {
            AnsiConsole.MarkupLine("[yellow]![/] No PROGRESS.md found.");
            return 0;
        }

        var lines = File.ReadAllLines("PROGRESS.md");
        var (iterCount, successes, failures, lineCount) = ProgressParser.Stats("PROGRESS.md");

        AnsiConsole.MarkupLine($"[dim]PROGRESS.md: {lineCount} lines, {iterCount} iterations[/]");

        if (lineCount < 100)
        {
            AnsiConsole.MarkupLine("[green]✓[/] File is small — no compaction needed.");
            return 0;
        }

        var keep = settings.Keep;
        AnsiConsole.MarkupLine($"[dim]Keeping last {keep} iterations[/]");

        // Backup
        File.Copy("PROGRESS.md", "PROGRESS.md.bak", overwrite: true);
        AnsiConsole.MarkupLine("[green]✓[/] Backup: PROGRESS.md.bak");

        var compactedBefore = Math.Max(0, iterCount - keep);

        var sb = new StringBuilder();
        sb.AppendLine("# Progress Log");
        sb.AppendLine();
        sb.AppendLine($"> Ralph Wiggum Loop — Iteration History");
        sb.AppendLine($"> Compacted: {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"## Summary (Iterations 1–{compactedBefore})");
        sb.AppendLine();
        sb.AppendLine($"- **Total iterations:** {compactedBefore}");
        sb.AppendLine($"- **Successes:** ~{successes}");
        sb.AppendLine($"- **Failures:** ~{failures}");
        sb.AppendLine();
        sb.AppendLine("> Older iteration details compacted. Full history in PROGRESS.md.bak.");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // Keep last N iterations
        if (keep > 0 && iterCount > 0)
        {
            // Find the line where the (iterCount - keep + 1)th iteration starts
            var iterLines = new List<int>();
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("## Iteration", StringComparison.Ordinal))
                    iterLines.Add(i);
            }

            if (iterLines.Count > 0)
            {
                var startIdx = Math.Max(0, iterLines.Count - keep);
                var startLine = iterLines[startIdx];
                for (var i = startLine; i < lines.Length; i++)
                {
                    sb.AppendLine(lines[i]);
                }
            }
        }

        File.WriteAllText("PROGRESS.md", sb.ToString());

        var newLineCount = File.ReadAllLines("PROGRESS.md").Length;
        AnsiConsole.MarkupLine($"[green]✓[/] Compacted: {lineCount} → {newLineCount} lines");
        AnsiConsole.WriteLine();

        return 0;
    }
}
