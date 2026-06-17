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
}
