using Project.Services;

public class CheckoutLogic
{
    private readonly OrdersAccess _ordersAccess = new OrdersAccess();
    private readonly OrderLogic _orderLogic = new OrderLogic();
    private readonly GameLogic _gameLogic = new GameLogic();

    public int Checkout(int customerId, double totalPrice, List<CartModel> items)
    {
        if (customerId <= 0)
            throw new ArgumentException("Invalid customer id.");

        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        // Create order in PostgreSQL and MongoDB
        var gameModels = _gameLogic.GetAllGames();
        var (orderId, orderNumber) = _orderLogic.CreateOrderWithDocumentAsync(
            customerId,
            totalPrice,
            items,
            gameModels,
            "Not specified"
        ).Result;

        return orderId;
    }
}