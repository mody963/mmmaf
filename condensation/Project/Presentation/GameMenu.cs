using System;
using Spectre.Console;

public static class GameMenu
{
    private static readonly GameLogic _gameLogic = new GameLogic();

    public static void Start()
    {
        while (true)
        {
            AnsiConsole.Clear();

            var gameMenu = new SelectionPrompt<string>()
                .Title($"[bold]{Texts.Get("Menu_Game")}[/]")
                .AddChoices(
                    Texts.Get("Game_List"),
                    Texts.Get("Game_Search"),
                    Texts.Get("Game_Filter"),
                    Texts.Get("Game_Back")
                )
                .HighlightStyle(new Style(foreground: Color.Green));

            var gameChoice = AnsiConsole.Prompt(gameMenu);

            switch (gameChoice)
            {
                case var c when c == Texts.Get("Game_List"):
                    ShowGameList();
                    break;

                case var c when c == Texts.Get("Game_Search"):
                    AnsiConsole.MarkupLine("[blue]Game search not implemented.[/]");
                    Console.ReadKey(true);
                    break;

                case var c when c == Texts.Get("Game_Filter"):
                    AnsiConsole.MarkupLine("[blue]Game filter not implemented.[/]");
                    Console.ReadKey(true);
                    break;

                case var c when c == Texts.Get("Game_Back"):
                    return;
            }
        }
    }

    private static void ShowGameList()
    {
        AnsiConsole.Clear();

        var games = _gameLogic.GetActiveGames();

        if (games.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No games available.[/]");
            Console.ReadKey(true);
            return;
        }

        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.AddColumn("Game Name");
        table.AddColumn("Price");

        foreach (var game in games)
        {
            table.AddRow(game.Title, $"€{game.Price:0.00}");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\nPress any key to return...");
        Console.ReadKey(true);
    }
}