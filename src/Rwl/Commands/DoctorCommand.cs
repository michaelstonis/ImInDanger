using Rwl.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class DoctorCommand : Command
{
    protected override int Execute(CommandContext context, CancellationToken cancellation)
    {
        Banner.Show();
        AnsiConsole.MarkupLine("[bold]Doctor — Setup Verification[/]");
        AnsiConsole.WriteLine();

        var issues = 0;
        var warnings = 0;

        // ── Git ──
        Section("Git");
        if (Directory.Exists(".git"))
        {
            AnsiConsole.MarkupLine("  [green]✓[/] Git repository detected");
        }
        else
        {
            AnsiConsole.MarkupLine("  [yellow]![/] Not a git repository");
            warnings++;
        }

        // ── Agents ──
        Section("Agents");
        foreach (var agent in new[] { "ralph-wiggum-loop", "loop-planner", "loop-reviewer" })
        {
            var path = Path.Combine(".github", "agents", $"{agent}.agent.md");
            if (File.Exists(path))
            {
                AnsiConsole.MarkupLine($"  [green]✓[/] {agent}");
            }
            else
            {
                AnsiConsole.MarkupLine($"  [red]✗[/] {agent} — not found");
                issues++;
            }
        }

        // ── Skills ──
        Section("Skills");
        foreach (var skill in new[] { "loop-runner", "task-state-manager", "convergence-detector", "loop-guardrails" })
        {
            var skillFile = Path.Combine(".github", "skills", skill, "SKILL.md");
            if (File.Exists(skillFile))
            {
                AnsiConsole.MarkupLine($"  [green]✓[/] {skill}");

                // Check scripts are executable (Unix only)
                if (!OperatingSystem.IsWindows())
                {
                    var skillDir = Path.Combine(".github", "skills", skill);
                    foreach (var script in Directory.GetFiles(skillDir, "*.sh"))
                    {
                        var info = new FileInfo(script);
                        var isExec = (File.GetUnixFileMode(script) & UnixFileMode.UserExecute) != 0;
                        if (isExec)
                        {
                            AnsiConsole.MarkupLine($"    [dim]{info.Name} (executable ✓)[/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"    [yellow]![/] {info.Name} not executable — run: chmod +x {script}");
                            warnings++;
                        }
                    }
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"  [red]✗[/] {skill} — not found");
                issues++;
            }
        }

        // ── Instructions ──
        Section("Instructions");
        foreach (var instr in new[] { ".github/copilot-instructions.md", "AGENTS.md" })
        {
            if (File.Exists(instr))
                AnsiConsole.MarkupLine($"  [green]✓[/] {instr}");
            else
            {
                AnsiConsole.MarkupLine($"  [yellow]![/] {instr} — not found [dim](optional)[/]");
                warnings++;
            }
        }

        foreach (var name in new[] { "loop-tasks.instructions.md", "loop-progress.instructions.md" })
        {
            var path = Path.Combine(".github", "instructions", name);
            if (File.Exists(path))
                AnsiConsole.MarkupLine($"  [green]✓[/] {name}");
            else
            {
                AnsiConsole.MarkupLine($"  [yellow]![/] {name} — not found");
                warnings++;
            }
        }

        // ── State Files ──
        Section("State Files");
        foreach (var sf in new[] { "TASKS.md", "PROGRESS.md", "LOOP_CONFIG.md" })
        {
            if (File.Exists(sf))
            {
                var lineCount = File.ReadAllLines(sf).Length;
                AnsiConsole.MarkupLine($"  [green]✓[/] {sf} ({lineCount} lines)");
            }
            else if (sf == "TASKS.md")
            {
                AnsiConsole.MarkupLine($"  [red]✗[/] {sf} — required before running");
                issues++;
            }
            else
            {
                AnsiConsole.MarkupLine($"  [yellow]![/] {sf} — not found");
                warnings++;
            }
        }

        // Check TASKS.md has tasks
        if (File.Exists("TASKS.md"))
        {
            var tasks = TaskParser.Parse("TASKS.md");
            if (tasks.Count == 0)
            {
                AnsiConsole.MarkupLine("  [yellow]![/] TASKS.md has no tasks defined yet");
                warnings++;
            }
            else
            {
                AnsiConsole.MarkupLine($"  [dim]  {tasks.Count} task(s) defined[/]");
            }
        }

        // ── Config Validation ──
        if (File.Exists("LOOP_CONFIG.md"))
        {
            Section("Configuration");
            var config = ConfigParser.Parse("LOOP_CONFIG.md");

            if (config.ValidationCommands.Count > 0)
                AnsiConsole.MarkupLine("  [green]✓[/] Validation commands configured");
            else
            {
                AnsiConsole.MarkupLine("  [yellow]![/] No validation commands in LOOP_CONFIG.md");
                warnings++;
            }

            AnsiConsole.MarkupLine($"  [green]✓[/] Max iterations: {config.MaxIterations}");
        }

        // ── Hooks ──
        Section("Git Hooks");
        if (Directory.Exists(".git"))
        {
            var preCommit = Path.Combine(".git", "hooks", "pre-commit");
            if (File.Exists(preCommit) && File.ReadAllText(preCommit).Contains("Ralph Wiggum", StringComparison.Ordinal))
                AnsiConsole.MarkupLine("  [green]✓[/] Pre-commit hook installed");
            else
                AnsiConsole.MarkupLine("  [dim]  Pre-commit hook not installed (optional)[/]");
        }

        // ── Stale Artifacts ──
        Section("Stale Artifacts");
        if (File.Exists(".loop-stop"))
        {
            AnsiConsole.MarkupLine("  [yellow]![/] Found .loop-stop — remove before starting: rm .loop-stop");
            warnings++;
        }
        else
        {
            AnsiConsole.MarkupLine("  [green]✓[/] No stale stop flags");
        }

        // ── Summary ──
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule());
        if (issues == 0 && warnings == 0)
        {
            AnsiConsole.MarkupLine("[green bold]All checks passed![/] Ready for the loop.");
        }
        else if (issues == 0)
        {
            AnsiConsole.MarkupLine($"[yellow bold]{warnings} warning(s)[/] — loop can run, review items above.");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red bold]{issues} issue(s)[/], [yellow]{warnings} warning(s)[/]");
        }
        AnsiConsole.WriteLine();

        return issues > 0 ? 1 : 0;
    }

    private static void Section(string title)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold underline]{title}[/]");
    }
}
