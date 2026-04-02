using Microsoft.Extensions.Configuration;
using CondensationApp;
using System.Text;
using Project.Services;

// Fix euro sign
Console.OutputEncoding = Encoding.UTF8;

// Load config
var config = new ConfigurationBuilder() // a configuration builder is used to load json's in our project
    .SetBasePath(AppContext.BaseDirectory) // tells the builder where to look for the json files, in this case the base directory of the app    
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.local.json", optional: true)
    .Build();

string connectionString = config.GetConnectionString("DefaultConnection") ?? "";

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");

AppConfig.ConnectionString = connectionString;

// Dapper turns game_id into GameId, but our properties are gameId, so we need to tell it to match names with underscores
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

// Init DB
var db = new Database(connectionString);

await db.TestConnectionAsync();
await db.EnsureAnalyticsViewsAsync();

var uiSoundPlayer = new UiSoundPlayer(AppContext.BaseDirectory);
SoundEffects.Configure(uiSoundPlayer);

// Start app
MainMenu.Start();