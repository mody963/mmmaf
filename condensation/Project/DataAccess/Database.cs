using Npgsql;
 
public class Database
{
    private readonly string _connectionString;
 
    public Database(string connectionString)
    {
        _connectionString = connectionString;
    }
 
    public async Task TestConnectionAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            Console.WriteLine("Databaseverbinding gelukt.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fout bij verbinden:");
            Console.WriteLine(ex.ToString());
        }
    }
 
    public async Task EnsureReviewSchemaAsync()
    {
        string sql = @"
    ALTER TABLE reviews
    ADD COLUMN IF NOT EXISTS comment TEXT NOT NULL DEFAULT '';
 
    ALTER TABLE reviews
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ";
 
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
 
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}