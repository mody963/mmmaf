using Spectre.Console;

public static class AdminMenu
{
    public static void Start()
    {
        bool exitMenu = false;

        while (!exitMenu)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Admin Menu[/]")
                    .AddChoices(
                        "Add Game",
                        "Update Game",
                        "Delete Game",
                        "Log out"
                    )
                    .HighlightStyle(new Style(foreground: Color.Yellow))
            );

            switch (choice)
            {
                case "Add Game":
                    AnsiConsole.MarkupLine("[yellow]Add Game not implemented yet.[/]");
                    AnsiConsole.MarkupLine("\nPress any key to return...");
                    Console.ReadKey(true);
                    break;

                case "Update Game":
                    AnsiConsole.MarkupLine("[yellow]Update Game not implemented yet.[/]");
                    AnsiConsole.MarkupLine("\nPress any key to return...");
                    Console.ReadKey(true);
                    break;

                case "Delete Game":
                    AnsiConsole.MarkupLine("[yellow]Delete Game not implemented yet.[/]");
                    AnsiConsole.MarkupLine("\nPress any key to return...");
                    Console.ReadKey(true);
                    break;

                case "Log out":
                    Logout();
                    exitMenu = true; // Return to login/main menu
                    break;
            }
        }
    }

    private static void Logout()
    {
        CurrentUserModel.CurrentUser = null;
        AnsiConsole.MarkupLine("\n[green]You have been logged out.[/]");
        AnsiConsole.MarkupLine("Press any key to return to the main menu...");
        Console.ReadKey(true);
    }
}