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
                // Fall back to embedded resource (available in distributed binary)
                var content = ResourceLoader.LoadTemplate($"agents.{agent}");
                if (content is not null)
                {
                    File.WriteAllText(dest, content);
                    count++;
                }
                else
                {
                    AnsiConsole.MarkupLine($"  [red]✗[/] {agent} [dim]— not found[/]");
                    AnsiConsole.MarkupLine($"    [dim]Set RWL_HOME to the rwl repository root and retry.[/]");
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

            if (Directory.Exists(sourceDir))
            {
                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    var filename = Path.GetFileName(file);
                    var destFile = Path.Combine(destDir, filename);
                    if (File.Exists(destFile))
                    {
                        AnsiConsole.MarkupLine($"  [dim]skip[/] {skill}/{filename} [dim](exists)[/]");
                        continue;
                    }
                    File.Copy(file, destFile);
                    MakeExecutableIfScript(destFile);
                    count++;
                }
            }
            else
            {
                // Fall back to embedded resources (available in distributed binary)
                var prefix = $"skills.{skill}.";
                var embedded = ResourceLoader.ListTemplates()
                    .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (embedded.Count > 0)
                {
                    foreach (var resourceName in embedded)
                    {
                        var filename = resourceName[prefix.Length..];
                        var destFile = Path.Combine(destDir, filename);
                        if (File.Exists(destFile))
                        {
                            AnsiConsole.MarkupLine($"  [dim]skip[/] {skill}/{filename} [dim](exists)[/]");
                            continue;
                        }
                        var content = ResourceLoader.LoadTemplate(resourceName);
                        if (content is not null)
                        {
                            File.WriteAllText(destFile, content);
                            MakeExecutableIfScript(destFile);
                            count++;
                        }
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"  [red]✗[/] {skill} [dim]— not found[/]");
                    AnsiConsole.MarkupLine($"    [dim]Set RWL_HOME to the rwl repository root and retry.[/]");
                }
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
            else
            {
                var content = ResourceLoader.LoadTemplate("copilot-instructions.md");
                if (content is not null)
                {
                    File.WriteAllText(copilotInstr, content);
                    count++;
                }
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
        else
        {
            // Fall back to embedded resources
            foreach (var resourceName in ResourceLoader.ListTemplates()
                         .Where(n => n.StartsWith("instructions.", StringComparison.OrdinalIgnoreCase)
                                     && n.EndsWith(".instructions.md", StringComparison.OrdinalIgnoreCase)))
            {
                var filename = resourceName["instructions.".Length..];
                var dest = Path.Combine(instrDir, filename);
                if (File.Exists(dest)) continue;
                var content = ResourceLoader.LoadTemplate(resourceName);
                if (content is not null)
                {
                    File.WriteAllText(dest, content);
                    count++;
                }
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
            else
            {
                var content = ResourceLoader.LoadTemplate("AGENTS.md");
                if (content is not null)
                {
                    File.WriteAllText(agentsMd, content);
                    count++;
                }
            }
        }

        return count;
    }

    public static int InstallHooks(string targetDir)
    {
        var count = 0;
        var rwlHome = RwlHome.Resolve();

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
                if (!OperatingSystem.IsWindows())
                {
                    System.Diagnostics.Process.Start("chmod", ["+x", dest])?.WaitForExit();
                }
                count++;
            }
        }

        return count;
    }

    private static void MakeExecutableIfScript(string path)
    {
        if (!OperatingSystem.IsWindows() && path.EndsWith(".sh", StringComparison.OrdinalIgnoreCase))
        {
            System.Diagnostics.Process.Start("chmod", ["+x", path])?.WaitForExit();
        }
    }
}
