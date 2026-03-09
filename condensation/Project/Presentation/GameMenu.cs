using System;
using Spectre.Console;

public static class GameMenu
{
    private static readonly GameLogic _gameLogic = new GameLogic();
    private const string _backOption = "Back";

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
                    AnsiConsole.MarkupLine("[red]Game search not implemented.[/]");
                    Console.ReadKey(true);
                    break;

                case var c when c == Texts.Get("Game_Filter"):
                    FilterGamesByGenre();
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
            AnsiConsole.MarkupLine("[red]No games available.[/]");
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
        AnsiConsole.MarkupLine("\n[grey]Press any key to return...[/]");
        Console.ReadKey(true);
    }

    private static void FilterGamesByGenre()
    {
        AnsiConsole.Clear();

        List<GenreModel> genres = _gameLogic.GetAllGenres();

        if (genres.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No genres available.[/]");
            Console.ReadKey(true);
            return;
        }

        var genreChoices = genres.Select(g => g.Name).Append(_backOption).ToList();

        string selectedGenreName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Choose a genre to filter by:[/]")
                .AddChoices(genreChoices)
                .HighlightStyle(new Style(foreground: Color.Yellow))
        );

        if (selectedGenreName == _backOption) return;

        GenreModel? selectedGenre = genres.FirstOrDefault(g => g.Name == selectedGenreName);

        if (selectedGenre == null)
        {
            AnsiConsole.MarkupLine("[red]Invalid genre selection.[/]");
            Console.ReadKey(true);
            return;
        }

        List<GameModel> filteredGames = _gameLogic.GetGamesByGenre(selectedGenre.Id);

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold green]Games in genre:[/] [yellow]{selectedGenre.Name}[/]");

        if (filteredGames.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No games were found for the selected genre.[/]");
            AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
            Console.ReadKey(true);
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Title");
        table.AddColumn("Description");
        table.AddColumn("Price");

        foreach (var game in filteredGames)
        {
            table.AddRow(game.Title, $"€{game.Price:0.00}");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[grey]Press any key to return...[/]");
        Console.ReadKey(true);
    }
}