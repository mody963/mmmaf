using System.Runtime.CompilerServices;
using Spectre.Console;

public class Cart
{
    private readonly CartLogic _cartLogic = new CartLogic();

    public void AddToCart(int id, string name, double price)
    {
        if (!_cartLogic.AddToCart(id, name, price))
        {
            AnsiConsole.MarkupLine($"[red]Item with id {id} is already in the cart.[/]");
            Console.ReadKey(true);
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]Item {name} added to cart for €{price:F2}.[/]");
            Console.ReadKey(true);
        }
    }

    public void RemoveFromCart(string name)
    {
        _cartLogic.RemoveFromCart(name);
    }

    public void ClearCart()
    {
        _cartLogic.ClearCart();
    }

    public List<CartModel> GetCartItems()
    {
        return _cartLogic.GetCartItems();
    }

    public double GetTotalPrice()
    {
        return _cartLogic.GetTotalPrice();
    }

    public void CartOptions()
    {
        var optie = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("[bold yellow]Select Cart Option:[/]")
        .AddChoices("view", "remove item", "clear cart", " back"));

        switch (optie)
        {
            case "view":
                ShowCart();
                break;
            case "remove item":
                var items = GetCartItems();

                if (items.Count == 0)
                {
                    AnsiConsole.MarkupLine("[red]Cart is empty.[/]");
                    Console.ReadKey(true);
                    break;
                }

                var selectedItem = AnsiConsole.Prompt(
                    new SelectionPrompt<CartModel>()
                        .Title("[yellow]Select an item to remove:[/]")
                        .UseConverter(item => $"{item.Name} - €{item.Price:F2}")
                        .AddChoices(items)
                        .HighlightStyle(new Style(foreground: Color.Red))
                );

                RemoveFromCart(selectedItem.Name);

                AnsiConsole.MarkupLine($"[green]{selectedItem.Name} removed from cart.[/]");
                Console.ReadKey(true);

                break;
            case "clear cart":
                ClearCart();
                AnsiConsole.MarkupLine("[green]Cart cleared.[/]");
                Console.ReadKey(true);
                break;
            case "back":
                return;
        }
    }

    private void ShowCart()
    {
        Console.Clear();

        var items = GetCartItems();
        var totalPrice = GetTotalPrice();

        if (items.Count == 0)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[red]Your cart is empty.[/]");
            AnsiConsole.MarkupLine("[grey]Press any key to return to menu...[/]");
            Console.ReadKey(true);
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold yellow]🛒 Your Shopping Cart[/]")
            .AddColumn("[cyan]Product[/]")
            .AddColumn("[cyan]Price[/]")
            .AddColumn("[cyan]Purchase Date[/]");

        foreach (var item in items)
        {
            table.AddRow(
                item.Name,
                $"[green]€{item.Price:F2}[/]",
                item.DateAdded.ToString("dd-MM-yyyy")
            );
        }

        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Your cart is empty.[/]");
            return;
        }

        AnsiConsole.Write(table);

        AnsiConsole.WriteLine();

        var totalPanel = new Panel(
            $"[bold green]Total Price:[/] €{totalPrice:F2}"
        )
        .Border(BoxBorder.Double)
        .Padding(2,1); // horizontale en verticale ruimte binnen de panel

        AnsiConsole.Write(totalPanel);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey();
   }
}