public class CartLogic
{

    public List<CartModel> games = new();

    public bool AddToCart(int id, string name, double price)
    {
        if (games.Any(g => g.id == id))
        {
            return false; 
        }
        else
        {
            games.Add(new CartModel(id, name, price));
            return true; 
        }
    }

    public void ClearCart()
    {
        games.Clear();
    }

    public List<CartModel> GetCartItems()
    {
        return games;
    }

    public void RemoveFromCart(int id)
    {
        games.RemoveAll(g => g.id == id); // meteen alle items met die id worden verwijdert. 
    }


    public double GetTotalPrice()
    {
        return games.Sum(g => g.Price);
    }


}
