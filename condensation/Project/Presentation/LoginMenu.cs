using Spectre.Console;
using System.Text;

public static class LoginMenu
{
    private static readonly AccountsLogic accountsLogic = new AccountsLogic();

    public static void Start()
    {
        bool exitMenu = false;

        while (!exitMenu)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Login Menu[/]")
                    .AddChoices(
                        "Log in",
                        "Create account",
                        "Log out",
                        "Go Back"
                    )
                    .HighlightStyle(new Style(foreground: Color.Green))
            );

            switch (choice)
            {
                case "Log in":
                    if (DoLogin()) exitMenu = true; // Exit menu after login success
                    
                    break;

                case "Create account":
                    // TODO: Implement registration later
                    AnsiConsole.MarkupLine("[yellow]Registration not implemented yet.[/]");
                    break;

                case "Log out":
                    LogoutCurrentUser();
                    break;

                case "Go Back":
                    exitMenu = true;
                    break;
            }
        }
    }

    private static bool DoLogin()
    {
        if (CurrentUserModel.CurrentUser != null)
        {
            AnsiConsole.MarkupLine($"\n[blue]Already logged in as {CurrentUserModel.CurrentUser.FirstName}.[/]");
            return true;
        }

        int attempts = 0;
        const int maxAttempts = 3;

        while (attempts < maxAttempts)
        {
            AnsiConsole.Clear();

            string email = AnsiConsole.Prompt(
                new TextPrompt<string>("Email:")
            );

            string password = AnsiConsole.Prompt(
                new TextPrompt<string>("Password:")
                    .Secret() // Masks the input
            );

            var account = accountsLogic.CheckAdminLogin(email, password); // Admin-only for now

            if (account != null)
            {
                CurrentUserModel.CurrentUser = account;

                if (account.Role == AccountRoles.Admin)
                {
                    AnsiConsole.MarkupLine($"[green]Welcome back {account.FirstName}! (Admin)[/]");
                    AdminMenu.Start(); // Open admin menu
                    return true;       // Exit login menu after admin logs out
                }

                AnsiConsole.MarkupLine($"[green]Welcome back {account.FirstName}![/]");
                Console.ReadKey(true);
                return true;
            }
            else
            {
                attempts++;
                AnsiConsole.MarkupLine($"[red]Incorrect email or password. Attempts left: {maxAttempts - attempts}[/]");

                if (attempts >= maxAttempts)
                {
                    AnsiConsole.MarkupLine("[red]Too many failed attempts. Returning to main menu...[/]");
                    return true; // Return to main menu after 3 failed tries
                }

                // Optional: allow escape to go back early
                AnsiConsole.MarkupLine("Press Enter to try again or ESC to go back.");
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                    return false;
            }
        }

        return false;
    }

    private static void LogoutCurrentUser()
    {
        if (CurrentUserModel.CurrentUser != null)
        {
            AnsiConsole.MarkupLine($"\n[blue]{CurrentUserModel.CurrentUser.FirstName} has been logged out.[/]");
            CurrentUserModel.CurrentUser = null;
        }
        else
        {
            AnsiConsole.MarkupLine("\n[blue]No user is currently logged in.[/]");
        }
    }
}