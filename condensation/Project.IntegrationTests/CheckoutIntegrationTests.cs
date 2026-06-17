public class CheckoutIntegrationTests
{
    private readonly CheckoutLogic _checkoutLogic;
    private readonly OrdersAccess _ordersAccess;
    private readonly OrderDocumentAccess _orderDocumentAccess;
    private readonly GameLogic _gameLogic;

    public CheckoutIntegrationTests()
    {
        AppConfig.PostgresConnectionString = "Host=localhost;Port=5433;Database=Condensation;Username=mouhamad;Password=dei2Kaish4dooquiepei";
        AppConfig.MongoDbConnectionString = "mongodb://hro_gebruiker:hetismongodb@pg-hro.crazyelectron.io:27017/?authsource=admin";
        AppConfig.MongoDbDatabaseName = "Condensation";

        _ordersAccess = new OrdersAccess();

        var mongo = new MongoDb(
            AppConfig.MongoDbConnectionString,
            AppConfig.MongoDbDatabaseName
        );

        _orderDocumentAccess = new OrderDocumentAccess(mongo);
        _checkoutLogic = new CheckoutLogic();
        _gameLogic = new GameLogic();
    }
    [Fact]
    public void Checkout_CreatesOrder_InPreviousOrders()
    {
        int customerId = 9999;
        double totalPrice = 59.99;

        var items = new List<CartModel>
        {
            new CartModel(1, "Test Game", 59.99)
        };

        int orderId = _checkoutLogic.Checkout(customerId, totalPrice, items);

        var history = _ordersAccess.GetOrderHistoryByCustomerId(customerId);

        Assert.Contains(history, o => o.Id == orderId);

        // Cleanup
        _ordersAccess.DeleteOrder(orderId);
    }
    [Fact]
    public async Task Checkout_CreatesOrderDocument()
    {
        int customerId = 9999;
        double totalPrice = 59.99;

        var items = new List<CartModel>
        {
            new CartModel(1, "Test Game", 59.99)
        };

        int orderId = _checkoutLogic.Checkout(customerId, totalPrice, items);

        string orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{orderId}";

        var doc = await _orderDocumentAccess.GetOrderDocumentByNumberAsync(orderNumber);

        Assert.NotNull(doc);

        // Cleanup
        _ordersAccess.DeleteOrder(orderId);
        await _orderDocumentAccess.DeleteOrderDocumentAsync(orderNumber);
    }
    [Fact]
    public async Task Checkout_AddsCorrectItems_ToOrderDocument()
    {
        int customerId = 9999;

        var items = new List<CartModel>
        {
            new CartModel(1, "Test Game", 59.99)
        };

        int orderId = _checkoutLogic.Checkout(customerId, 59.99, items);
        string orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{orderId}";

        var doc = await _orderDocumentAccess.GetOrderDocumentByNumberAsync(orderNumber);

        Assert.Single(doc.Items);
        Assert.Equal(1, doc.Items[0].GameId);

        // Cleanup
        _ordersAccess.DeleteOrder(orderId);
        await _orderDocumentAccess.DeleteOrderDocumentAsync(orderNumber);
    }
    [Fact]
    public void Checkout_AddsGame_ToOwnedGames()
    {
        int customerId = 9999;

        var items = new List<CartModel>
        {
            new CartModel(1, "Test Game", 59.99)
        };

        int orderId = _checkoutLogic.Checkout(customerId, 59.99, items);

        var owned = _ordersAccess.GetOwnedGamesByCustomerId(customerId);

        Assert.Contains(owned, g => g.Id == 1);

        // Cleanup
        _ordersAccess.DeleteOrder(orderId);
    }
    [Fact]
    public async Task Checkout_SetsInitialStatus_AndHistory()
    {
        int customerId = 9999;

        var items = new List<CartModel>
        {
            new CartModel(1, "Test Game", 59.99)
        };

        int orderId = _checkoutLogic.Checkout(customerId, 59.99, items);
        string orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{orderId}";

        var doc = await _orderDocumentAccess.GetOrderDocumentByNumberAsync(orderNumber);

        Assert.Equal("Created", doc.OrderStatus);
        Assert.NotEmpty(doc.StatusHistory);

        // Cleanup
        _ordersAccess.DeleteOrder(orderId);
        await _orderDocumentAccess.DeleteOrderDocumentAsync(orderNumber);
    }
    [Fact]
    public async Task Checkout_FullFlow_Works()
    {
        int customerId = 9999;

        var items = new List<CartModel>
        {
            new CartModel(1, "Test Game", 59.99)
        };

        int orderId = _checkoutLogic.Checkout(customerId, 59.99, items);
        string orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{orderId}";

        var history = _ordersAccess.GetOrderHistoryByCustomerId(customerId);
        var doc = await _orderDocumentAccess.GetOrderDocumentByNumberAsync(orderNumber);
        var owned = _ordersAccess.GetOwnedGamesByCustomerId(customerId);

        Assert.Contains(history, o => o.Id == orderId);
        Assert.NotNull(doc);
        Assert.Contains(owned, g => g.Id == 1);

        // Cleanup
        _ordersAccess.DeleteOrder(orderId);
        await _orderDocumentAccess.DeleteOrderDocumentAsync(orderNumber);
    }

}
