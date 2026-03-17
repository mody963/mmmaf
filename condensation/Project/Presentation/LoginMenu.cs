using System;
using Spectre.Console;

public static class LoginMenu
{ 
    private static readonly AccountsLogic accountsLogic = new AccountsLogic();
    private static readonly CustomersLogic customerLogic = new CustomersLogic();

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
            SoundEffects.PlayMenuClick();

            switch (choice)
            {
                case var c when c == Texts.Get("Log_In"):
                    if (DoLogin()) exitMenu = true; 
                    break;

                case var c when c == Texts.Get("Create_Account"):
                    DoRegister();
                    break;

                case var c when c == Texts.Get("Log_Out"):
                    LogoutCurrentUser();
                    Console.ReadKey(true);
                    break;

                case var c when c == Texts.Get("Go_Back"):
                    exitMenu = true;
                    break;
            }
        }
    }

    private static void DoRegister()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold cyan]--- {Texts.Get("Create_Account")} ---[/]\n");

        // 1. Spectre.Console's built-in validation linked to Logic layer)
        string email = AnsiConsole.Prompt(
            new TextPrompt<string>("Email:")
                .Validate(e => accountsLogic.IsValidEmail(e) 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[red]Invalid email format.[/]")));

        string firstName = AnsiConsole.Prompt(
            new TextPrompt<string>("First Name:")
                .Validate(n => accountsLogic.IsValidName(n) 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[red]Invalid name format.[/]")));

        string lastName = AnsiConsole.Prompt(
            new TextPrompt<string>("Last Name:")
                .Validate(n => accountsLogic.IsValidName(n) 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[red]Invalid name format.[/]")));

        string password = AnsiConsole.Prompt(
            new TextPrompt<string>("Password (min 8 chars, 1 special char):")
                .Secret() // Masks the input
                .Validate(p => accountsLogic.IsValidPassword(p) 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[red]Password must be 8+ chars and contain a special character (!@#$%^&*()).[/]")));

        // 2. Customer Info
        string address = AnsiConsole.Prompt(
            new TextPrompt<string>("Address:")
                .Validate(a => customerLogic.IsValidAddress(a) 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[red]Address must be at least 6 characters.[/]")));

        // SelectionPrompt no validation needed
        string paymentMethod = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select your preferred [green]Payment Method[/]:")
                .AddChoices("IBAN", "CreditCard", "PayPal")
        );
        SoundEffects.PlayMenuClick();

        // 3. Save the Account
        AccountModel newAccount = new AccountModel
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Password = password,
            Role = AccountRoles.Customer, // Default to Customer
            IsActive = true
        };

        int accountId;
        try
        {
            // accountsLogic checks if email exists and throws an error if it does
            accountId = accountsLogic.CreateAccount(newAccount);
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"\n[red]Registration failed: {ex.Message}[/]");
            AnsiConsole.MarkupLine("Press any key to return...");
            Console.ReadKey(true);
            return; // cancels registration if email already used
        }

        // 4. Save the Customer details linked to the new Account ID
        CustomerModel newCustomer = new CustomerModel
        {
            AccountId = accountId,
            Address = address,
            PaymentMethod = paymentMethod.ToLower()
        };
        
        customerLogic.CreateCustomer(newCustomer);

        AnsiConsole.MarkupLine($"\n[green]Account successfully created! You can now log in, {firstName}.[/]");
        AnsiConsole.MarkupLine("Press any key to return to the menu...");
        Console.ReadKey(true);
    }



    private static bool DoLogin() 
    {
        if (CurrentUserModel.CurrentUser != null)
        {
            AnsiConsole.MarkupLine($"\n[blue]{Texts.Get("Login_AlreadyLoggedIn")} {CurrentUserModel.CurrentUser.FirstName}.[/]");
            return true;
        }

        int attempts = 0;
        const int maxAttempts = 3;

        while (attempts < maxAttempts)
        {
            AnsiConsole.Clear();

            string email = AnsiConsole.Prompt(
                new TextPrompt<string>(Texts.Get("Login_Email"))
            );

            string password = AnsiConsole.Prompt(
                new TextPrompt<string>(Texts.Get("Login_Password"))
                    .Secret() // Masks the input
            );

            var account = accountsLogic.CheckLogin(email, password); 

            if (account != null)
            {
                CurrentUserModel.CurrentUser = account;

                if (account.Role == AccountRoles.Admin)
                {
                    AnsiConsole.MarkupLine($"[green]{Texts.Get("Login_WelcomeAdmin")} {account.FirstName}! {Texts.Get("Login_AdminSuffix")}[/]");
                    AdminMenu.Start(); // Open admin menu
                    return true;       
                }

                AnsiConsole.MarkupLine($"[green]{Texts.Get("Login_Welcome")} {account.FirstName}![/]");
                Console.ReadKey(true);
                return true;
            }
            else
            {
                attempts++;
                AnsiConsole.MarkupLine($"[red]{Texts.Get("Login_IncorrectCredentials")} {maxAttempts - attempts}[/]");

                if (attempts >= maxAttempts)
                {
                    AnsiConsole.MarkupLine($"[red]{Texts.Get("Login_TooManyAttempts")}[/]");
                    return true; 
                }

                AnsiConsole.MarkupLine(Texts.Get("Login_PressEnterToRetry"));
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
            AnsiConsole.MarkupLine($"\n[blue]{CurrentUserModel.CurrentUser.FirstName} {Texts.Get("Login_LoggedOut")}[/]");
            CurrentUserModel.CurrentUser = null;
        }
        else
        {
            AnsiConsole.MarkupLine($"\n[blue]{Texts.Get("Login_NoUserLoggedIn")}[/]");
        }
    }
}