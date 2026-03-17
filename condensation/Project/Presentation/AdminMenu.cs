using Spectre.Console;

public static class AdminMenu
{
    private static readonly GameLogic _gameLogic = new GameLogic();
    public static void Start()
    {
        bool exitMenu = false;

        while (!exitMenu)
        {
            AnsiConsole.Clear();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold green]{Texts.Get("Admin_Menu")}[/]")
                    .AddChoices(
                        Texts.Get("Add_Game"),
                        Texts.Get("Update_Game"),
                        Texts.Get("Delete_Game"),
                        Texts.Get("Log_Out")
                    )
                    .HighlightStyle(new Style(foreground: Color.Yellow))
            );

            SoundEffects.PlayMenuClick();

            switch (choice)
            {
                case var c when c == Texts.Get("Add_Game"):
                    AddGameMenu();
                    break;

                case var c when c == Texts.Get("Update_Game"):
                    UpdateGameMenu();
                    break;

                case var c when c == Texts.Get("Delete_Game"):
                    DeleteGameMenu();
                    break;

                case var c when c == Texts.Get("Log_Out"):
                    Logout();
                    exitMenu = true; // Return to login/main menu
                    break;
            }
        }
    }
    private static void AddGameMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold cyan]{Texts.Get("Add_Game_Menu_Title")}[/]");

        string title = AnsiConsole.Prompt(new TextPrompt<string>($"{Texts.Get("Title")}:"));
        SoundEffects.PlayMenuClick();
        string description = AnsiConsole.Prompt(new TextPrompt<string>($"{Texts.Get("Description")}:"));
        SoundEffects.PlayMenuClick();
        double price;
        do
        {
            price = AnsiConsole.Prompt(new TextPrompt<double>($"{Texts.Get("Price")}:"));
            if (price <= 0)
            {
                SoundEffects.PlayErrorSound();
            }
            else
            {
                SoundEffects.PlayMenuClick();
            }
        }
        while (price <= 0);
        // placeholder for genre, publisher and agerating till they are implemented.
        int publisherId = AnsiConsole.Prompt(new TextPrompt<int>($"{Texts.Get("Publisher_ID")}:"));
        SoundEffects.PlayMenuClick();
        var genres = _gameLogic.GetAllGenres();
        
        var selectedGenre = AnsiConsole.Prompt(
            new SelectionPrompt<GenreModel>()
                .Title($"{Texts.Get("Select_Genre")}:")
                .UseConverter(g => g.Name) // Tells the menu to only display the Name text
                .AddChoices(genres)
        );
        SoundEffects.PlayMenuClick();

        var ageRatings = _gameLogic.GetAllAgeRatings();
        var selectedAgeRating = AnsiConsole.Prompt(
            new SelectionPrompt<AgeRatingModel>()
                .Title($"{Texts.Get("Select_Age_Rating")}:")
                .UseConverter(a => a.Name)
                .AddChoices(ageRatings)
        );
        SoundEffects.PlayMenuClick();

        GameModel newGame = new GameModel
        {
            Title = title,
            Description = description,
            Price = price,
            PublisherId = publisherId,
            GenreId = selectedGenre.Id,
            AgeRatingId = selectedAgeRating.Id,
        };

        _gameLogic.AddGame(newGame);

        AnsiConsole.MarkupLine($"[green]{Texts.Get("Successfully_Added")} '{title}' {Texts.Get("To_The_Database")}![/]");
        AnsiConsole.MarkupLine($"\n{Texts.Get("Press_Any_Key_To_Return")}");
        Console.ReadKey(true);
    }
    private static GameModel? SearchAndSelectGame(string action, bool onlyActive = false)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"\n[bold cyan]--- {action} ---[/]");
        string searchTitle = AnsiConsole.Prompt(new TextPrompt<string>(Texts.Get("Search_Title"))); 
        SoundEffects.PlayMenuClick();
        
        var results = _gameLogic.SearchGamesByTitle(searchTitle);
        if (onlyActive)
        {
            results = results.Where(g => g.IsActive).ToList();
        }

        if (results.Count == 0)
        {
            SoundEffects.PlayErrorSound();
            AnsiConsole.MarkupLine($"[red]{Texts.Get("No_games_found")}[/]");
            return null;
        }

        // Let them pick from the search results
        var selectedGame = AnsiConsole.Prompt(
            new SelectionPrompt<GameModel>()
                .Title($"{Texts.Get("Game_SelectDetails")} {action.ToLower()}")
                .UseConverter(g => $"{g.Title} (${g.Price}) - Active: {g.IsActive}")
                .AddChoices(results)
        );
        SoundEffects.PlayMenuClick();
        return selectedGame;
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
    private static void DeleteGameMenu()
    {
        var game = SearchAndSelectGame(Texts.Get("Admin_DeactivateGame"), true);
        if (game == null) { SoundEffects.PlayErrorSound(); Console.ReadKey(true); return; }

        var confirmed = AnsiConsole.Confirm($"\n{Texts.Get("Admin_ConfirmDeactivate")} '[red]{game.Title}[/]'?");
        SoundEffects.PlayMenuClick();
        if (confirmed)
        {
            _gameLogic.SoftDeleteGame(game.Id);
            AnsiConsole.MarkupLine($"\n[green]'{game.Title}' {Texts.Get("Admin_GameDeactivated")}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"\n[grey]{Texts.Get("Admin_DeactivationCancelled")}[/]");
        }
        
        AnsiConsole.MarkupLine(Texts.Get("Admin_PressAnyKeyToReturn"));
        Console.ReadKey(true);
    }

    private static void Logout()
    {
        AnsiConsole.Clear();
        CurrentUserModel.CurrentUser = null;
        AnsiConsole.MarkupLine($"\n[green]{Texts.Get("Admin_LoggedOut")}[/]");
        AnsiConsole.MarkupLine(Texts.Get("Admin_PressAnyKeyToReturnToMainMenu"));
        Console.ReadKey(true);
    }
}