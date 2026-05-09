using Microsoft.Extensions.Configuration;
using CondensationApp;
using System.Text;
using Project.Services;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;

// Fix euro sign
Console.OutputEncoding = Encoding.UTF8;

// Load config
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.local.json", optional: true)
    .Build();

string postgresConnectionString = config.GetConnectionString("DefaultConnection") ?? "";

if (string.IsNullOrWhiteSpace(postgresConnectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");

string redisConnectionString = config.GetConnectionString("RedisConnection") ?? "";

if (string.IsNullOrWhiteSpace(redisConnectionString))
    throw new InvalidOperationException("Connection string 'RedisConnection' is missing or empty.");

string mongoDbConnectionString = config.GetConnectionString("MongoDbConnection") ?? "";
string mongoDbDatabaseName = config["MongoDb:DatabaseName"] ?? "";

if (string.IsNullOrWhiteSpace(mongoDbConnectionString))
    throw new InvalidOperationException("Connection string 'MongoDbConnection' is missing or empty.");

if (string.IsNullOrWhiteSpace(mongoDbDatabaseName))
    throw new InvalidOperationException("MongoDb:DatabaseName is missing or empty.");

// Dapper turns game_id into GameId, but our properties are gameId, so we need to tell it to match names with underscores
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Init PostgreSQL
var db = new Database(postgresConnectionString);

await db.TestConnectionAsync();
await db.EnsureAnalyticsViewsAsync();
await db.EnsureReviewSchemaAsync();

// Init Redis
using var redisDb = new RedisDB(redisConnectionString);
await redisDb.TestConnectionAsync();

// Init MongoDB
var mongoDb = new MongoDb(
    mongoDbConnectionString,
    mongoDbDatabaseName
);

await mongoDb.TestConnectionAsync();

var uiSoundPlayer = new UiSoundPlayer(AppContext.BaseDirectory);
SoundEffects.Configure(uiSoundPlayer);

// Start app
MainMenu.Start();