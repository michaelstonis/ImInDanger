using Spectre.Console;

namespace Rwl.Commands;

public static class Banner
{
    // ASCII Ralph Wiggum face (Simpsons-style), all art strings padded to 32 chars
    private static readonly (string Art, string Quote)[] Lines =
    [
        (@"         .-----------.          ", ""),
        (@"        /  ~~~~~~~~~  \         ", ""),
        (@"       |               |        ", ""),
        (@"       |    _      _   |        ", ""),
        (@"       |  ( O )  ( O ) |        ", "  [italic dim]\"My context window[/]"),
        (@"       |               |        ", "  [italic dim] smells like[/]"),
        (@"       |     ( . )     |        ", "  [italic dim] fresh tasks.\"[/]"),
        (@"       |               |        ", ""),
        (@"       |    \_____/    |        ", ""),
        (@"        \             /         ", ""),
        (@"         '-----------'          ", ""),
        (@"               | |              ", ""),
        (@"              _| |_             ", ""),
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
