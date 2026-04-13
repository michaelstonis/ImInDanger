using Spectre.Console;

namespace Rwl.Commands;

public static class Banner
{
    public static void Show()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new FigletText("RWL")
                .LeftJustified()
                .Color(Color.Yellow));
        AnsiConsole.MarkupLine("[dim]Ralph Wiggum Loop — Iterative Fresh-Context Agent Loop[/]");
        AnsiConsole.WriteLine();
    }
}
