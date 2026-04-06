using Dapper;
using Npgsql;

public class ReviewAccess : IReviewAccess
{
    private string _connectionString => AppConfig.ConnectionString;

    public List<GameModel> GetOwnedGamesByCustomerId(int customerId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = @"
            SELECT DISTINCT g.*
            FROM orders o
            JOIN order_games og ON og.order_id = o.id
            JOIN game g ON g.id = og.game_id
            WHERE o.customer_id = @CustomerId
            ORDER BY g.title;";

        return connection.Query<GameModel>(sql, new { CustomerId = customerId }).ToList();
    }

    public bool HasPurchasedGame(int customerId, int gameId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = @"
            SELECT EXISTS (
                SELECT 1
                FROM orders o
                JOIN order_games og ON og.order_id = o.id
                WHERE o.customer_id = @CustomerId
                  AND og.game_id = @GameId
            );";

        return connection.ExecuteScalar<bool>(sql, new { CustomerId = customerId, GameId = gameId });
    }

    public List<ReviewModel> GetReviewsForGame(int gameId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = @"
            SELECT
                r.id,
                r.game_id AS GameId,
                r.customer_id AS CustomerId,
                r.rating,
                r.comment,
                r.created_at AS CreatedAt,
                CONCAT(a.first_name, ' ', a.last_name) AS ReviewerName
            FROM reviews r
            JOIN customer c ON c.id = r.customer_id
            JOIN account a ON a.id = c.account_id
            WHERE r.game_id = @GameId
            ORDER BY r.created_at DESC, r.id DESC;";

        return connection.Query<ReviewModel>(sql, new { GameId = gameId }).ToList();
    }

    public ReviewModel? GetCustomerReviewForGame(int customerId, int gameId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = @"
            SELECT
                r.id,
                r.game_id AS GameId,
                r.customer_id AS CustomerId,
                r.rating,
                r.comment,
                r.created_at AS CreatedAt,
                CONCAT(a.first_name, ' ', a.last_name) AS ReviewerName
            FROM reviews r
            JOIN customer c ON c.id = r.customer_id
            JOIN account a ON a.id = c.account_id
            WHERE r.customer_id = @CustomerId
              AND r.game_id = @GameId
            LIMIT 1;";

        return connection.QueryFirstOrDefault<ReviewModel>(sql, new { CustomerId = customerId, GameId = gameId });
    }

    public void UpsertReview(ReviewModel review)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO reviews (game_id, customer_id, rating, comment)
            VALUES (@GameId, @CustomerId, @Rating, @Comment)
            ON CONFLICT (game_id, customer_id)
            DO UPDATE SET
                rating = EXCLUDED.rating,
                comment = EXCLUDED.comment,
                created_at = CURRENT_TIMESTAMP;";

        connection.Execute(sql, review);
    }


    public List<ReviewModel> GetReviewsByPublisherId(int publisherId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        const string sql = @"
            SELECT 
                r.id, 
                r.game_id AS GameId, 
                r.customer_id AS CustomerId, 
                r.rating, 
                r.comment, 
                r.created_at AS CreatedAt,
                g.title AS GameTitle, -- Handig om de gamenaam erbij te hebben
                CONCAT(a.first_name, ' ', a.last_name) AS ReviewerName
            FROM reviews r
            JOIN game g ON g.id = r.game_id
            JOIN customer c ON c.id = r.customer_id
            JOIN account a ON a.id = c.account_id
            WHERE g.publisher_id = @PublisherId
            ORDER BY r.created_at DESC;";

        return connection.Query<ReviewModel>(sql, new { PublisherId = publisherId }).ToList();
    }
}
