using Spectre.Console;

namespace Rwl.Commands;

public static class Banner
{
    // ASCII Ralph Wiggum, columns 0-29 art | columns 32+ quote
    private static readonly (string Art, string Quote)[] Lines =
    [
        (@"          .-------.         ", ""),
        (@"         /  .   .  \        ", ""),
        (@"        | (o)   (o) |       ", "  [italic dim]\"My context window[/]"),
        (@"        |     _     |       ", "  [italic dim]smells like[/]"),
        (@"        |   (___)   |       ", "  [italic dim]fresh tasks.\"[/]"),
        (@"         \  '---'  /        ", ""),
        (@"          '-------'         ", ""),
        (@"               |            ", ""),
        (@"          _____|_____       ", ""),
        (@"         /   R W L   \      ", ""),
        (@"        |    v 2.0    |     ", ""),
        (@"         \___________/      ", ""),
        (@"              | |           ", ""),
        (@"             _| |_          ", ""),
        (@"            |_____|         ", ""),
    ];

    public static void Show()
    {
        AnsiConsole.WriteLine();

        foreach (var (art, quote) in Lines)
        {
            if (string.IsNullOrWhiteSpace(quote))
                AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(art)}[/]");
            else
                AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(art)}[/]{quote}");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Ralph Wiggum Loop — Iterative Fresh-Context Agent Loop[/]");
        AnsiConsole.WriteLine();
    }
}
