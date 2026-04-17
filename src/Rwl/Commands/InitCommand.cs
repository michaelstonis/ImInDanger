using System.ComponentModel;
using Rwl.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class InitSettings : CommandSettings
{
    [CommandOption("--no-agents")]
    [Description("Skip installing agent profiles")]
    public bool NoAgents { get; init; }

    [CommandOption("--no-skills")]
    [Description("Skip installing skills")]
    public bool NoSkills { get; init; }

    [CommandOption("--no-hooks")]
    [Description("Skip installing git hooks")]
    public bool NoHooks { get; init; }

    [CommandOption("--no-instructions")]
    [Description("Skip installing instructions")]
    public bool NoInstructions { get; init; }
}

public sealed class InitCommand : Command<InitSettings>
{
    protected override int Execute(CommandContext context, InitSettings settings, CancellationToken cancellation)
    {
        Banner.Show();
        AnsiConsole.MarkupLine("[bold]Initialize Ralph Wiggum Loop[/]");
        AnsiConsole.WriteLine();

        var targetDir = Directory.GetCurrentDirectory();
        var filesCreated = 0;
        var agentsInstalled = -1; // -1 = skipped; >=0 = attempted
        var skillsInstalled = -1;

        // ── Install components ──
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Installing components...", ctx =>
            {
                if (!settings.NoAgents)
                {
                    ctx.Status("Installing agents...");
                    agentsInstalled = ComponentInstaller.InstallAgents(targetDir);
                    filesCreated += agentsInstalled;
                    AnsiConsole.MarkupLine($"  [green]✓[/] Agents: {agentsInstalled} installed");
                }

                if (!settings.NoSkills)
                {
                    ctx.Status("Installing skills...");
                    skillsInstalled = ComponentInstaller.InstallSkills(targetDir);
                    filesCreated += skillsInstalled;
                    AnsiConsole.MarkupLine($"  [green]✓[/] Skills: {skillsInstalled} installed");
                }

                if (!settings.NoInstructions)
                {
                    ctx.Status("Installing instructions...");
                    var count = ComponentInstaller.InstallInstructions(targetDir);
                    filesCreated += count;
                    AnsiConsole.MarkupLine($"  [green]✓[/] Instructions: {count} installed");
                }

                if (!settings.NoHooks)
                {
                    ctx.Status("Installing hooks...");
                    var count = ComponentInstaller.InstallHooks(targetDir);
                    filesCreated += count;
                    AnsiConsole.MarkupLine($"  [green]✓[/] Hooks: {count} installed");
                }
            });

        AnsiConsole.WriteLine();

        // ── Validate component installation ──
        var installFailed = false;
        if (agentsInstalled == 0)
        {
            var agentsDir = Path.Combine(targetDir, ".github", "agents");
            if (!Directory.GetFiles(agentsDir, "*.agent.md").Any())
            {
                AnsiConsole.MarkupLine("[red bold]✗[/] Agent installation failed — no agent files found.");
                AnsiConsole.MarkupLine("  [dim]Set RWL_HOME to the rwl repository root or run [bold]rwl doctor[/] for diagnostics.[/]");
                installFailed = true;
            }
        }
        if (skillsInstalled == 0)
        {
            var skillsDir = Path.Combine(targetDir, ".github", "skills");
            if (!Directory.GetDirectories(skillsDir).Any())
            {
                AnsiConsole.MarkupLine("[red bold]✗[/] Skill installation failed — no skill directories found.");
                AnsiConsole.MarkupLine("  [dim]Set RWL_HOME to the rwl repository root or run [bold]rwl doctor[/] for diagnostics.[/]");
                installFailed = true;
            }
        }
        if (installFailed)
        {
            AnsiConsole.WriteLine();
            return 1;
        }

        // ── State files ──
        AnsiConsole.MarkupLine("[bold]State Files[/]");
        AnsiConsole.WriteLine();

