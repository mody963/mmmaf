public class OrderLogic
{
    private readonly OrdersAccess _ordersAccess;

    public OrderLogic()
    {
        _ordersAccess = new OrdersAccess();
    }

    public OrderLogic(OrdersAccess ordersAccess)
    {
        _ordersAccess = ordersAccess;
    }

    public List<GameModel> GetOwnedGames(int customerId)
    {
        if (customerId <= 0)
            return new List<GameModel>();

        return _ordersAccess.GetOwnedGamesByCustomerId(customerId);
    }

    public bool HasPurchasedGame(int customerId, int gameId)
    {
        if (customerId <= 0 || gameId <= 0)
            return false;

        return _ordersAccess.HasPurchasedGame(customerId, gameId);
    }

    public List<OrderModel> GetOrderHistory(int customerId)
    {
        if (customerId <= 0)
            return new List<OrderModel>();

        return _ordersAccess.GetOrderHistoryByCustomerId(customerId);
    }

    public OrderModel? GetOrderDetails(int orderId)
    {
        if (orderId <= 0)
            return null;

        return _ordersAccess.GetOrderDetailsById(orderId);
    }
}