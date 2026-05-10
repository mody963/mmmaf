using MongoDB.Bson;
using Spectre.Console;

public static class CheckoutMenu
{
    private static readonly CheckoutLogic _checkoutLogic = new CheckoutLogic();
    private static readonly CustomerAccess _customerAccess = new CustomerAccess();
    private static readonly CustomersLogic _customersLogic = new CustomersLogic();

    public static void Start(Cart cart)
    {
        AnsiConsole.Clear();

        List<CartModel> items = cart.GetCartItems();

        if (items == null || items.Count == 0)
        {
            SoundEffects.PlayErrorSound();
            AnsiConsole.MarkupLine("[red]Your cart is empty.[/]");
            Pause();
            return;
        }

        if (CurrentUserModel.CurrentUser != null)
        {
            CheckoutLoggedIn(cart, items);
            return;
        }

        CheckoutNotLoggedIn(cart, items);
    }

    private static void CheckoutNotLoggedIn(Cart cart, List<CartModel> items)
    {
        while (true)
        {
            AnsiConsole.Clear();
            ShowReceipt(items, "Not logged in", "Not selected yet");

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Checkout[/]")
                    .AddChoices(
                        "Log in",
                        "Continue as guest",
                        "Back")
            );

            SoundEffects.PlayMenuClick();

            switch (choice)
            {
                case "Log in":
                    LoginMenu.Start();

                    if (CurrentUserModel.CurrentUser != null)
                    {
                        CheckoutLoggedIn(cart, cart.GetCartItems());
                    }

                    return;

                case "Continue as guest":
                    CheckoutGuest(cart, items);
                    return;

                case "Back":
                    return;
            }
        }
    }

    private static void CheckoutLoggedIn(Cart cart, List<CartModel> items)
    {
        var account = CurrentUserModel.CurrentUser;

        if (account == null)
        {
            SoundEffects.PlayErrorSound();
            AnsiConsole.MarkupLine("[red]No logged in account found.[/]");
            Pause();
            return;
        }

        var customer = _customerAccess.GetByAccountId(account.Id);

        if (customer == null)
        {
            UserActionLogger.Log(
                actionType: "error",
                objectType: "checkout",
                details: new BsonDocument
                {
                    { "message", "No customer profile found for this account." },
                    { "accountId", account.Id },
                    { "email", account.Email }
                }
            );

            SoundEffects.PlayErrorSound();
            AnsiConsole.MarkupLine("[red]No customer profile found for this account.[/]");
            Pause();
            return;
        }

        AnsiConsole.Clear();
        ShowReceipt(items, account.Email, customer.PaymentMethod);

        bool confirm = AnsiConsole.Confirm("Confirm order?");

        if (!confirm)
        {
            return;
        }

        try
        {
            int orderId = _checkoutLogic.Checkout(customer.Id, items);

            var itemDocuments = new BsonArray();

            foreach (var item in items)
            {
                itemDocuments.Add(new BsonDocument
                {
                    { "id", item.id },
                    { "name", item.Name },
                    { "price", item.Price }
                });
            }

            UserActionLogger.Log(
                actionType: "complete_order",
                objectType: "order",
                objectId: orderId.ToString(),
                details: new BsonDocument
                {
                    { "customerId", customer.Id },
                    { "email", account.Email },
                    { "paymentMethod", customer.PaymentMethod },
                    { "itemCount", items.Count },
                    { "total", items.Sum(item => item.Price) },
                    { "items", itemDocuments }
                }
            );

            cart.ClearCart();

            SoundEffects.PlayKaching();
            AnsiConsole.MarkupLine($"\n[green]Order confirmed.[/]");
            AnsiConsole.MarkupLine($"[grey]Order ID:[/] [yellow]{orderId}[/]");

            ReturnToMainMenu();
        }
        catch (Exception ex)
        {
            UserActionLogger.LogError(
                message: "Checkout failed.",
                source: "CheckoutMenu.CheckoutLoggedIn",
                exception: ex,
                details: new BsonDocument
                {
                    { "customerId", customer.Id },
                    { "email", account.Email },
                    { "itemCount", items.Count },
                    { "total", items.Sum(item => item.Price) }
                }
            );

            SoundEffects.PlayErrorSound();
            AnsiConsole.MarkupLine($"[red]Checkout failed:[/] {ex.Message}");
            Pause();
        }
    }

    private static void CheckoutGuest(Cart cart, List<CartModel> items)
    {
        AnsiConsole.Clear();

        string email = PromptGuestEmail();
        string paymentMethod = PromptGuestPaymentMethod();

        AnsiConsole.Clear();
        ShowReceipt(items, email, paymentMethod);

        bool confirm = AnsiConsole.Confirm("Confirm order?");

        if (!confirm)
        {
            return;
        }

        cart.ClearCart();

        SoundEffects.PlayKaching();
        AnsiConsole.MarkupLine("\n[green]Order confirmed.[/]");
        AnsiConsole.MarkupLine("[grey]Guest checkout completed.[/]");
        AnsiConsole.MarkupLine("[grey]Receipt displayed above.[/]");

        ReturnToMainMenu();
    }

    private static string PromptGuestEmail()
    {
        while (true)
        {
            string email = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter your [green]email[/]:")
                    .PromptStyle("yellow")
            );

            if (_customersLogic.IsValidEmail(email))
            {
                return email;
            }

            SoundEffects.PlayErrorSound();
            AnsiConsole.MarkupLine("[red]Please enter a valid email address.[/]");
        }
    }

    private static string PromptGuestPaymentMethod()
    {
        while (true)
        {
            string paymentMethod = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select your [green]payment method[/]:")
                    .AddChoices("IBAN", "CreditCard", "PayPal")
            );

            if (_customersLogic.IsValidPaymentMethod(paymentMethod))
            {
                return paymentMethod;
            }

            SoundEffects.PlayErrorSound();
            AnsiConsole.MarkupLine("[red]Invalid payment method.[/]");
        }
    }

    private static void ShowReceipt(List<CartModel> items, string email, string paymentMethod)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold yellow]Receipt[/]")
            .AddColumn("[cyan]Game[/]")
            .AddColumn("[cyan]Price[/]")
            .AddColumn("[cyan]Added[/]");

        foreach (var item in items)
        {
            table.AddRow(
                item.Name,
                $"€{item.Price:F2}",
                item.DateAdded.ToString("dd-MM-yyyy")
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        double total = items.Sum(item => item.Price);

        var summary = new Panel(
            $"[bold]Email:[/] {email}\n" +
            $"[bold]Payment:[/] {paymentMethod}\n" +
            $"[bold green]Total:[/] €{total:F2}"
        )
        .Header("Order Summary")
        .Border(BoxBorder.Rounded);

        AnsiConsole.Write(summary);
        AnsiConsole.WriteLine();
    }

    private static void ReturnToMainMenu()
    {
        bool returnToMenu = AnsiConsole.Confirm("Return to main menu?");

        if (!returnToMenu)
        {
            Pause();
        }

        AnsiConsole.Clear();
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}