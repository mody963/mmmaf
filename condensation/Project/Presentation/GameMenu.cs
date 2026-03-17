using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Spectre.Console;

public static class GameMenu
{
    private static readonly GameLogic _gameLogic = new GameLogic();
    // we use a readonly field so the value is pulled from resources once the class is loaded
    private static readonly string _backOption = Texts.Get("Go_Back");

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
            SoundEffects.PlayMenuClick();

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
                AnsiConsole.MarkupLine($"[red]{Texts.Get("No_Games_Available")}[/]");
                Console.ReadKey(true);
                return;
            }

            int maxTitleLen = games.Max(g => g.Title.Length);
            const int priceWidth = 8;

            var prompt = new SelectionPrompt<GameModel>()
                .Title($"[bold]{Texts.Get("Game_SelectDetails")}[/]")
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
            SoundEffects.PlayMenuClick();
            if (selectedGame.Id == -1)
                return;

            var detailsTable = new Table().Border(TableBorder.Rounded);
            detailsTable.AddColumn(Texts.Get("Game_Property"));
            detailsTable.AddColumn(Texts.Get("Game_Value"));
            detailsTable.AddRow(Texts.Get("Title"), selectedGame.Title);
            detailsTable.AddRow(Texts.Get("Description"), string.IsNullOrWhiteSpace(selectedGame.Description) ? "<none>" : selectedGame.Description);
            detailsTable.AddRow(Texts.Get("Price"), $"€{selectedGame.Price:0.00}");

            var genreName = _gameLogic.GetAllGenres()
                              .FirstOrDefault(g => g.Id == selectedGame.GenreId)?.Name ?? "<unknown>";
            var ageName = _gameLogic.GetAllAgeRatings()
                              .FirstOrDefault(a => a.Id == selectedGame.AgeRatingId)?.Name ?? "<unknown>";
            detailsTable.AddRow(Texts.Get("Game_Genre"), genreName);
            detailsTable.AddRow(Texts.Get("Game_AgeRating"), ageName);

            AnsiConsole.Clear();
            AnsiConsole.Write(detailsTable);

            string detailAction = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]{Texts.Get("Game_WhatWouldYouLikeToDo")}[/]")
                    .AddChoices(Texts.Get("Game_AddToCart"), _backOption)
                    .HighlightStyle(new Style(foreground: Color.Green))
            );
            SoundEffects.PlayMenuClick();
            if (detailAction == Texts.Get("Game_AddToCart"))
            {
                cart.AddToCart(selectedGame.Id, selectedGame.Title, selectedGame.Price);
            }
            // loop again after add or back
        }
    }

    private static void FilterGamesByGenre()
    {
        AnsiConsole.Clear();

        List<GenreModel> genres = _gameLogic.GetAllGenres();

        if (genres.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("No_genres_available")}[/]");
            Console.ReadKey(true);
            return;
        }

        var genreChoices = genres.Select(g => g.Name).Append(_backOption).ToList();

        string selectedGenreName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[bold]{Texts.Get("Game_ChooseGenre")}[/]")
                .AddChoices(genreChoices)
                .HighlightStyle(new Style(foreground: Color.Yellow))
        );
        SoundEffects.PlayMenuClick();

        if (selectedGenreName == _backOption) return;

        GenreModel? selectedGenre = genres.FirstOrDefault(g => g.Name == selectedGenreName);

        if (selectedGenre == null)
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("Invalid_genre_selection")}[/]");
            Console.ReadKey(true);
            return;
        }

        List<GameModel> filteredGames = _gameLogic.GetGamesByGenre(selectedGenre.Id);

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold green]{Texts.Get("Games_in_genre")}[/] [yellow]{selectedGenre.Name}[/]");

        if (filteredGames.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("No_games_for_genre")}[/]");
            AnsiConsole.MarkupLine($"[grey]{Texts.Get("Press_Any_Key_To_Return")}[/]");
            Console.ReadKey(true);
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn(Texts.Get("Title"));
        table.AddColumn(Texts.Get("Price"));

        foreach (var game in filteredGames)
        {
            table.AddRow(game.Title, $"€{game.Price:0.00}");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[grey]{Texts.Get("Press_Any_Key_To_Return")}[/]");
        Console.ReadKey(true);
    }

        private static void SearchAndDisplayGames(Cart cart, bool onlyActive = false)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"\n[bold cyan]{Texts.Get("Game_SearchHeader")}[/]");
            string searchTitle = AnsiConsole.Prompt(new TextPrompt<string>(Texts.Get("Search_Title")));
            
            var results = _gameLogic.SearchGamesByTitle(searchTitle);
            if (onlyActive)
            {
                results = results.Where(g => g.IsActive).ToList();
            }

            if (results.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]{Texts.Get("No_games_found")}[/]");
                AnsiConsole.MarkupLine($"[grey]{Texts.Get("Press_Any_Key_To_Return")}[/]");
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
                    .Title($"[bold]{Texts.Get("Game_SelectDetails")}[/]")
                    .UseConverter(g =>
                    {
                        if (g.Id == -1) // New Search option
                            return Texts.Get("Game_NewSearch");
                        if (g.Id == -2) // Back to Game Menu option
                            return Texts.Get("Game_BackToMenu");
                        return string.Format($"{{0,-{maxTitleLen}}}  €{{1,{priceWidth}:0.00}}", g.Title, g.Price);
                    })
                    .HighlightStyle(new Style(foreground: Color.Green));

                foreach (var game in results)
                    prompt.AddChoice(game);

                prompt.AddChoice(new GameModel { Id = -1, Title = Texts.Get("Game_NewSearch"), Price = 0 });
                prompt.AddChoice(new GameModel { Id = -2, Title = Texts.Get("Game_BackToMenu"), Price = 0 });

                var selectedGame = AnsiConsole.Prompt(prompt);
                SoundEffects.PlayMenuClick();
                if (selectedGame.Id == -1) // New Search selected
                    break; // Break inner loop to go back to search prompt
                if (selectedGame.Id == -2) // Back to Game Menu selected
                    return;

                // Show details in a table (same as game_list)
                var detailsTable = new Table().Border(TableBorder.Rounded);
                detailsTable.AddColumn(Texts.Get("Game_Property"));
                detailsTable.AddColumn(Texts.Get("Game_Value"));
                detailsTable.AddRow(Texts.Get("Title"), selectedGame.Title);
                detailsTable.AddRow(Texts.Get("Description"), string.IsNullOrWhiteSpace(selectedGame.Description) ? "<none>" : selectedGame.Description);
                detailsTable.AddRow(Texts.Get("Price_Without_E.g"), $"€{selectedGame.Price:0.00}");

                var genreName = _gameLogic.GetAllGenres()
                                  .FirstOrDefault(g => g.Id == selectedGame.GenreId)?.Name ?? "<unknown>";
                var ageName = _gameLogic.GetAllAgeRatings()
                                  .FirstOrDefault(a => a.Id == selectedGame.AgeRatingId)?.Name ?? "<unknown>";
                detailsTable.AddRow(Texts.Get("Game_Genre"), genreName);
                detailsTable.AddRow(Texts.Get("Game_AgeRating"), ageName);

                AnsiConsole.Write(detailsTable);

                // Ask user what to do next: add to cart or back to results
                string detailAction = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold]{Texts.Get("Game_WhatWouldYouLikeToDo")}[/]")
                        .AddChoices(Texts.Get("Game_AddToCart"), Texts.Get("Game_BackToResults"))
                        .HighlightStyle(new Style(foreground: Color.Green))
                );
                SoundEffects.PlayMenuClick();
                if (detailAction == Texts.Get("Game_AddToCart"))
                {
                    cart.AddToCart(selectedGame.Id, selectedGame.Title, selectedGame.Price);
                    // Continue inner loop to show results again
                }
                // else "Back to results" - continue inner loop
            }
        }
    }
}