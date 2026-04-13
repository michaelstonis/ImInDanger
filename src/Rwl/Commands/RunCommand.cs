using System.Diagnostics;
using Rwl.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class RunCommand : Command
{
    public override int Execute(CommandContext context)
    {
        var runner = Path.Combine(".github", "skills", "loop-runner", "run-loop.sh");

        if (File.Exists(runner))
        {
            AnsiConsole.MarkupLine("[bold]Starting Ralph Wiggum Loop...[/]");
            AnsiConsole.WriteLine();

            var psi = new ProcessStartInfo
            {
                FileName = "bash",
                ArgumentList = { runner },
                UseShellExecute = false,
            };

            try
            {
                var process = Process.Start(psi);
                process?.WaitForExit();
                return process?.ExitCode ?? 1;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗[/] Failed to start loop: {Markup.Escape(ex.Message)}");
                return 1;
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]✗[/] Loop runner not found at {runner}");
            AnsiConsole.MarkupLine("  Run [bold]rwl init[/] to install components");
            return 1;
        }
    }
}
