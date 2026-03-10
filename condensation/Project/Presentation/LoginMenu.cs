using System;
using Spectre.Console;

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
                    .Title($"[bold yellow]{Texts.Get("Login_Menu")}[/]")
                    .AddChoices(
                        Texts.Get("Log_In"),
                        Texts.Get("Create_Account"),
                        Texts.Get("Log_Out"),
                        Texts.Get("Go_Back")
                    )
                    .HighlightStyle(new Style(foreground: Color.Green))
            );

            switch (choice)
            {
                case var c when c == Texts.Get("Log_In"):
                    // 3. Pass it down into the DoLogin method
                    if (DoLogin()) exitMenu = true; 
                    break;

                case var c when c == Texts.Get("Create_Account"):
                    AnsiConsole.MarkupLine("[yellow]Registration not implemented yet.[/]");
                    break;

                case var c when c == Texts.Get("Log_Out"):
                    LogoutCurrentUser();
                    break;

                case var c when c == Texts.Get("Go_Back"):
                    exitMenu = true;
                    break;
            }
        }
    }

    // 4. Ask for it here too!
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

            // 5. Now it uses the fully connected accountsLogic!
            var account = accountsLogic.CheckAdminLogin(email, password); 

            if (account != null)
            {
                CurrentUserModel.CurrentUser = account;

                if (account.Role == AccountRoles.Admin)
                {
                    AnsiConsole.MarkupLine($"[green]Welcome back {account.FirstName}! (Admin)[/]");
                    AdminMenu.Start(); // Open admin menu
                    return true;       
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
                    return true; 
                }

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