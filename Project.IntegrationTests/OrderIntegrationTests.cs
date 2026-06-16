public class OrderIntegrationTests
{
    private readonly OrdersAccess _ordersAccess;
    private readonly OrderDocumentAccess _orderDocumentAccess;
    private readonly OrderLogic _logic;

    public OrderIntegrationTests()
    {
        // Gebruik jouw echte configuratie
        _ordersAccess = new OrdersAccess();
        _orderDocumentAccess = new OrderDocumentAccess(AppConfig.MongoDb);

        _logic = new OrderLogic(_ordersAccess, _orderDocumentAccess);
    }

    [Fact]
    public async Task CreateOrder_FetchOrder_Cleanup_Succeeds()
    {
        // Arrange
        int customerId = 99999; // Test user
        double totalPrice = 49.99;

        var items = new List<CartModel>
        {
            new CartModel { id = 1, Price = 49.99 }
        };

        var games = new List<GameModel>
        {
            new GameModel { Id = 1, Title = "Test Game" }
        };

        string shippingAddress = "Teststraat 123";

        // Act — create order
        var (orderId, orderNumber) = await _logic.CreateOrderWithDocumentAsync(
            customerId,
            totalPrice,
            items,
            games,
            shippingAddress
        );

        // Assert — PostgreSQL order exists
        var history = _logic.GetOrderHistory(customerId);
        Assert.Contains(history, o => o.Id == orderId);

        // Assert — MongoDB document exists
        var doc = await _logic.GetOrderDocumentAsync(orderNumber);
        Assert.NotNull(doc);
        Assert.Equal(orderNumber, doc.OrderNumber);

        // Cleanup
        bool pgDeleted = _ordersAccess.DeleteOrder(orderId);
        bool mongoDeleted = await _orderDocumentAccess.DeleteOrderDocumentAsync(orderNumber);

        Assert.True(pgDeleted);
        Assert.True(mongoDeleted);
    }
}