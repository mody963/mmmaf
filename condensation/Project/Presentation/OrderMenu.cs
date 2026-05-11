using Spectre.Console;
using System.Globalization;

public static class OrderMenu
{
    private static readonly OrderLogic _orderLogic = new OrderLogic();
    private static readonly CustomersLogic _customersLogic = new CustomersLogic();

    public static void Start()
    {
        var currentUser = CurrentUserModel.CurrentUser;

        if (currentUser == null || currentUser.Role != AccountRoles.Customer)
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("Order_NotLoggedInCustomer")}[/]");
            Console.ReadKey(true);
            return;
        }

        var customer = _customersLogic.GetByAccountId(currentUser.Id);
        if (customer == null)
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("Order_NoCustomerProfile")}[/]");
            Console.ReadKey(true);
            return;
        }

        ShowOrderHistory(customer.Id);
    }

    private static void ShowOrderHistory(int customerId)
    {
        while (true)
        {
            AnsiConsole.Clear();

            var orders = _orderLogic.GetOrderHistory(customerId);

            if (orders.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{Texts.Get("Order_NoOrderHistory")}[/]");
                Console.ReadKey(true);
                return;
            }

            var prompt = new SelectionPrompt<OrderModel>()
                .Title($"[bold]{Texts.Get("Order_SelectOrder")}[/]")
                .UseConverter(o => o.Id == -1 
                    ? Texts.Get("Order_Back") 
                    : $"Order #{o.Id} - {o.OrderDate:yyyy-MM-dd} - EUR {o.TotalPrice:0.00}")
                .HighlightStyle(new Style(foreground: Color.Green));

            foreach (var order in orders)
                prompt.AddChoice(order);

            // go back optie. 
            prompt.AddChoice(new OrderModel { Id = -1 });

            var selectedOrder = AnsiConsole.Prompt(prompt);
            SoundEffects.PlayMenuClick();

            if (selectedOrder.Id == -1)
                return;

            ShowOrderDetails(selectedOrder);
        }
    }

    private static void ShowOrderDetails(OrderModel order)
    {
        while (true)
        {
            AnsiConsole.Clear();
            var fullOrder = _orderLogic.GetOrderDetails(order.Id);

            if (fullOrder == null)
            {
                AnsiConsole.MarkupLine($"[red]{Texts.Get("Order_NotFound")}[/]");
                Console.ReadKey(true);
                return;
            }

            AnsiConsole.MarkupLine($"[bold cyan]{Texts.Get("Order_Details")}[/]");
            AnsiConsole.WriteLine();

            var headerTable = new Table().Border(TableBorder.Rounded);
            headerTable.AddColumn(Texts.Get("Order_Property"));
            headerTable.AddColumn(Texts.Get("Order_Value"));
            headerTable.AddRow(Texts.Get("Order_OrderId"), $"#{fullOrder.Id}");
            headerTable.AddRow(Texts.Get("Order_OrderDate"), fullOrder.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"));
            headerTable.AddRow(Texts.Get("Order_TotalPrice"), $"EUR {fullOrder.TotalPrice:0.00}");

            AnsiConsole.Write(headerTable);
            AnsiConsole.WriteLine();




            AnsiConsole.MarkupLine($"[bold cyan]{Texts.Get("Order_Games")}[/]");

            var gamesTable = new Table().Border(TableBorder.Rounded);
            gamesTable.AddColumn(Texts.Get("Order_GameName"));
            gamesTable.AddColumn(Texts.Get("Order_GamePrice"));

            double calculatedTotal = 0;
            foreach (var game in fullOrder.Games)
            {
                calculatedTotal += game.PriceAtPurchase;

                gamesTable.AddRow(
                    Markup.Escape(game.GameTitle),
                    $"EUR {game.PriceAtPurchase:0.00}"
                );
            }

            AnsiConsole.Write(gamesTable);
            AnsiConsole.WriteLine();


            var backPrompt = new SelectionPrompt<string>()
                .Title($"[bold]{Texts.Get("Order_WhatWouldYouLikeToDo")}[/]")
                .AddChoices(Texts.Get("Order_Back"))
                .HighlightStyle(new Style(foreground: Color.Green));

            AnsiConsole.Prompt(backPrompt);
            SoundEffects.PlayMenuClick();
            return;
        }
    }
}
