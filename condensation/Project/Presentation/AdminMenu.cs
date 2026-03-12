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
        string description = AnsiConsole.Prompt(new TextPrompt<string>($"{Texts.Get("Description")}:"));
        double price;
        do
        {
            price = AnsiConsole.Prompt(new TextPrompt<double>($"{Texts.Get("Price")}:"));
        }
        while (price <= 0);
        // placeholder for genre, publisher and agerating till they are implemented.
        int publisherId = AnsiConsole.Prompt(new TextPrompt<int>($"{Texts.Get("Publisher_ID")}:"));
        var genres = _gameLogic.GetAllGenres();
        
        var selectedGenre = AnsiConsole.Prompt(
            new SelectionPrompt<GenreModel>()
                .Title($"{Texts.Get("Select_Genre")}:")
                .UseConverter(g => g.Name) // Tells the menu to only display the Name text
                .AddChoices(genres)
        );

        var ageRatings = _gameLogic.GetAllAgeRatings();
        var selectedAgeRating = AnsiConsole.Prompt(
            new SelectionPrompt<AgeRatingModel>()
                .Title($"{Texts.Get("Select_Age_Rating")}:")
                .UseConverter(a => a.Name)
                .AddChoices(ageRatings)
        );

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
        
        var results = _gameLogic.SearchGamesByTitle(searchTitle);
        if (onlyActive)
        {
            results = results.Where(g => g.IsActive).ToList();
        }

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("No_games_found")}[/]");
            return null;
        }

        // Let them pick from the search results
        return AnsiConsole.Prompt(
            new SelectionPrompt<GameModel>()
                .Title($"{Texts.Get("Game_SelectDetails")} {action.ToLower()}")
                .UseConverter(g => $"{g.Title} (${g.Price}) - Active: {g.IsActive}")
                .AddChoices(results)
        );
    }
    private static void UpdateGameMenu()
    {
        var game = SearchAndSelectGame("Update a Game");
        if (game == null) { Console.ReadKey(true); return; }

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"\n[bold yellow]Updating: {game.Title}[/]");
        AnsiConsole.MarkupLine("[grey](Press Enter to keep the current text values)[/]\n");

        game.Title = AnsiConsole.Prompt(new TextPrompt<string>("Title:").DefaultValue(game.Title));
        game.Description = AnsiConsole.Prompt(new TextPrompt<string>("Description:").DefaultValue(game.Description));
        game.Price = AnsiConsole.Prompt(new TextPrompt<double>("Price:").DefaultValue(game.Price));

        // 2. Dropdown for Genre
        var genres = _gameLogic.GetAllGenres();
        var currentGenre = genres.FirstOrDefault(g => g.Id == game.GenreId); // Find the current one
        
        var selectedGenre = AnsiConsole.Prompt(
            new SelectionPrompt<GenreModel>()
                .Title($"Select a [green]Genre[/] (Current: [yellow]{currentGenre?.Name}[/]):")
                .UseConverter(g => g.Name)
                .AddChoices(genres)
        );
        game.GenreId = selectedGenre.Id;

        var ageRatings = _gameLogic.GetAllAgeRatings();
        var currentAgeRating = ageRatings.FirstOrDefault(a => a.Id == game.AgeRatingId);
        
        var selectedAgeRating = AnsiConsole.Prompt(
            new SelectionPrompt<AgeRatingModel>()
                .Title($"Select an [green]Age Rating[/] (Current: [yellow]{currentAgeRating?.Name}[/]):")
                .UseConverter(a => a.Name)
                .AddChoices(ageRatings)
        );
        game.AgeRatingId = selectedAgeRating.Id;

        game.IsActive = AnsiConsole.Confirm(Texts.Get("Admin_GameActivePrompt"), defaultValue: game.IsActive);

        _gameLogic.UpdateGame(game);

        AnsiConsole.MarkupLine($"\n[green]Successfully updated and saved '{game.Title}'![/]");
        AnsiConsole.MarkupLine("Press any key to return...");
        Console.ReadKey(true);
    }
    private static void DeleteGameMenu()
    {
        var game = SearchAndSelectGame("Deactivate a Game", true);
        if (game == null) { Console.ReadKey(true); return; }

        if (AnsiConsole.Confirm($"\nAre you sure you want to deactivate (soft-delete) '[red]{game.Title}[/]'?"))
        {
            _gameLogic.SoftDeleteGame(game.Id);
            AnsiConsole.MarkupLine($"\n[green]'{game.Title}' has been deactivated.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("\n[grey]Deactivation cancelled.[/]");
        }
        
        AnsiConsole.MarkupLine("Press any key to return...");
        Console.ReadKey(true);
    }

    private static void Logout()
    {
        AnsiConsole.Clear();
        CurrentUserModel.CurrentUser = null;
        AnsiConsole.MarkupLine("\n[green]You have been logged out.[/]");
        AnsiConsole.MarkupLine("Press any key to return to the main menu...");
        Console.ReadKey(true);
    }
}