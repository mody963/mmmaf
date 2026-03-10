using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        while (true)
        {
            AnsiConsole.Clear();

            var games = _gameLogic.GetActiveGames();

            if (games.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No games available.[/]");
                Console.ReadKey(true);
                return;
            }

            int maxTitleLen = games.Max(g => g.Title.Length);
            const int priceWidth = 8;

            var prompt = new SelectionPrompt<GameModel>()
                .Title("[bold]Select a game to view details:[/]")
                .UseConverter(g =>
                {
                    if (g.Id == -1) // back option
                        return _backOption;
                    return string.Format($"{{0,-{maxTitleLen}}}  €{{1,{priceWidth}:0.00}}", g.Title, g.Price);
                })
                .HighlightStyle(new Style(foreground: Color.Green));

            foreach (var game in games)
                prompt.AddChoice(game);

            prompt.AddChoice(new GameModel { Id = -1, Title = _backOption, Price = 0 });

            var selectedGame = AnsiConsole.Prompt(prompt);
            if (selectedGame.Id == -1) // back option selected
                return;

            // show details in a panel or table
            var detailsTable = new Table().Border(TableBorder.Rounded);
            detailsTable.AddColumn("Property");
            detailsTable.AddColumn("Value");
            detailsTable.AddRow("Title", selectedGame.Title);
            detailsTable.AddRow("Description", string.IsNullOrWhiteSpace(selectedGame.Description) ? "<none>" : selectedGame.Description);
            detailsTable.AddRow("Price", $"€{selectedGame.Price:0.00}");

            // look up genre and age rating names
            var genreName = _gameLogic.GetAllGenres()
                              .FirstOrDefault(g => g.Id == selectedGame.GenreId)?.Name ?? "<unknown>";
            var ageName = _gameLogic.GetAllAgeRatings()
                              .FirstOrDefault(a => a.Id == selectedGame.AgeRatingId)?.Name ?? "<unknown>";
            detailsTable.AddRow("Genre", genreName);
            detailsTable.AddRow("Age rating", ageName);

            AnsiConsole.Clear();
            AnsiConsole.Write(detailsTable);

            // ask user what to do next: add to cart or back
            string detailAction = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]What would you like to do?[/]")
                    .AddChoices("Add to cart", _backOption)
                    .HighlightStyle(new Style(foreground: Color.Green))
            );
            if (detailAction == "Add to cart")
            {
                new Cart().AddToCart(selectedGame.Id, selectedGame.Title, selectedGame.Price);
            }
            // if back or after adding to cart, simply loop again
        }
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