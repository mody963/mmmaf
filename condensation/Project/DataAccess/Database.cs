using Npgsql;

namespace CondensationApp;

public class Database
{
    private readonly string _connectionString;

    public Database(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task TestConnectionAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        Console.WriteLine("Databaseverbinding gelukt.");
    }

    public async Task RunVersionQueryAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = "SELECT version();";

        await using var cmd = new NpgsqlCommand(sql, conn);
        var result = await cmd.ExecuteScalarAsync();

        Console.WriteLine("PostgreSQL versie:");
        Console.WriteLine(result?.ToString());
    }
}