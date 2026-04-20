using Spectre.Console;
using System.Threading;

public static class MyGamesMenu
{
    private static readonly CustomersLogic _customersLogic = new CustomersLogic();
    private static readonly ReviewLogic _reviewLogic = new ReviewLogic();
    private static readonly GameLogic _gameLogic = new GameLogic();
    private static readonly OrderLogic _orderLogic = new OrderLogic();

    public static void Start()
    {
        var currentUser = CurrentUserModel.CurrentUser;

        if (currentUser == null || currentUser.Role != AccountRoles.Customer)
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("MyGames_NotLoggedInCustomer")}[/]");
            Console.ReadKey(true);
            return;
        }

        var customer = _customersLogic.GetByAccountId(currentUser.Id);
        if (customer == null)
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("MyGames_NoCustomerProfile")}[/]");
            Console.ReadKey(true);
            return;
        }

        while (true)
        {
            AnsiConsole.Clear();
            var ownedGames = _orderLogic.GetOwnedGames(customer.Id);

            if (ownedGames.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{Texts.Get("MyGames_NoOwnedGames")}[/]");
                Console.ReadKey(true);
                return;
            }

            var prompt = new SelectionPrompt<GameModel>()
                .Title($"[bold]{Texts.Get("MyGames_SelectGame")}[/]")
                .UseConverter(g => g.Id == -1 ? Texts.Get("MyGames_Back") : g.Title)
                .HighlightStyle(new Style(foreground: Color.Green));

            foreach (var game in ownedGames)
                prompt.AddChoice(game);

            prompt.AddChoice(new GameModel { Id = -1, Title = Texts.Get("MyGames_Back") });

            var selectedGame = AnsiConsole.Prompt(prompt);
            SoundEffects.PlayMenuClick();

            if (selectedGame.Id == -1)
                return;

            ShowOwnedGameDetails(selectedGame, customer.Id);
        }
    }

    private static void ShowOwnedGameDetails(GameModel game, int customerId)
    {
        while (true)
        {
            AnsiConsole.Clear();

            var detailsTable = new Table().Border(TableBorder.Rounded);
            detailsTable.AddColumn(Texts.Get("Game_Property"));
            detailsTable.AddColumn(Texts.Get("Game_Value"));
            detailsTable.AddRow(Texts.Get("Title"), game.Title);
            detailsTable.AddRow(Texts.Get("Description"), string.IsNullOrWhiteSpace(game.Description) ? "<none>" : game.Description);
            detailsTable.AddRow(Texts.Get("Price"), $"EUR {game.Price:0.00}");

            var genreName = _gameLogic.GetAllGenres().FirstOrDefault(g => g.Id == game.GenreId)?.Name ?? "<unknown>";
            var ageName = _gameLogic.GetAllAgeRatings().FirstOrDefault(a => a.Id == game.AgeRatingId)?.Name ?? "<unknown>";
            detailsTable.AddRow(Texts.Get("Game_Genre"), genreName);
            detailsTable.AddRow(Texts.Get("Game_AgeRating"), ageName);

            AnsiConsole.Write(detailsTable);
            AnsiConsole.WriteLine();

            var reviews = _reviewLogic.GetReviewsForGame(game.Id);
            AnsiConsole.MarkupLine($"[bold cyan]{Texts.Get("MyGames_ReviewsHeader")}[/]");

            if (reviews.Count == 0)
            {
                AnsiConsole.MarkupLine($"[grey]{Texts.Get("MyGames_NoReviewsYet")}[/]");
            }
            else
            {
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
                        review.CreatedAt.ToString("yyyy-MM-dd"));
                }

                AnsiConsole.Write(reviewTable);
            }

            AnsiConsole.WriteLine();
            var ownReview = _reviewLogic.GetCustomerReviewForGame(customerId, game.Id);

            var choices = new List<string>();
            if (ownReview == null)
            {
                choices.Add(Texts.Get("MyGames_LeaveReview"));
            }
            else
            {
                choices.Add(Texts.Get("MyGames_EditReview"));
                choices.Add(Texts.Get("MyGames_DeleteReview"));
            }
            choices.Add(Texts.Get("MyGames_Back"));

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]{Texts.Get("Game_WhatWouldYouLikeToDo")}[/]")
                    .AddChoices(choices)
                    .HighlightStyle(new Style(foreground: Color.Green))
            );
            SoundEffects.PlayMenuClick();

            if (action == Texts.Get("MyGames_Back"))
                return;

            if (action == Texts.Get("MyGames_LeaveReview") || action == Texts.Get("MyGames_EditReview"))
            {
                LeaveOrEditReview(game.Id, customerId, ownReview);
            }
            else if (action == Texts.Get("MyGames_DeleteReview"))
            {
                if (AnsiConsole.Confirm($"[red]{Texts.Get("MyGames_DeleteConfirm")}[/]"))
                {
                    try
                    {
                        _reviewLogic.DeleteReviewWithAuth(
                            customerId,
                            (int)CurrentUserModel.CurrentUser!.Role,
                            ownReview!.Id,
                            game.Id
                        );
                        AnsiConsole.MarkupLine($"[green]{Texts.Get("MyGames_ReviewDeleted")}[/]");
                        SoundEffects.PlayMenuClick();
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
                        SoundEffects.PlayErrorSound();
                    }
                    Thread.Sleep(1000);
                }
            }
        }
    }

    private static void LeaveOrEditReview(int gameId, int customerId, ReviewModel? ownReview)
    {
        int rating = AnsiConsole.Prompt(
            new TextPrompt<int>(Texts.Get("MyGames_RatingPrompt"))
                .Validate(r => r >= 1 && r <= 10
                    ? ValidationResult.Success()
                    : ValidationResult.Error(Texts.Get("MyGames_RatingValidation")))
        );

        if (ownReview != null)
            AnsiConsole.MarkupLine($"[grey]{Texts.Get("MyGames_CurrentComment")} {Markup.Escape(ownReview.Comment)}[/]");

        string comment = AnsiConsole.Prompt(
            new TextPrompt<string>(Texts.Get("MyGames_CommentPrompt"))
                .Validate(c => _reviewLogic.IsValidComment(c)
                    ? ValidationResult.Success()
                    : ValidationResult.Error(Texts.Get("MyGames_CommentValidation")))
        );

        try
        {
            _reviewLogic.SaveReview(new ReviewModel
            {
                Id = ownReview != null ? ownReview.Id : 0,
                GameId = gameId,
                CustomerId = customerId,
                Rating = rating,
                Comment = comment,
                // Preserve the reviewer name if editing, or set it if new
                ReviewerName = ownReview?.ReviewerName ?? CurrentUserModel.CurrentUser?.FirstName ?? "Unknown"
            });

            AnsiConsole.MarkupLine($"[green]{Texts.Get("MyGames_ReviewSaved")}[/]");
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
        }

        Console.ReadKey(true);
    }
}
