using StackExchange.Redis;

namespace CondensationApp;

public class RedisDB : IDisposable
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisDB(string connectionString)
    {
        _redis = ConnectionMultiplexer.Connect(connectionString);
        _db = _redis.GetDatabase();
    }

    public async Task TestConnectionAsync()
    {
        try
        {
            var ping = await _db.PingAsync();
            Console.WriteLine($"Redis verbinding gelukt.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fout bij verbinden met Redis:");
            Console.WriteLine(ex.ToString());
        }
    }

    public void Dispose()
    {
        _redis.Dispose();
    }
}