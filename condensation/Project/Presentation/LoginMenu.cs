using System;
using Spectre.Console;

public static class LoginMenu
{ 
    private static readonly AccountsLogic accountsLogic = new AccountsLogic();
    private static readonly CustomersLogic customerLogic = new CustomersLogic();
    private static readonly PublisherLogic publisherLogic = new PublisherLogic();

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
                        "Apply as Publisher",
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
                case "Apply as Publisher":
                    DoPublisherRegister();
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

        // 1. Email
        string email;
        do
        {
            email = AnsiConsole.Prompt(new TextPrompt<string>("Email:"));
            if (!accountsLogic.IsValidEmail(email))
            {
                SoundEffects.PlayErrorSound();
                AnsiConsole.MarkupLine("[red]Invalid email format.[/]");
            }
        } while (!accountsLogic.IsValidEmail(email));

        // 2. First Name
        string firstName;
        do
        {
            firstName = AnsiConsole.Prompt(new TextPrompt<string>("First Name:"));
            if (!accountsLogic.IsValidName(firstName))
            {
                SoundEffects.PlayErrorSound();
                AnsiConsole.MarkupLine("[red]Invalid name format.[/]");
            }
        } while (!accountsLogic.IsValidName(firstName));

        // 3. Last Name
        string lastName;
        do
        {
            lastName = AnsiConsole.Prompt(new TextPrompt<string>("Last Name:"));
            if (!accountsLogic.IsValidName(lastName))
            {
                SoundEffects.PlayErrorSound();
                AnsiConsole.MarkupLine("[red]Invalid name format.[/]");
            }
        } while (!accountsLogic.IsValidName(lastName));

        // 4. Password
        string password;
        do
        {
            password = AnsiConsole.Prompt(
                new TextPrompt<string>("Password (min 8 chars, 1 special char):")
                    .Secret() // Masks the input
            );
            if (!accountsLogic.IsValidPassword(password))
            {
                SoundEffects.PlayErrorSound();
                AnsiConsole.MarkupLine("[red]Password must be 8+ chars and contain a special character (!@#$%^&*()).[/]");
            }
        } while (!accountsLogic.IsValidPassword(password));

        // 5. Address
        string address;
        do
        {
            address = AnsiConsole.Prompt(new TextPrompt<string>("Address:"));
            if (!customerLogic.IsValidAddress(address))
            {
                SoundEffects.PlayErrorSound();
                AnsiConsole.MarkupLine("[red]Address must be at least 6 characters.[/]");
            }
        } while (!customerLogic.IsValidAddress(address));

        // 6. Payment Method
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

    private static void DoPublisherRegister()
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[bold cyan]--- Apply as a Publisher ---[/]\n");

        string email = AnsiConsole.Prompt(
            new TextPrompt<string>("Contact Email:")
                .Validate(e => accountsLogic.IsValidEmail(e) 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[red]Invalid email format.[/]")));

        string firstName = AnsiConsole.Prompt(new TextPrompt<string>("Representative First Name:"));
        string lastName = AnsiConsole.Prompt(new TextPrompt<string>("Representative Last Name:"));

        string password = AnsiConsole.Prompt(
            new TextPrompt<string>("Password (min 8 chars, 1 special char):")
                .Secret() 
                .Validate(p => accountsLogic.IsValidPassword(p) 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[red]Password must be 8+ chars and contain a special character (!@#$%^&*()).[/]")));

        // 2. Publisher specific info
        string studioName = AnsiConsole.Prompt(
            new TextPrompt<string>("Studio Name:")
                .Validate(s => publisherLogic.IsValidStudioName(s) 
                    ? ValidationResult.Success() 
                    : ValidationResult.Error("[red]Invalid or already taken Studio Name.[/]")));

        // 3. Create Account: starts with isactive false cause we need approval from admin
        AccountModel newAccount = new AccountModel
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Password = password,
            Role = AccountRoles.Publisher, 
            IsActive = false
        };

        int accountId;
        try
        {
            accountId = accountsLogic.CreateAccount(newAccount);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"\n[red]Registration failed: {ex.Message}[/]");
            AnsiConsole.MarkupLine("Press any key to return...");
            Console.ReadKey(true);
            return;
        }

        // 4. Create the Publisher profile
        PublisherModel newPublisher = new PublisherModel
        {
            AccountId = accountId,
            StudioName = studioName,
            AmountOfGames = 0
        };
        
        publisherLogic.CreatePublisher(newPublisher);

        AnsiConsole.MarkupLine("\n[green]A request has been sent to the admin for approval.[/]");
        AnsiConsole.MarkupLine("[grey]You will be able to log in once an administrator approves your studio.[/]");
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

            if (!accountsLogic.IsValidEmail(email))
            {
                SoundEffects.PlayErrorSound();
                attempts++;
                AnsiConsole.MarkupLine($"[red]Invalid email format. Attempts left: {maxAttempts - attempts}[/]");

                if (attempts >= maxAttempts)
                {
                    SoundEffects.PlayErrorSound();
                    AnsiConsole.MarkupLine($"[red]{Texts.Get("Login_TooManyAttempts")}[/]");
                    return true;
                }

                AnsiConsole.MarkupLine(Texts.Get("Login_PressEnterToRetry"));
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                    return false;
                continue;
            }

            string password = AnsiConsole.Prompt(
                new TextPrompt<string>(Texts.Get("Login_Password"))
                    .Secret() // Masks the input
            );

            AccountModel? account = null;
            try
            {
                account = accountsLogic.CheckLogin(email, password); 
            }
            catch (UnauthorizedAccessException ex)
            {
                AnsiConsole.MarkupLine($"\n[yellow]{ex.Message}[/]");
                AnsiConsole.MarkupLine("Press any key to return to the menu...");
                Console.ReadKey(true);
                return false;
            }

            if (account != null)
            {
                CurrentUserModel.CurrentUser = account;

                if (account.Role == AccountRoles.Admin)
                {
                    AnsiConsole.MarkupLine($"[green]{Texts.Get("Login_WelcomeAdmin")} {account.FirstName}! {Texts.Get("Login_AdminSuffix")}[/]");
                    AdminMenu.Start(); // Open admin menu
                    return true;       
                }

                if (account.Role == AccountRoles.Publisher)
                {
                    var publisher = publisherLogic.GetByAccountId(account.Id);
                    if (publisher != null && !account.IsActive)
                    {
                        AnsiConsole.MarkupLine($"\n[yellow]{Texts.Get("Login_PublisherPending")}[/]");
                        AnsiConsole.MarkupLine("Press any key to return to the menu...");
                        Console.ReadKey(true);
                        return false;
                    }
                    else if (publisher != null && account.IsActive)
                    {
                        AnsiConsole.MarkupLine($"[green]{Texts.Get("Login_WelcomePublisher")} {account.FirstName}! {Texts.Get("Login_PublisherSuffix")}[/]");
                        PublisherMenu.Start();
                        return true;       
                    }
                }

                AnsiConsole.MarkupLine($"[green]{Texts.Get("Login_Welcome")} {account.FirstName}![/]");
                Console.ReadKey(true);
                return true;
            }
            else
            {
                SoundEffects.PlayErrorSound();
                attempts++;
                AnsiConsole.MarkupLine($"[red]{Texts.Get("Login_IncorrectCredentials")} {maxAttempts - attempts}[/]");

                if (attempts >= maxAttempts)
                {
                    SoundEffects.PlayErrorSound();
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