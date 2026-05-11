using MongoDB.Bson;
using MongoDB.Driver;

public class MongoDb
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _database;

    public MongoDb(string connectionString, string databaseName)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);

        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
        settings.ConnectTimeout = TimeSpan.FromSeconds(5);
        settings.SocketTimeout = TimeSpan.FromSeconds(5);

        _client = new MongoClient(settings);
        _database = _client.GetDatabase(databaseName);
    }

    public IMongoDatabase Database => _database;

    public async Task TestConnectionAsync()
    {
        try
        {
            await _database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
            Console.WriteLine("MongoDB verbinding gelukt.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fout bij verbinden met MongoDB:");
            Console.WriteLine(ex.ToString());
        }
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
}