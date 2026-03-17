public class CartModel
{
    public int id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.Now;

    public CartModel(int id, string name, double price)
    {
        this.id = id;
        this.Name = name;
        this.Price = price;
    }
}