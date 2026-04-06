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
                        "Delete My Game (Soft)",
                        "View My Average Ratings",
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
                case "View My Average Ratings":
                    // ShowAverageRatings(publisher);
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

}