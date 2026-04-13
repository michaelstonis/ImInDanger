using Spectre.Console;
using Spectre.Console.Cli;

namespace Rwl.Commands;

public sealed class StopCommand : Command
{
    public override int Execute(CommandContext context)
    {
        File.Create(".loop-stop").Dispose();
        AnsiConsole.MarkupLine("[green]✓[/] Stop flag created (.loop-stop)");
        AnsiConsole.MarkupLine("[dim]  Loop will halt at end of current iteration.[/]");
        AnsiConsole.MarkupLine("[dim]  Remove with: rm .loop-stop[/]");
        return 0;
    }
}
