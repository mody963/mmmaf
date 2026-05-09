public class OrderLogic
{
    private readonly OrdersAccess _ordersAccess;
    private readonly OrderDocumentAccess _orderDocumentAccess;

    public OrderLogic()
    {
        _ordersAccess = new OrdersAccess();
        if (AppConfig.MongoDb != null)
        {
            _orderDocumentAccess = new OrderDocumentAccess(AppConfig.MongoDb);
        }
    }

    public OrderLogic(OrdersAccess ordersAccess)
    {
        _ordersAccess = ordersAccess;
    }

    public OrderLogic(OrdersAccess ordersAccess, OrderDocumentAccess orderDocumentAccess)
    {
        _ordersAccess = ordersAccess;
        _orderDocumentAccess = orderDocumentAccess;
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

    public async Task<(int orderId, string orderNumber)> CreateOrderWithDocumentAsync(
        int customerId,
        double totalPrice,
        List<CartModel> items,
        List<GameModel> gameModels,
        string shippingAddress)
    {
        if (customerId <= 0 || items.Count == 0)
            throw new ArgumentException("Invalid customer or empty cart");

        if (_orderDocumentAccess == null)
            throw new InvalidOperationException("OrderDocumentAccess not initialized");

        int orderId = _ordersAccess.CreateOrder(customerId, totalPrice, items);
        string orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{orderId}";
        var orderDoc = new OrderDocumentModel(customerId, orderNumber, shippingAddress, totalPrice);

        foreach (var cartItem in items)
        {
            var gameModel = gameModels.FirstOrDefault(g => g.Id == cartItem.id);
            if (gameModel != null)
            {
                orderDoc.Items.Add(new OrderItemModel(
                    cartItem.id,
                    gameModel.Title,
                    cartItem.Price,
                    1
                ));
            }
        }

        orderDoc.StatusHistory.Add(new OrderStatusHistoryModel("Created", DateTime.Now));

        try
        {
            await _orderDocumentAccess.CreateOrderDocumentAsync(orderDoc);
        }
        catch (Exception mongoEx)
        {
            Console.WriteLine($"Warning: Order created in PostgreSQL (ID: {orderId}), but MongoDB save failed: {mongoEx.Message}");
        }

        return (orderId, orderNumber);
    }

    public async Task<OrderDocumentModel> GetOrderDocumentAsync(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return null;

        if (_orderDocumentAccess == null)
            throw new InvalidOperationException("OrderDocumentAccess not initialized");

        return await _orderDocumentAccess.GetOrderDocumentByNumberAsync(orderNumber);
    }

    public async Task<List<OrderDocumentModel>> GetCustomerOrderDocumentsAsync(int customerId)
    {
        if (customerId <= 0)
            return new List<OrderDocumentModel>();

        if (_orderDocumentAccess == null)
            throw new InvalidOperationException("OrderDocumentAccess not initialized");

        return await _orderDocumentAccess.GetOrderDocumentsByCustomerIdAsync(customerId);
    }

    public async Task<bool> UpdateOrderStatusAsync(string orderNumber, string newStatus)
    {
        if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(newStatus))
            return false;

        if (_orderDocumentAccess == null)
            throw new InvalidOperationException("OrderDocumentAccess not initialized");

        return await _orderDocumentAccess.UpdateOrderStatusAsync(orderNumber, newStatus);
    }
}