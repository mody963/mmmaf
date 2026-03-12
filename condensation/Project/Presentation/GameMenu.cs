using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Spectre.Console;

public static class GameMenu
{
    private static readonly GameLogic _gameLogic = new GameLogic();
    private const string _backOption = "Back";

    public static void Start(Cart cart) // cart meegegeven vanuit MainMenu zodat we dezelfde cart gebruiken
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
                    ShowGameList(cart); // cart meegeven zodat het vanuit begin zelfde is
                    break;

                case var c when c == Texts.Get("Game_Search"):
                    SearchAndDisplayGames(cart, onlyActive: true);
                    break;

                case var c when c == Texts.Get("Game_Filter"):
                    FilterGamesByGenre();
                    break;

                case var c when c == Texts.Get("Game_Back"):
                    return;
            }
        }
    }

    private static void ShowGameList(Cart cart)
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
                cart.AddToCart(selectedGame.Id, selectedGame.Title, selectedGame.Price); // cart van meegegeven
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

        private static void SearchAndDisplayGames(Cart cart, bool onlyActive = false)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"\n[bold cyan]--- Search for a game ---[/]");
            string searchTitle = AnsiConsole.Prompt(new TextPrompt<string>("Enter game title to search for:"));
            
            var results = _gameLogic.SearchGamesByTitle(searchTitle);
            if (onlyActive)
            {
                results = results.Where(g => g.IsActive).ToList();
            }

            if (results.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No games found matching that title.[/]");
                AnsiConsole.MarkupLine("[grey]Press any key to return...[/]");
                Console.ReadKey(true);
                continue;
            }

            // Inner loop: browse search results
            while (true)
            {
                AnsiConsole.Clear();

                // Display search results in a selectable list like game_list
                int maxTitleLen = results.Max(g => g.Title.Length);
                const int priceWidth = 8;

                var prompt = new SelectionPrompt<GameModel>()
                    .Title("[bold]Select a game to view details:[/]")
                    .UseConverter(g =>
                    {
                        if (g.Id == -1) // New Search option
                            return "New Search";
                        if (g.Id == -2) // Back to Game Menu option
                            return "Back to Game Menu";
                        return string.Format($"{{0,-{maxTitleLen}}}  €{{1,{priceWidth}:0.00}}", g.Title, g.Price);
                    })
                    .HighlightStyle(new Style(foreground: Color.Green));

                foreach (var game in results)
                    prompt.AddChoice(game);

                prompt.AddChoice(new GameModel { Id = -1, Title = "New Search", Price = 0 });
                prompt.AddChoice(new GameModel { Id = -2, Title = "Back to Game Menu", Price = 0 });

                var selectedGame = AnsiConsole.Prompt(prompt);
                if (selectedGame.Id == -1) // New Search selected
                    break; // Break inner loop to go back to search prompt
                if (selectedGame.Id == -2) // Back to Game Menu selected
                    return;

                // Show details in a table (same as game_list)
                var detailsTable = new Table().Border(TableBorder.Rounded);
                detailsTable.AddColumn("Property");
                detailsTable.AddColumn("Value");
                detailsTable.AddRow("Title", selectedGame.Title);
                detailsTable.AddRow("Description", string.IsNullOrWhiteSpace(selectedGame.Description) ? "<none>" : selectedGame.Description);
                detailsTable.AddRow("Price", $"€{selectedGame.Price:0.00}");

                // Ask user what to do next: add to cart or back to results
                string detailAction = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]What would you like to do?[/]")
                        .AddChoices("Add to cart", "Back to results")
                        .HighlightStyle(new Style(foreground: Color.Green))
                );
                if (detailAction == "Add to cart")
                {
                    cart.AddToCart(selectedGame.Id, selectedGame.Title, selectedGame.Price);
                    // Continue inner loop to show results again
                }
                // else "Back to results" - continue inner loop
            }
        }
    }
}