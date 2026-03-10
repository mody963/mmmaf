using Spectre.Console;

public class Cart
{
    private readonly CartLogic _cartLogic = new CartLogic();

    public void AddToCart(int id, string name, double price)
    {
        _cartLogic.AddToCart(id, name, price);
    }

    public void RemoveFromCart(int id)
    {
        _cartLogic.RemoveFromCart(id);
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



    public void ShowCart()
    {
        Console.Clear();

        var items = GetCartItems();
        var totalPrice = GetTotalPrice();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold yellow]🛒 Your Shopping Cart[/]")
            .AddColumn("[cyan]ID[/]")
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
        .Header("[yellow]Summary[/]")
        .Padding(2,1); // horizontale en verticale ruimte binnen de panel

        AnsiConsole.Write(totalPanel);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey();
    }
    }