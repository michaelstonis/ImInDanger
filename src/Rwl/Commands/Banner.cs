using Spectre.Console;

namespace Rwl.Commands;

public static class Banner
{
    // Verified quotes from The Simpsons (sourced from en.wikiquote.org)
    private static readonly string[] Quotes =
    [
        "Me fail English? That's unpossible.",
        "I eated the purple berries.",
        "They taste like burning.",
        "I found a moon rock in my nose.",
        "I bent my Wookiee.",
        "And the doctor told me that both my eyes were lazy, and that's why it was the best summer ever!",
        "I just ate a thumbtack!",
        "You're wrinkly, somebody should iron you.",
        "Why do people run from me?",
        "I once picked my nose 'til it bleeded.",
        "Smells like hot dogs.",
        "The pointy kitty took it!",
    ];

    private static readonly string Art =
"""
                                        @@@@@@@@@@@@@@@@@
                                 @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                            @@@@  @@@@@@   @@@ @@@  @@ @@@@ @@@@@@@@@@
                        @@@@@  @@@@@@@   @@  @@@   @@  @@ @@  @@ @@@@@@@@@
                     @@@@@  @@@@ @@@   @@  @@@    @@   @@  @@  @@@ @@@@@@@@@@
                  @@@@@@  @@@  @@@   @@@  @@     @@    @    @@   @@  @@@@@@@@@@@
                @@@@@@  @@@   @@    @@   @@     @@    @@     @@   @@   @@@ @@@@@@@
              @@@ @@   @@   @@    @@    @@      @@    @@     @@    @@    @@  @@ @@@@@
            @@@ @@   @@@   @@    @@    @@      @@     @@      @@    @@    @@  @@@  @@@@
           @@  @@   @@    @@    @@     @@      @@     @@      @@     @@    @@   @@  @@@@@
         @@@  @@   @@    @@     @     @@       @      @@      @@      @@    @@   @@   @@@@@
        @@   @@   @@    @@     @@     @@       @      @        @@      @     @    @@   @@ @@@
       @@   @    @@    @@@    @@      @@                                                @@  @@
      @@   @@   @@     @@     @@                                                         @@   @
     @@   @@   @@     @@     @@             @@@@@@@@@@@@@                 @@@@@@@@@@@    @@
     @   @@    @@     @@     @@           @@@           @@@             @@           @@   @@
    @@   @@   @@     @@                  @@               @@          @@              @@  @@
    @@  @@    @@     @@                 @@                 @@         @@               @@ @@
    @   @@   @@      @@                 @@     @@           @         @        @@@      @ @@
        @            @                  @@    @@@@          @         @@       @@@     @@ @@
       @@@@@@@                          @@     @@          @@          @               @  @@
      @@@    @@@                         @@                @            @@           @@@  @@
     @@                                   @@@            @@    @@@@@@@@  @@@@      @@@    @
    @@   @@@@                               @@@@      @@@           @@@@@@  @@@@@@       @@
     @  @ @                                     @@@@@                    @@               @
     @@    @                                            @@                @@              @@
      @@@                                              @@  @@@@@          @@               @@@
        @@@@    @                                      @@@@@   @@@       @@                  @@
          @@@@@@@                                    @@@@       @@@@@@@@@@                    @@
             @@                                    @@@         @@@@                            @@
              @@                                  @@         @@@                               @@
               @@@                        @@     @@         @@  @@@                           @@
                 @@                       @@    @@         @@@@@@@@@@@                     @@@@
                  @@@                    @@@@@@@@@         @@@       @@               @@@@@@
                 @@ @@@@                @@      @                   @@@@@@@@@@@@@@@@@@@
                @@     @@@                      @                @@@@  @@@       @@
               @@        @@@@@                  @               @        @@      @
               @            @@@@@@              @                         @@   @@
              @@                @@@@@@@@@   @@@@@                      @ @@  @@@@
              @@@@                    @@@@@@@@@@@@@@                 @@@@@@ @@  @@@@
            @@@@@@@                       @@       @@@@             @@ @@@ @@      @@@
         @@@      @@@                    @@           @@@@          @@@    @@    @@@ @@
        @           @@@@                @@               @@@@       @@      @@ @@@    @@
       @@             @@@@@           @@@                  @@@      @@       @@@@@@   @@
       @                 @@@@@       @@@                     @@@    @@     @@@    @@   @@@
      @@                    @@@@@   @@                         @@  @@     @@       @@@ @@@@
       @                         @@@@                           @@@@@@@ @@@          @@@@ @@
       @@                      @@@@                             @@@   @@@             @@@  @@
       @@                                                        @@                    @    @@
       @@@                                                       @@                          @@
      @@@@                                                      @@@                           @@
    @@   @@                                                    @@@                            @@@
    @     @@                                                 @@@                               @@@
   @       @@@                                              @@@                                 @@@@
  @@        @@@                                           @@@                                   @@@@
""";

    public static void Show()
    {
        var quote = Quotes[Random.Shared.Next(Quotes.Length)];

        AnsiConsole.WriteLine();

        foreach (var line in Art.Split('\n'))
            AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(line)}[/]");

        AnsiConsole.MarkupLine($"  [italic dim]\"{Markup.Escape(quote)}\"[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Ralph Wiggum Loop — Iterative Fresh-Context Agent Loop[/]");
        AnsiConsole.WriteLine();
    }
}
