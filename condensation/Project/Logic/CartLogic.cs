public class CartLogic
{

    public List<(int id, string Name, double Price)> games = new();

    public bool AddToCart(int id, string name, double price)
    {
        if (games.Any(g => g.id == id))
        {
            return false; 
        }
        else
        {
            games.Add((id, name, price));
            return true; 
        }
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
