using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class HealthCommand : Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]  🏥 Loop Health Check[/]");
        AnsiConsole.WriteLine();

        var convergence = Path.Combine(".github", "skills", "convergence-detector", "check-convergence.sh");
        var guardrails = Path.Combine(".github", "skills", "loop-guardrails", "check-guardrails.sh");

        if (File.Exists(convergence))
        {
            RunScript(convergence, "--mode", "all");
            AnsiConsole.WriteLine();
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]![/] Convergence detector not found");
        }

        AnsiConsole.Write(new Rule());

        if (File.Exists(guardrails))
        {
            AnsiConsole.WriteLine();
            RunScript(guardrails);
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]![/] Guardrails checker not found");
        }

        AnsiConsole.WriteLine();
        return 0;
    }

    private static void RunScript(string script, params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "bash",
                UseShellExecute = false,
            };
            psi.ArgumentList.Add(script);
            foreach (var arg in args)
                psi.ArgumentList.Add(arg);

            var proc = Process.Start(psi);
            proc?.WaitForExit();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗[/] Failed to run {Path.GetFileName(script)}: {Markup.Escape(ex.Message)}");
        }
    }
}
