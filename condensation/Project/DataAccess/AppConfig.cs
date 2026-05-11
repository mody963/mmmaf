public static class AppConfig
{
    // This will hold the connection string for the whole app
    public static string PostgresConnectionString { get; set; } = "";
    public static string RedisConnectionString { get; set; } = "";

    public static string MongoDbConnectionString { get; set; } = "";
    public static string MongoDbDatabaseName { get; set; } = "";
    public static MongoDb? MongoDb { get; set; }
}