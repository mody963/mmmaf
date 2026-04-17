using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Spectre.Console;

public static class GameMenu
{
    private static readonly GameLogic _gameLogic = new GameLogic();
    private static readonly ReviewLogic _reviewLogic = new ReviewLogic();
    private static readonly OrderLogic _orderLogic = new OrderLogic();
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
        const int pageSize = 10;

        AnsiConsole.Clear();
        var games = _gameLogic.GetActiveGames();

        if (games.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("No_Games_Available")}[/]");
            Console.ReadKey(true);
            return;
        }

        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(games.Count / (double)pageSize);
        int maxTitleLen = games.Max(g => g.Title.Length);
        const int priceWidth = 8;
        int selectedIndex = 0;

        while (true)
        {
            AnsiConsole.Clear();
            var pageGames = games.Skip(currentPage * pageSize).Take(pageSize).ToList();

            if (selectedIndex >= pageGames.Count)
                selectedIndex = pageGames.Count - 1;

            AnsiConsole.MarkupLine($"[bold]{Texts.Get("Game_SelectDetails")}[/] [grey](Page {currentPage + 1} / {totalPages})[/]\n");

            for (int i = 0; i < pageGames.Count; i++)
            {
                var g = pageGames[i];
                string line = Markup.Escape(string.Format($"{{0,-{maxTitleLen}}}  €{{1,{priceWidth}:0.00}}", g.Title, g.Price));
                if (i == selectedIndex)
                    AnsiConsole.MarkupLine($"[bold green]> {line}[/]");
                else
                    AnsiConsole.MarkupLine($"  {line}");
            }

            AnsiConsole.MarkupLine("\n[grey]↑ ↓  Select   ← →  Page   Enter: Open   Esc: Back[/]");

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + pageGames.Count) % pageGames.Count;
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % pageGames.Count;
                    break;

                case ConsoleKey.LeftArrow:
                    if (currentPage > 0)
                    {
                        currentPage--;
                        selectedIndex = 0;
                        SoundEffects.PlayMenuClick();
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (currentPage < totalPages - 1)
                    {
                        currentPage++;
                        selectedIndex = 0;
                        SoundEffects.PlayMenuClick();
                    }
                    break;

                case ConsoleKey.Escape:
                    return;

                case ConsoleKey.Enter:
                    {
                        SoundEffects.PlayMenuClick();
                        var selectedGame = pageGames[selectedIndex];

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
                        RenderReviewsForGame(selectedGame.Id);

                        string detailAction = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title($"[bold]{Texts.Get("Game_WhatWouldYouLikeToDo")}[/]")
                                .AddChoices(Texts.Get("Game_AddToCart"), _backOption)
                                .HighlightStyle(new Style(foreground: Color.Green))
                        );
                        SoundEffects.PlayMenuClick();
                        if (detailAction == Texts.Get("Game_AddToCart"))
                        {
                            // Get the current customer profile
                            var customerLogic = new CustomersLogic();
                            var customer = customerLogic.GetByAccountId(CurrentUserModel.CurrentUser!.Id);

                            // Check if they already own it!
                            if (customer != null && _orderLogic.HasPurchasedGame(customer.Id, selectedGame.Id))
                            {
                                AnsiConsole.MarkupLine("\n[red]You already own this game![/]");
                                AnsiConsole.MarkupLine("Press any key to return...");
                                Console.ReadKey(true);
                            }
                            else
                            {
                                cart.AddToCart(selectedGame.Id, selectedGame.Title, selectedGame.Price);
                                AnsiConsole.MarkupLine("\n[green]Game added to cart![/]");
                                Thread.Sleep(1000);
                            }
                        }
                        break;
                    }
            }
        }
    }

    private static void FilterGamesByGenre()
    {
        AnsiConsole.Clear();

        List<GenreModel> genres = _gameLogic.GetAllGenres();

        if (genres.Count == 0)
        {
            SoundEffects.PlayErrorSound();
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
            SoundEffects.PlayErrorSound();
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
                SoundEffects.PlayErrorSound();
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
                RenderReviewsForGame(selectedGame.Id);

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

    private static void RenderReviewsForGame(int gameId)
    {
        var reviews = _reviewLogic.GetReviewsForGame(gameId);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]{Texts.Get("MyGames_ReviewsHeader")}[/]");

        if (reviews.Count == 0)
        {
            AnsiConsole.MarkupLine($"[grey]{Texts.Get("MyGames_NoReviewsYet")}[/]");
            return;
        }

        var reviewTable = new Table().Border(TableBorder.Rounded);
        reviewTable.AddColumn(Texts.Get("MyGames_Reviewer"));
        reviewTable.AddColumn(Texts.Get("MyGames_Rating"));
        reviewTable.AddColumn(Texts.Get("MyGames_Comment"));
        reviewTable.AddColumn(Texts.Get("MyGames_ReviewDate"));

        foreach (var review in reviews)
        {
            reviewTable.AddRow(
                Markup.Escape(string.IsNullOrWhiteSpace(review.ReviewerName) ? "Unknown" : review.ReviewerName),
                review.Rating.ToString(),
                Markup.Escape(string.IsNullOrWhiteSpace(review.Comment) ? "-" : review.Comment),
                review.CreatedAt.ToString("yyyy-MM-dd")
            );
        }

        AnsiConsole.Write(reviewTable);
    }
}