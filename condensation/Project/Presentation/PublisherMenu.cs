using System.Runtime.CompilerServices;
using Spectre.Console;

public static class PublisherMenu
{
    private static readonly GameLogic _gameLogic = new GameLogic();
    private static readonly PublisherLogic _publisherLogic = new PublisherLogic();

    public static void Start()
    {
        // Haal de gekoppelde publisher-gegevens op van de ingelogde gebruiker
        var publisher = _publisherLogic.GetByAccountId(CurrentUserModel.CurrentUser.Id);

        if (publisher == null)
        {
            AnsiConsole.MarkupLine("[red]Fout: Geen publisher profiel gevonden voor dit account.[/]");
            Console.ReadKey(true);
            return;
        }

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold blue]Publisher Dashboard:[/] [yellow]{publisher.StudioName}[/]\n");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Wat wilt u doen?")
                    .AddChoices(
                        "Add My Game",
                        "Update My Game",
                        "Delete My Game",
                        "View My Ratings",
                        "Go Back"
                    )
                    .HighlightStyle(new Style(foreground: Color.Cyan1))
            );

            SoundEffects.PlayMenuClick();

            switch (choice)
            {
                case "Add My Game":
                    AdminMenu.AddGameMenu(publisher.Id);
                    break;
                case "Update My Game":
                    UpdateGameMenu();
                    break;
                case "Delete My Game":
                    AdminMenu.DeleteGameMenu();
                    break;
                case "View My Ratings":
                    ShowPublisherRatings(publisher.Id);
                    break;
                case "Go Back":
                    return;
            }
        }
    }



    private static GameModel? SearchAndSelectGame(string action, bool onlyActive = false)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"\n[bold cyan]--- {action} ---[/]");
        string searchTitle = AnsiConsole.Prompt(new TextPrompt<string>(Texts.Get("Search_Title"))); 
        SoundEffects.PlayMenuClick();
        
        List<GameModel> results = _gameLogic.SearchGamesByTitle(searchTitle);
        if (results.Count == 0)
        {
            SoundEffects.PlayErrorSound();
            AnsiConsole.MarkupLine($"[red]{Texts.Get("No_games_found")}[/]");
            return null;
        }

        foreach (var game in results)
        {
            List<GameModel> publishergames = _gameLogic.GetActiveGames().Where(g => g.PublisherId == CurrentUserModel.CurrentUser.Id).ToList();
            var selectedGame = AnsiConsole.Prompt(
                new SelectionPrompt<GameModel>()
                    .Title($"{Texts.Get("Game_SelectDetails")} {action.ToLower()}")
                    .UseConverter(g => $"{g.Title} (${g.Price}) - Active: {g.IsActive}")
                    .AddChoices(publishergames)
            );
            SoundEffects.PlayMenuClick();
            return selectedGame;
        }

        AnsiConsole.MarkupLine($"[red]{Texts.Get("No_games_found")}[/]");
        return null;
 
    }

    private static void UpdateGameMenu()
    {
        var game = SearchAndSelectGame("Update a Game");
        if (game == null) { SoundEffects.PlayErrorSound(); Console.ReadKey(true); return; }

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"\n[bold yellow]{Texts.Get("Admin_UpdatingGame")} {game.Title}[/]");
        AnsiConsole.MarkupLine($"[grey]{Texts.Get("Admin_PressEnterToKeep")}[/]\n");

        game.Title = AnsiConsole.Prompt(new TextPrompt<string>($"{Texts.Get("Title")}:").DefaultValue(game.Title));
        SoundEffects.PlayMenuClick();
        game.Description = AnsiConsole.Prompt(new TextPrompt<string>($"{Texts.Get("Description")}:" ).DefaultValue(game.Description));
        SoundEffects.PlayMenuClick();
        game.Price = AnsiConsole.Prompt(new TextPrompt<double>($"{Texts.Get("Price")}:" ).DefaultValue(game.Price));
        SoundEffects.PlayMenuClick();

        // 2. Dropdown for Genre
        var genres = _gameLogic.GetAllGenres();
        var currentGenre = genres.FirstOrDefault(g => g.Id == game.GenreId); // Find the current one
        
        var selectedGenre = AnsiConsole.Prompt(
            new SelectionPrompt<GenreModel>()
                .Title($"{Texts.Get("Admin_SelectGenreWithCurrent")} [yellow]{currentGenre?.Name}[/]):")
                .UseConverter(g => g.Name)
                .AddChoices(genres)
        );
        SoundEffects.PlayMenuClick();
        game.GenreId = selectedGenre.Id;

        var ageRatings = _gameLogic.GetAllAgeRatings();
        var currentAgeRating = ageRatings.FirstOrDefault(a => a.Id == game.AgeRatingId);
        
        var selectedAgeRating = AnsiConsole.Prompt(
            new SelectionPrompt<AgeRatingModel>()
                .Title($"{Texts.Get("Admin_SelectAgeRatingWithCurrent")} [yellow]{currentAgeRating?.Name}[/]):")
                .UseConverter(a => a.Name)
                .AddChoices(ageRatings)
        );
        SoundEffects.PlayMenuClick();
        game.AgeRatingId = selectedAgeRating.Id;

        game.IsActive = AnsiConsole.Confirm(Texts.Get("Admin_GameActivePrompt"), defaultValue: game.IsActive);
        SoundEffects.PlayMenuClick();

        _gameLogic.UpdateGame(game);

        AnsiConsole.MarkupLine($"\n[green]{Texts.Get("Admin_SuccessfullyUpdated")} '{game.Title}'![/]");
        AnsiConsole.MarkupLine(Texts.Get("Admin_PressAnyKeyToReturn"));
        Console.ReadKey(true);
    }



    private static readonly ReviewLogic _reviewLogic = new ReviewLogic();

    private static void ShowPublisherRatings(int publisherId)
    {
        const int pageSize = 10;

        AnsiConsole.Clear();

        var allReviews = _reviewLogic.GetPublisherReviews(publisherId);

        if (allReviews.Count == 0)
        {
            SoundEffects.PlayErrorSound();
            AnsiConsole.MarkupLine("[red]No reviews available for your games.[/]");
            Console.ReadKey(true);
            return;
        }

        var gamesWithReviews = allReviews
            .GroupBy(r => new { r.GameId })
            .Select(g => new
            {
                GameId = g.Key.GameId,  
                GameTitle = _gameLogic.GetGameById(g.Key.GameId)?.Title ?? "Unknown Game",
                Reviews = g.ToList()
            })
            .OrderBy(g => g.GameTitle)
            .ToList();

        int selectedIndex = 0;
        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(gamesWithReviews.Count / (double)pageSize);

        while (true)
        {
            AnsiConsole.Clear();

            var pageGames = gamesWithReviews
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            if (selectedIndex >= pageGames.Count)
                selectedIndex = pageGames.Count - 1;

            AnsiConsole.MarkupLine($"[bold]Select a game to view reviews[/] [grey](Page {currentPage + 1}/{totalPages})[/]\n");

            for (int i = 0; i < pageGames.Count; i++)
            {
                var g = pageGames[i];
                string line = $"{g.GameTitle} ({g.Reviews.Count} reviews)";

                if (i == selectedIndex)
                    AnsiConsole.MarkupLine($"[bold green]> {Markup.Escape(line)}[/]");
                else
                    AnsiConsole.MarkupLine($"  {Markup.Escape(line)}");
            }

            AnsiConsole.MarkupLine("\n[grey]↑ ↓ Select   ← → Page   Enter: Open   Esc: Back[/]");

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
                    SoundEffects.PlayMenuClick();
                    ShowReviewsForGame(pageGames[selectedIndex]);
                    break;
            }
        }
    }

    private static void ShowReviewsForGame(dynamic gameGroup)
    {
        const int pageSize = 10;

        var reviews = ((List<ReviewModel>)gameGroup.Reviews)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(reviews.Count / (double)pageSize);

        while (true)
        {
            AnsiConsole.Clear();

            var pageReviews = reviews
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            AnsiConsole.MarkupLine($"[bold yellow]{Markup.Escape(gameGroup.GameTitle)}[/]");
            AnsiConsole.MarkupLine($"[grey]Reviews (Page {currentPage + 1}/{totalPages})[/]\n");

            foreach (var r in pageReviews)
            {
                var panel = new Panel(
                    $"[bold]{Markup.Escape(r.ReviewerName)}[/]\n" +
                    $"[yellow]Rating:[/] {r.Rating}/10\n\n" +
                    $"{Markup.Escape(r.Comment)}\n\n" +
                    $"[grey]{r.CreatedAt:g}[/]"
                )
                .Border(BoxBorder.Rounded)
                .Padding(1, 1);

                AnsiConsole.Write(panel);
            }

            // Average rating
            double avg = reviews.Average(r => r.Rating);

            AnsiConsole.MarkupLine($"\n[bold green]Average Rating:[/] {avg:0.00}/10");

            AnsiConsole.MarkupLine("\n[grey]← → Page   Esc: Back[/]");

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (currentPage > 0)
                    {
                        currentPage--;
                        SoundEffects.PlayMenuClick();
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (currentPage < totalPages - 1)
                    {
                        currentPage++;
                        SoundEffects.PlayMenuClick();
                    }
                    break;

                case ConsoleKey.Escape:
                    return;
            }
        }
    }


}