using System;
using System.Threading;
using Spectre.Console;
using Microsoft.Extensions.Configuration;
using CondensationApp;

// 1. DATABASE & CONFIG SETUP
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string? connString = config.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connString))
{
    AnsiConsole.MarkupLine("[red]Error: Connection string 'DefaultConnection' is missing in appsettings.json![/]");
    return;
}
AppConfig.ConnectionString = connString;
// Initialize your data access classes
var db = new Database(connString);

try
{
    // Test the connection before starting the UI
    await db.TestConnectionAsync();
    await db.RunVersionQueryAsync();
    AnsiConsole.MarkupLine("[green]Database connection successful![/]");
    Thread.Sleep(1500);
    Console.Clear();
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Error connecting to database: {ex.Message}[/]");
    AnsiConsole.MarkupLine("[yellow]Press any key to exit...[/]");
    Console.ReadKey();
    return;
}

// 2. LAUNCH THE APP
// Hand over control to the MainMenu, passing along the database logic!
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
MainMenu.Start();