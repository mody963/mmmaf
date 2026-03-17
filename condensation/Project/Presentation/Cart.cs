using System.Runtime.CompilerServices;
using Spectre.Console;

public class Cart
{
    private readonly CartLogic _cartLogic = new CartLogic();
    private static readonly string _backOption = Texts.Get("Cart_Back");

    public void AddToCart(int id, string name, double price)
    {
        if (!_cartLogic.AddToCart(id, name, price))
        {
            AnsiConsole.MarkupLine($"[red]{Texts.Get("Cart_ItemAlreadyInCart")} {id} {Texts.Get("Cart_ItemAlreadyInCartEnd")}[/]");
            Console.ReadKey(true);
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]{Texts.Get("Cart_ItemAddedToCart")} {name} {Texts.Get("Cart_ItemAddedToCartEnd")}{price:F2}.[/]");
            SoundEffects.PlayKaching();
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
        .Title($"[bold yellow]{Texts.Get("Cart_SelectOption")}[/]")
        .AddChoices(Texts.Get("Cart_View"), Texts.Get("Cart_RemoveItem"), Texts.Get("Cart_ClearCart"), _backOption)
        .HighlightStyle(new Style(foreground: Color.Green)));
        SoundEffects.PlayMenuClick();

        switch (optie)
        {
            case var c when c == Texts.Get("Cart_View"):
                ShowCart();
                break;
            case var c when c == Texts.Get("Cart_RemoveItem"):
                var items = GetCartItems();

                if (items.Count == 0)
                {
                    AnsiConsole.MarkupLine($"[red]{Texts.Get("Cart_IsEmpty")}[/]");
                    Console.ReadKey(true);
                    break;
                }

                var selectedItem = AnsiConsole.Prompt(
                    new SelectionPrompt<CartModel>()
                        .Title($"[yellow]{Texts.Get("Cart_SelectItemToRemove")}[/]")
                        .UseConverter(item => $"{item.Name} - €{item.Price:F2}")
                        .AddChoices(items)
                        .HighlightStyle(new Style(foreground: Color.Red))
                );
                SoundEffects.PlayMenuClick();

                RemoveFromCart(selectedItem.Name);

                AnsiConsole.MarkupLine($"[green]{selectedItem.Name} {Texts.Get("Cart_RemovedFromCart")}[/]");
                Console.ReadKey(true);

                break;
            case var c when c == Texts.Get("Cart_ClearCart"):
                ClearCart();
                AnsiConsole.MarkupLine($"[green]{Texts.Get("Cart_Cleared")}[/]");
                Console.ReadKey(true);
                break;
            case var c when c == _backOption:
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
            AnsiConsole.MarkupLine($"[red]{Texts.Get("Cart_YourCartIsEmpty")}[/]");
            AnsiConsole.MarkupLine($"[grey]{Texts.Get("Press_Any_Key_To_Return")}[/]");
            Console.ReadKey(true);
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title($"[bold yellow]{Texts.Get("Cart_Title")}[/]")
            .AddColumn($"[cyan]{Texts.Get("Cart_Product")}[/]")
            .AddColumn($"[cyan]{Texts.Get("Price_Without_E.g")}[/]")
            .AddColumn($"[cyan]{Texts.Get("Cart_PurchaseDate")}[/]");

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
            AnsiConsole.MarkupLine($"[red]{Texts.Get("Cart_YourCartIsEmpty")}[/]");
            return;
        }

        AnsiConsole.Write(table);

        AnsiConsole.WriteLine();

        var totalPanel = new Panel(
            $"[bold green]{Texts.Get("Cart_TotalPrice")}[/] €{totalPrice:F2}"
        )
        .Border(BoxBorder.Double)
        .Padding(2, 1);

        AnsiConsole.Write(totalPanel);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]{Texts.Get("Cart_PressAnyKeyToContinue")}[/]");
        Console.ReadKey();
    }
}