        if (!File.Exists("TASKS.md"))
        {
            var createTasks = AnsiConsole.Confirm("Create [yellow]TASKS.md[/]?", defaultValue: true);
            if (createTasks)
            {
                var project = ProjectDetector.Detect(targetDir);
                var objective = AnsiConsole.Ask<string>("  [dim]Objective:[/] ");
                CreateTasksFile(objective, project);
                filesCreated++;
                AnsiConsole.MarkupLine("  [green]✓[/] TASKS.md created");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("  [dim]skip[/] TASKS.md [dim](exists)[/]");
        }

        if (!File.Exists("PROGRESS.md"))
        {
            File.WriteAllText("PROGRESS.md", """
                # Progress Log

                > Ralph Wiggum Loop — Iteration History

                ---

                """.Replace("                ", ""));
            filesCreated++;
            AnsiConsole.MarkupLine("  [green]✓[/] PROGRESS.md created");
        }
        else
        {
            AnsiConsole.MarkupLine("  [dim]skip[/] PROGRESS.md [dim](exists)[/]");
        }

        if (!File.Exists("LOOP_CONFIG.md"))
        {
            var createConfig = AnsiConsole.Confirm("Configure [yellow]LOOP_CONFIG.md[/]?", defaultValue: true);
            if (createConfig)
            {
                CreateConfigFile(targetDir);
                filesCreated++;
                AnsiConsole.MarkupLine("  [green]✓[/] LOOP_CONFIG.md created");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("  [dim]skip[/] LOOP_CONFIG.md [dim](exists)[/]");
        }

        // ── Summary ──
        AnsiConsole.WriteLine();
        var panel = new Panel(
            $"""
            [green]{filesCreated} files created[/]

            [bold]Next steps:[/]
            [cyan]1.[/] Define tasks: edit TASKS.md or run [bold]rwl plan[/]
            [cyan]2.[/] Review config: check LOOP_CONFIG.md
            [cyan]3.[/] Verify setup: [bold]rwl doctor[/]
            [cyan]4.[/] Start the loop: [bold]rwl run[/]
            """)
        {
            Header = new PanelHeader("[green bold] Setup Complete [/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1),
        };
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        return 0;
    }

    private static void CreateTasksFile(string objective, Models.ProjectInfo project)
    {
        var sourceInfo = project.SourceDirs.Count > 0
            ? string.Join(", ", project.SourceDirs.Select(d => $"`{d}/`"))
            : "_(auto-detected)_";

        File.WriteAllText("TASKS.md", $$"""
            # Tasks

            ## Objective
            {{objective}}

            ## Success Criteria
            All tasks complete, tests pass, build succeeds.

            ## Project Info
            - **Type:** {{project.Type}}
            - **Source dirs:** {{sourceInfo}}

            ## Task List

            <!-- Add tasks using: rwl plan, rwl add-task, or edit this file directly -->
            <!-- Status markers: [ ] pending, [~] in progress, [x] done, [!] failed, [STOP] halt -->
            """.Replace("            ", ""));
    }

    private static void CreateConfigFile(string targetDir)
    {
        var project = ProjectDetector.Detect(targetDir);

        var maxIter = AnsiConsole.Ask("  Max iterations?", 20);
        var timeout = AnsiConsole.Ask("  Timeout (minutes)?", 10);
        var validation = AnsiConsole.Ask("  Validation command?", project.TestCommand ?? "make test");

        var sourcePaths = project.SourceDirs.Count > 0
            ? string.Join("\n", project.SourceDirs.Select(d => $"- `{d}/`"))
            : "- `src/`";

        File.WriteAllText("LOOP_CONFIG.md", $$"""
            # Loop Configuration

            ## Limits
            - max_iterations: {{maxIter}}
            - timeout_minutes: {{timeout}}
            - auto_review_interval: 5

            ## Validation Commands
            - `{{validation}}`

            ## Allowed Paths
            {{sourcePaths}}

            ## Restricted Paths
            - `.git/`
            - `node_modules/`
            - `.env`

            ## Guardrails
            - max_lines_per_iteration: 200
            - max_files_per_iteration: 10

            <!-- Generated by: rwl init -->
            """.Replace("            ", ""));
    }
}
