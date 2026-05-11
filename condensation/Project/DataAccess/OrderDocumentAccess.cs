using MongoDB.Bson;
using MongoDB.Driver;

public class OrderDocumentAccess
{
    private readonly MongoDb _mongoDb;
    private readonly string _collectionName = "orders";

    public OrderDocumentAccess(MongoDb mongoDb)
    {
        _mongoDb = mongoDb;
    }

    public async Task<string> CreateOrderDocumentAsync(OrderDocumentModel orderDocument)
    {
        try
        {
            var collection = _mongoDb.GetCollection<OrderDocumentModel>(_collectionName);
            await collection.InsertOneAsync(orderDocument);
            return orderDocument.OrderNumber;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij aanmaken orderdocument: {ex.Message}");
            throw;
        }
    }

    public async Task<OrderDocumentModel> GetOrderDocumentByNumberAsync(string orderNumber)
    {
        try
        {
            var collection = _mongoDb.GetCollection<OrderDocumentModel>(_collectionName);
            var filter = Builders<OrderDocumentModel>.Filter.Eq(o => o.OrderNumber, orderNumber);
            return await collection.Find(filter).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij ophalen orderdocument: {ex.Message}");
            throw;
        }
    }

    public async Task<List<OrderDocumentModel>> GetOrderDocumentsByCustomerIdAsync(int customerId)
    {
        try
        {
            var collection = _mongoDb.GetCollection<OrderDocumentModel>(_collectionName);
            var filter = Builders<OrderDocumentModel>.Filter.Eq(o => o.CustomerId, customerId);
            return await collection.Find(filter).SortByDescending(o => o.OrderDate).ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij ophalen klantorders: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(string orderNumber, string newStatus)
    {
        try
        {
            var collection = _mongoDb.GetCollection<OrderDocumentModel>(_collectionName);
            var filter = Builders<OrderDocumentModel>.Filter.Eq(o => o.OrderNumber, orderNumber);

            var statusHistoryEntry = new OrderStatusHistoryModel(newStatus, DateTime.Now);

            var update = Builders<OrderDocumentModel>.Update
                .Set(o => o.OrderStatus, newStatus)
                .Push(o => o.StatusHistory, statusHistoryEntry);

            var result = await collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij bijwerken orderstatus: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdatePaymentStatusAsync(string orderNumber, string newPaymentStatus)
    {
        try
        {
            var collection = _mongoDb.GetCollection<OrderDocumentModel>(_collectionName);
            var filter = Builders<OrderDocumentModel>.Filter.Eq(o => o.OrderNumber, orderNumber);

            var update = Builders<OrderDocumentModel>.Update.Set(o => o.PaymentStatus, newPaymentStatus);

            var result = await collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij bijwerken betaalstatus: {ex.Message}");
            throw;
        }
    }

    public async Task<List<OrderDocumentModel>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var collection = _mongoDb.GetCollection<OrderDocumentModel>(_collectionName);
            var filter = Builders<OrderDocumentModel>.Filter.And(
                Builders<OrderDocumentModel>.Filter.Gte(o => o.OrderDate, startDate),
                Builders<OrderDocumentModel>.Filter.Lte(o => o.OrderDate, endDate)
            );
            return await collection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij ophalen orders per datumrange: {ex.Message}");
            throw;
        }
    }
}
