using Spectre.Console;

namespace Rwl.Services;

public static class ComponentInstaller
{
    private static readonly string[] AgentFiles =
    [
        "ralph-wiggum-loop.agent.md",
        "loop-planner.agent.md",
        "loop-reviewer.agent.md",
    ];

    private static readonly string[] SkillDirs =
    [
        "loop-runner",
        "task-state-manager",
        "convergence-detector",
        "loop-guardrails",
    ];

    public static int InstallAgents(string targetDir)
    {
        var agentsDir = Path.Combine(targetDir, ".github", "agents");
        Directory.CreateDirectory(agentsDir);
        var count = 0;

        var rwlHome = RwlHome.Resolve();

        foreach (var agent in AgentFiles)
        {
            var source = Path.Combine(rwlHome, ".github", "agents", agent);
            var dest = Path.Combine(agentsDir, agent);

            if (File.Exists(dest))
            {
                AnsiConsole.MarkupLine($"  [dim]skip[/] {agent} [dim](exists)[/]");
                continue;
            }

            if (File.Exists(source))
            {
                File.Copy(source, dest);
                count++;
            }
            else
            {
                // Try embedded resource
                var content = ResourceLoader.LoadTemplate($"agents.{agent}");
                if (content is not null)
                {
                    File.WriteAllText(dest, content);
                    count++;
                }
                else
                {
                    AnsiConsole.MarkupLine($"  [red]✗[/] {agent} [dim]— source not found[/]");
                }
            }
        }

        return count;
    }

    public static int InstallSkills(string targetDir)
    {
        var skillsDir = Path.Combine(targetDir, ".github", "skills");
        Directory.CreateDirectory(skillsDir);
        var count = 0;

        var rwlHome = RwlHome.Resolve();

        foreach (var skill in SkillDirs)
        {
            var sourceDir = Path.Combine(rwlHome, ".github", "skills", skill);
            var destDir = Path.Combine(skillsDir, skill);
            Directory.CreateDirectory(destDir);

            if (!Directory.Exists(sourceDir))
            {
                AnsiConsole.MarkupLine($"  [yellow]![/] {skill} [dim]— source dir not found[/]");
                continue;
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                if (File.Exists(destFile))
                {
                    AnsiConsole.MarkupLine($"  [dim]skip[/] {skill}/{Path.GetFileName(file)} [dim](exists)[/]");
                    continue;
                }
                File.Copy(file, destFile);
                count++;
            }
        }

        return count;
    }

    public static int InstallInstructions(string targetDir)
    {
        var count = 0;
        var rwlHome = RwlHome.Resolve();

        // copilot-instructions.md
        var githubDir = Path.Combine(targetDir, ".github");
        Directory.CreateDirectory(githubDir);

        var copilotInstr = Path.Combine(githubDir, "copilot-instructions.md");
        if (!File.Exists(copilotInstr))
        {
            var source = Path.Combine(rwlHome, ".github", "copilot-instructions.md");
            if (File.Exists(source))
            {
                File.Copy(source, copilotInstr);
                count++;
            }
        }

        // Path-specific instructions
        var instrDir = Path.Combine(githubDir, "instructions");
        Directory.CreateDirectory(instrDir);

        var instrSourceDir = Path.Combine(rwlHome, ".github", "instructions");
        if (Directory.Exists(instrSourceDir))
        {
            foreach (var file in Directory.GetFiles(instrSourceDir, "*.instructions.md"))
            {
                var dest = Path.Combine(instrDir, Path.GetFileName(file));
                if (File.Exists(dest)) continue;
                File.Copy(file, dest);
                count++;
            }
        }

        // AGENTS.md at project root
        var agentsMd = Path.Combine(targetDir, "AGENTS.md");
        if (!File.Exists(agentsMd))
        {
            var source = Path.Combine(rwlHome, "AGENTS.md");
            if (File.Exists(source))
            {
                File.Copy(source, agentsMd);
                count++;
            }
        }

        return count;
    }

    public static int InstallHooks(string targetDir)
    {
        var count = 0;
        var rwlHome = RwlHome.Resolve();

        // Check for git
        var gitDir = Path.Combine(targetDir, ".git");
        if (!Directory.Exists(gitDir))
            return 0;

        var hooksDir = Path.Combine(gitDir, "hooks");
        Directory.CreateDirectory(hooksDir);

        var sourceHooksDir = Path.Combine(rwlHome, "hooks");
        if (!Directory.Exists(sourceHooksDir))
            return 0;

        var preCommit = Path.Combine(sourceHooksDir, "pre-commit");
        if (File.Exists(preCommit))
        {
            var dest = Path.Combine(hooksDir, "pre-commit");
            if (!File.Exists(dest) || !File.ReadAllText(dest).Contains("Ralph Wiggum", StringComparison.Ordinal))
            {
                File.Copy(preCommit, dest, overwrite: true);
                // Make executable on Unix
                if (!OperatingSystem.IsWindows())
                {
                    System.Diagnostics.Process.Start("chmod", ["+x", dest])?.WaitForExit();
                }
                count++;
            }
        }

        return count;
    }
}
