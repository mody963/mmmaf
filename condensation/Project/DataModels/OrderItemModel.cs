public class OrderItemModel
{
    public int GameId { get; set; }
    public string GameName { get; set; }
    public double PriceAtPurchase { get; set; }
    public int Quantity { get; set; }

    public OrderItemModel(int gameId, string gameName, double priceAtPurchase, int quantity)
    {
        GameId = gameId;
        GameName = gameName;
        PriceAtPurchase = priceAtPurchase;
        Quantity = quantity;
    }
}
