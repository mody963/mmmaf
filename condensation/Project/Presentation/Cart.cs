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

    public double GetTotalPrice()
    {
        return _cartLogic.GetTotalPrice();
    }

    public void ShowCart()
    {
        Console.Clear();

        
    }
}