using Npgsql;
using Dapper;

public class PublisherAccess
{
    private string _connectionString => AppConfig.ConnectionString;

    public int CreatePublisher(PublisherModel publisher)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = @"
            INSERT INTO publisher (account_id, studio_name, amount_of_games)
            VALUES (@AccountId, @StudioName, @AmountOfGames)
            RETURNING id;";
        return connection.ExecuteScalar<int>(sql, publisher);
    }

    public bool StudioNameExists(string studioName)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        // ILIKE does a case-insensitive check
        string sql = "SELECT COUNT(1) FROM publisher WHERE studio_name ILIKE @StudioName";
        int count = connection.ExecuteScalar<int>(sql, new { StudioName = studioName });
        
        return count > 0;
    }
    public PublisherModel? GetByAccountId(int accountId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = "SELECT * FROM publisher WHERE account_id = @AccountId";
        return connection.QueryFirstOrDefault<PublisherModel>(sql, new { AccountId = accountId });
    }
}