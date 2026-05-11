using Dapper;
using MongoDB.Driver;
using MongoDB.Bson;
using Npgsql;

public class OrdersAccess
{
    private string ConnectionString => AppConfig.PostgresConnectionString;
    private readonly IMongoCollection<BsonDocument> _ordersCollection;
    private int _nextOrderId = 1;

    public OrdersAccess()
    {
        var mongoDb = new MongoDb(AppConfig.MongoDbConnectionString, AppConfig.MongoDbDatabaseName);
        _ordersCollection = mongoDb.GetCollection<BsonDocument>("orders");
        
        var lastOrder = _ordersCollection
            .Find(new BsonDocument())
            .Sort(Builders<BsonDocument>.Sort.Descending("id"))
            .FirstOrDefault();
        
        if (lastOrder != null && lastOrder.Contains("id"))
        {
            _nextOrderId = lastOrder["id"].AsInt32 + 1;
        }
    }

    public int CreateOrder(int customerId, double totalPrice, List<CartModel> items)
    {
        int orderId = _nextOrderId++;

        var games = items.Select(item => new BsonDocument
        {
            { "gameId", item.id },
            { "gameTitle", item.Name },
            { "priceAtPurchase", item.Price }
        }).ToList();

        var orderDoc = new BsonDocument
        {
            { "id", orderId },
            { "customerId", customerId },
            { "orderDate", DateTime.UtcNow },
            { "totalPrice", totalPrice },
            { "games", new BsonArray(games) }
        };

        _ordersCollection.InsertOne(orderDoc);
        return orderId;
    }

    public List<GameModel> GetOwnedGamesByCustomerId(int customerId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("customerId", customerId);
        var orders = _ordersCollection.Find(filter).ToList();

        var ownedGames = new List<GameModel>();
        var seenGameIds = new HashSet<int>();

        foreach (var order in orders)
        {
            if (order.Contains("games") && order["games"].IsBsonArray)
            {
                var gamesArray = order["games"].AsBsonArray;
                foreach (var gameDoc in gamesArray)
                {
                    if (gameDoc.IsBsonDocument)
                    {
                        var game = gameDoc.AsBsonDocument;
                        if (game.Contains("gameId"))
                        {
                            int gameId = game["gameId"].AsInt32;
                            if (!seenGameIds.Contains(gameId))
                            {
                                seenGameIds.Add(gameId);
                                // Fetch game details from PostgreSQL
                                var gameModel = GetGameDetailsFromPostgres(gameId);
                                if (gameModel != null)
                                {
                                    ownedGames.Add(gameModel);
                                }
                            }
                        }
                    }
                }
            }
        }

        return ownedGames.OrderBy(g => g.Title).ToList();
    }

    public bool HasPurchasedGame(int customerId, int gameId)
    {
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("customerId", customerId),
            Builders<BsonDocument>.Filter.ElemMatch("games", 
                Builders<BsonDocument>.Filter.Eq("gameId", gameId))
        );

        return _ordersCollection.Find(filter).FirstOrDefault() != null;
    }

    public List<OrderModel> GetOrderHistoryByCustomerId(int customerId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("customerId", customerId);
        var orders = _ordersCollection
            .Find(filter)
            .Sort(Builders<BsonDocument>.Sort.Descending("orderDate"))
            .ToList();

        var orderModels = new List<OrderModel>();
        foreach (var doc in orders)
        {
            orderModels.Add(BsonDocumentToOrderModel(doc));
        }

        return orderModels;
    }

    public OrderModel? GetOrderDetailsById(int orderId)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("id", orderId);
        var orderDoc = _ordersCollection.Find(filter).FirstOrDefault();

        return orderDoc != null ? BsonDocumentToOrderModel(orderDoc) : null;
    }

    private OrderModel BsonDocumentToOrderModel(BsonDocument doc)
    {
        var order = new OrderModel
        {
            Id = doc["id"].AsInt32,
            CustomerId = doc["customerId"].AsInt32,
            OrderDate = doc["orderDate"].ToUniversalTime(),
            TotalPrice = doc["totalPrice"].AsDouble,
            Games = new List<OrderGameModel>()
        };

        if (doc.Contains("games") && doc["games"].IsBsonArray)
        {
            var gamesArray = doc["games"].AsBsonArray;
            foreach (var gameDoc in gamesArray)
            {
                if (gameDoc.IsBsonDocument)
                {
                    var game = gameDoc.AsBsonDocument;
                    order.Games.Add(new OrderGameModel
                    {
                        GameId = game["gameId"].AsInt32,
                        GameTitle = game["gameTitle"].AsString,
                        PriceAtPurchase = game["priceAtPurchase"].AsDouble
                    });
                }
            }
        }

        return order;
    }

    private GameModel? GetGameDetailsFromPostgres(int gameId)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        const string sql = "SELECT * FROM game WHERE id = @GameId";
        return connection.QueryFirstOrDefault<GameModel>(sql, new { GameId = gameId });
    }
}