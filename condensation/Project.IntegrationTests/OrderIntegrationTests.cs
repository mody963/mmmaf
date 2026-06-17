using Xunit;
using System;
using System.Collections.Generic;

public class OrderIntegrationTests
{
    private readonly OrdersAccess _ordersAccess;
    private readonly OrderDocumentAccess _orderDocumentAccess;

    public OrderIntegrationTests()
    {
        // Inject both connection strings so the test can log in
        AppConfig.PostgresConnectionString = "Host=localhost;Port=5433;Database=Condensation;Username=mouhamad;Password=dei2Kaish4dooquiepei";

        AppConfig.MongoDbConnectionString = "mongodb://hro_gebruiker:hetismongodb@pg-hro.crazyelectron.io:27017/?authsource=admin";
        AppConfig.MongoDbDatabaseName = "Condensation";

        _ordersAccess = new OrdersAccess();

        var mongo = new MongoDb(
            AppConfig.MongoDbConnectionString,
            AppConfig.MongoDbDatabaseName
        );

        _orderDocumentAccess = new OrderDocumentAccess(mongo);
    }

    [Fact]
    public async Task Create_View_Delete_Order_FullFlow_Works()
    {
        // Arrange
        int customerId = 9999;
        double totalPrice = 59.99;

        var items = new List<CartModel>
        {
            new CartModel(1, "Test Game", 59.99)
        };

        // Act 1 — Create order
        int orderId = _ordersAccess.CreateOrder(customerId, totalPrice, items);

        // Act 2 — Create order document
        var orderDoc = new OrderDocumentModel
        {
            OrderNumber = $"TEST-{Guid.NewGuid()}",
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            OrderStatus = "Created",
            PaymentStatus = "Pending",
            StatusHistory = new List<OrderStatusHistoryModel>()
        };

        await _orderDocumentAccess.CreateOrderDocumentAsync(orderDoc);

        // Act 3 — Fetch order + document
        var orderHistory = _ordersAccess.GetOrderHistoryByCustomerId(customerId);
        var fetchedDoc = await _orderDocumentAccess.GetOrderDocumentByNumberAsync(orderDoc.OrderNumber);

        // Assert
        Assert.NotEmpty(orderHistory);
        Assert.NotNull(fetchedDoc);

        // Cleanup
        _ordersAccess.DeleteOrder(orderId);
        await _orderDocumentAccess.DeleteOrderDocumentAsync(orderDoc.OrderNumber);
    }

    [Fact]
    public void CreateOrder_ThenRetrieveHistory_Works()
    {
        int customerId = 9999;
        double totalPrice = 59.99;

        var items = new List<CartModel>
        {
            new CartModel(1, "Test Game", 59.99)
        };

        int orderId = _ordersAccess.CreateOrder(customerId, totalPrice, items);

        var history = _ordersAccess.GetOrderHistoryByCustomerId(customerId);

        Assert.NotEmpty(history);
        Assert.Contains(history, o => o.Id == orderId);

        // Cleanup
        _ordersAccess.DeleteOrder(orderId);
    }

    [Fact]
    public async Task CreateOrderDocument_ThenRetrieve_Works()
    {
        string orderNumber = $"TEST-{Guid.NewGuid()}";

        var doc = new OrderDocumentModel
        {
            OrderNumber = orderNumber,
            CustomerId = 9999,
            OrderDate = DateTime.UtcNow,
            OrderStatus = "Created",
            PaymentStatus = "Pending",
            StatusHistory = new List<OrderStatusHistoryModel>()
        };

        await _orderDocumentAccess.CreateOrderDocumentAsync(doc);

        var fetched = await _orderDocumentAccess.GetOrderDocumentByNumberAsync(orderNumber);

        Assert.NotNull(fetched);
        Assert.Equal(orderNumber, fetched.OrderNumber);

        // Cleanup
        await _orderDocumentAccess.DeleteOrderDocumentAsync(orderNumber);
    }
    [Fact]
    public void HasPurchasedGame_ReturnsTrue_WhenGameWasBought()
    {
        int customerId = 9999;
        int gameId = 1;

        var items = new List<CartModel>
        {
            new CartModel(gameId, "Test Game", 59.99)
        };

        int orderId = _ordersAccess.CreateOrder(customerId, 59.99, items);

        bool result = _ordersAccess.HasPurchasedGame(customerId, gameId);

        Assert.True(result);

        // Cleanup
        _ordersAccess.DeleteOrder(orderId);
    }
    [Fact]
    public async Task UpdateOrderStatus_And_PaymentStatus_Works()
    {
        string orderNumber = $"TEST-{Guid.NewGuid()}";

        var doc = new OrderDocumentModel
        {
            OrderNumber = orderNumber,
            CustomerId = 9999,
            OrderDate = DateTime.UtcNow,
            OrderStatus = "Created",
            PaymentStatus = "Pending",
            StatusHistory = new List<OrderStatusHistoryModel>()
        };

        await _orderDocumentAccess.CreateOrderDocumentAsync(doc);

        await _orderDocumentAccess.UpdateOrderStatusAsync(orderNumber, "Shipped");
        await _orderDocumentAccess.UpdatePaymentStatusAsync(orderNumber, "Paid");

        var updated = await _orderDocumentAccess.GetOrderDocumentByNumberAsync(orderNumber);

        Assert.Equal("Shipped", updated.OrderStatus);
        Assert.Equal("Paid", updated.PaymentStatus);
        Assert.NotEmpty(updated.StatusHistory);

        // Cleanup
        await _orderDocumentAccess.DeleteOrderDocumentAsync(orderNumber);
    }
}
