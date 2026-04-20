using StackExchange.Redis;
using System.Text.Json;

public static class RedisService
{
    private static IConnectionMultiplexer? _connection;
    private static IDatabase? _db;

    public static void Initialize(string redisConnectionString)
    {
        if (_connection != null && _connection.IsConnected)
            return;

        try
        {
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 5000;
            options.SyncTimeout = 5000;

            _connection = ConnectionMultiplexer.Connect(options);
            _db = _connection.GetDatabase();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to connect to Redis: {ex.Message}");
            _connection = null;
            _db = null;
        }
    }

    public static bool IsConnected => _connection?.IsConnected ?? false;

    public static void Set<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (_db == null)
            return;

        try
        {
            var json = JsonSerializer.Serialize(value);
            _db.StringSet(key, json, expiry);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis Set Error: {ex.Message}");
        }
    }

    public static T? Get<T>(string key)
    {
        if (_db == null)
            return default;

        try
        {
            var value = _db.StringGet(key);
            if (!value.HasValue)
                return default;

            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis Get Error: {ex.Message}");
            return default;
        }
    }

    public static void Remove(string key)
    {
        if (_db == null)
            return;

        try
        {
            _db.KeyDelete(key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis Remove Error: {ex.Message}");
        }
    }

    public static void RemoveByPattern(string pattern)
    {
        if (_connection == null)
            return;

        try
        {
            var endpoints = _connection.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _connection.GetServer(endpoint);
                var keys = server.Keys(pattern: pattern).ToArray();
                if (keys.Length > 0)
                {
                    _db?.KeyDelete(keys);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis RemoveByPattern Error: {ex.Message}");
        }
    }
}
